using ARGI.DAL.DTO.Response;
using ARGI.DAL.Repository;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace ARGI.BLL.Service
{
    public class AiInsightService : IAiInsightService
    {
        private readonly IDomeRepository _domeRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AiInsightService(
            IDomeRepository domeRepository,
            ISensorRepository sensorRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _domeRepository = domeRepository;
            _sensorRepository = sensorRepository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<AiInsightDto>> GenerateInsightsAsync(int domeId, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId)
                return new List<AiInsightDto>();

            var readings = (await _sensorRepository.GetDomeHistoryAsync(domeId, hours: 24)).ToList();
            if (!readings.Any())
                return new List<AiInsightDto> { new AiInsightDto { Title = "لا توجد بيانات", Description = "لم يتم استقبال قراءات خلال الـ 24 ساعة الماضية.", Category = "Maintenance" } };

            var latest = readings.Last();
            var avgMoisture = readings.Average(r => r.SoilMoisture);
            var avgTemp = readings.Average(r => r.Temperature);

            var prompt = $@"أنت نظام ذكاء اصطناعي متخصص في الزراعة الذكية. حلل البيانات التالية وأعطني 3-4 توصيات عملية.

معلومات المزرعة:
- نوع النبات: {dome.PlantType ?? "غير محدد"}
- نوع التربة: {dome.SoilType ?? "غير محدد"}
- المنطقة: {dome.Country} - {dome.Governorate} - {dome.Neighborhood}
- ⭐ الحد الأدنى للرطوبة الذي حدده المالك: {dome.MinTargetMoisture}% (هذا هو الحد المعتمد للقرار — احترمه دائماً)
- النطاق المثالي للرطوبة (مرجعي): {dome.OptimalMoistureMin}% - {dome.OptimalMoistureMax}%
- النطاق المثالي لدرجة الحرارة: {dome.OptimalTempMin}°C - {dome.OptimalTempMax}°C
- النطاق المثالي لشدة الضوء: {dome.OptimalLightMin:F0} - {dome.OptimalLightMax:F0} lux

آخر قراءة:
- رطوبة التربة: {latest.SoilMoisture:F1}%
- درجة الحرارة: {latest.Temperature:F1}°C
- الرطوبة الجوية: {latest.Humidity:F1}%
- شدة الضوء: {latest.LightIntensity:F0} lux

متوسط آخر 24 ساعة:
- متوسط رطوبة التربة: {avgMoisture:F1}%
- متوسط درجة الحرارة: {avgTemp:F1}°C

أجب بصيغة JSON فقط بدون أي نص إضافي، بهذا الشكل:
[
  {{""title"": ""عنوان التوصية"", ""description"": ""وصف مختصر"", ""category"": ""Irrigation|Optimization|Growth|Maintenance""}}
]";

            try
            {
                var apiKey = _configuration["Anthropic:ApiKey"];
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var requestBody = new
                {
                    model = "claude-haiku-4-5-20251001",
                    max_tokens = 1024,
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

                if (!response.IsSuccessStatusCode)
                    return GetFallbackInsights(latest.SoilMoisture, dome.MinTargetMoisture);

                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();
                text = CleanJsonResponse(text);

                var insights = JsonSerializer.Deserialize<List<AiInsightDto>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return insights ?? GetFallbackInsights(latest.SoilMoisture, dome.MinTargetMoisture);
            }
            catch
            {
                return GetFallbackInsights(latest.SoilMoisture, dome.MinTargetMoisture);
            }
        }

        public async Task<PlantProfileDto> CalibrateProfileAsync(int domeId, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId)
                return new PlantProfileDto { Success = false, Message = "المزرعة غير موجودة" };

            var plantType = dome.PlantType ?? "نبات عام";
            var soilType = dome.SoilType ?? "تربة عادية";

            var prompt = $@"أنت خبير زراعي. أعطني النطاقات المثالية لزراعة ""{plantType}"" في تربة ""{soilType}"".
أجب بصيغة JSON فقط بدون أي نص إضافي:
{{
  ""optimalMoistureMin"": <رقم>,
  ""optimalMoistureMax"": <رقم>,
  ""optimalTempMin"": <رقم>,
  ""optimalTempMax"": <رقم>,
  ""optimalLightMin"": <رقم بالـ lux>,
  ""optimalLightMax"": <رقم بالـ lux>,
  ""notes"": ""ملاحظة مختصرة عن هذا النبات""
}}";

            try
            {
                var apiKey = _configuration["Anthropic:ApiKey"];
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var requestBody = new
                {
                    model = "claude-haiku-4-5-20251001",
                    max_tokens = 512,
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

                if (!response.IsSuccessStatusCode)
                    return GetDefaultProfile(plantType);

                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
                text = CleanJsonResponse(text);

                var profile = JsonSerializer.Deserialize<PlantProfileDto>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (profile == null) return GetDefaultProfile(plantType);

                
                dome.OptimalMoistureMin = profile.OptimalMoistureMin;
                dome.OptimalMoistureMax = profile.OptimalMoistureMax;
                dome.OptimalTempMin = profile.OptimalTempMin;
                dome.OptimalTempMax = profile.OptimalTempMax;
                dome.OptimalLightMin = profile.OptimalLightMin;
                dome.OptimalLightMax = profile.OptimalLightMax;
                
                dome.IsPlantProfileCalibrated = true;
                _domeRepository.Update(dome);
                await _domeRepository.SaveChangesAsync();

                profile.Success = true;
                profile.Message = $"تم معايرة النطاقات المثالية لـ {plantType} بنجاح";
                profile.PlantType = plantType;
                return profile;
            }
            catch
            {
                return GetDefaultProfile(plantType);
            }
        }

        public async Task<IrrigationDecisionDto> EvaluateIrrigationAsync(int domeId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null)
                return new IrrigationDecisionDto { ShouldWater = false, Reason = "المزرعة غير موجودة" };

            var latest = await _sensorRepository.GetLatestReadingAsync(domeId);
            if (latest == null)
                return new IrrigationDecisionDto { ShouldWater = false, Reason = "لا توجد قراءات" };

            
            if (latest.RainState >= 1.0)
                return new IrrigationDecisionDto { ShouldWater = false, Reason = "هناك مطر حالياً — تم تأجيل السقاية" };

            var prompt = $@"أنت نظام ري ذكي. قرر إذا كان يجب تشغيل الري الآن بناءً على كل العوامل التالية مجتمعةً وليس رطوبة التربة وحدها.

معلومات المزرعة:
- نوع النبات: {dome.PlantType ?? "غير محدد"}
- نوع التربة: {dome.SoilType ?? "غير محدد"}
- المنطقة: {dome.Country} - {dome.Governorate} - {dome.Neighborhood}
- الحد الأدنى للرطوبة الذي حدده المالك: {dome.MinTargetMoisture}%

القراءة الحالية من حساسات ESP32:
- رطوبة التربة: {latest.SoilMoisture:F1}%
- درجة الحرارة: {latest.Temperature:F1}°C
- الرطوبة الجوية: {latest.Humidity:F1}%
- شدة الضوء: {latest.LightIntensity:F0} lux
- حالة المطر: {(latest.RainState >= 1.0 ? "ممطر" : "جاف")}

اعتبارات مهمة: التربة الطينية تحتفظ بالماء أطول من الرملية، الحرارة العالية والضوء القوي يزيدان التبخر فتزيد الحاجة للري، الرطوبة الجوية العالية تقلل الحاجة للري، احترم الحد الأدنى الذي حدده المالك.

كذلك حدّد ""الحد الأعلى للرطوبة"" الذي يجب أن نسقي حتى نصل إليه ثم نتوقف — اختره ديناميكياً حسب الظروف الحالية:
- في الجو الحار/الجاف/الضوء القوي اجعله أعلى (لأن التبخر سريع)، وفي الجو المعتدل/الرطب اجعله أقل.
- يجب أن يكون أكبر من الحد الأدنى ({dome.MinTargetMoisture}%) وأقل من 95%.

أجب بصيغة JSON فقط بدون أي نص إضافي:
{{""shouldWater"": true/false, ""durationMinutes"": <رقم بين 5 و 30>, ""targetUpperMoisture"": <الحد الأعلى المتغير>, ""reason"": ""سبب مختصر بالعربي""}}";

            try
            {
                var apiKey = _configuration["Anthropic:ApiKey"];
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var requestBody = new
                {
                    model = "claude-haiku-4-5-20251001",
                    max_tokens = 256,
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var response = await client.PostAsync("https://api.anthropic.com/v1/messages",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return GetFallbackDecision(latest.SoilMoisture, dome.MinTargetMoisture, dome.OptimalMoistureMax);

                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
                text = CleanJsonResponse(text);

                var decision = JsonSerializer.Deserialize<IrrigationDecisionDto>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (decision == null)
                    return GetFallbackDecision(latest.SoilMoisture, dome.MinTargetMoisture, dome.OptimalMoistureMax);

                
                if (decision.TargetUpperMoisture <= dome.MinTargetMoisture || decision.TargetUpperMoisture > 95)
                    decision.TargetUpperMoisture = dome.OptimalMoistureMax > dome.MinTargetMoisture
                        ? dome.OptimalMoistureMax : dome.MinTargetMoisture + 10;
                return decision;
            }
            catch
            {
                return GetFallbackDecision(latest.SoilMoisture, dome.MinTargetMoisture, dome.OptimalMoistureMax);
            }
        }

        
        private IrrigationDecisionDto GetFallbackDecision(double soilMoisture, double minTarget, double optimalMax)
        {
            bool water = soilMoisture < minTarget;
            return new IrrigationDecisionDto
            {
                ShouldWater = water,
                DurationMinutes = 15,
                TargetUpperMoisture = optimalMax > minTarget ? optimalMax : minTarget + 10,
                Reason = water
                    ? $"رطوبة التربة {soilMoisture:F1}% أقل من الحد {minTarget}% (قرار احتياطي)"
                    : $"رطوبة التربة {soilMoisture:F1}% كافية"
            };
        }

        public async Task<double> CalculateHealthScoreAsync(int domeId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null) return 0;

            var readings = (await _sensorRepository.GetDomeHistoryAsync(domeId, hours: 24)).ToList();
            if (!readings.Any()) return 0;

            double totalScore = 0;
            int count = 0;

            foreach (var r in readings)
            {
                double moistureScore = ScoreInRange(r.SoilMoisture, dome.OptimalMoistureMin, dome.OptimalMoistureMax);
                double tempScore = ScoreInRange(r.Temperature, dome.OptimalTempMin, dome.OptimalTempMax);
                double lightScore = ScoreInRange(r.LightIntensity, dome.OptimalLightMin, dome.OptimalLightMax);

                totalScore += (moistureScore + tempScore + lightScore) / 3.0;
                count++;
            }

            return count > 0 ? Math.Round(totalScore / count, 1) : 0;
        }

        private double ScoreInRange(double value, double min, double max)
        {
            if (value >= min && value <= max) return 100.0;

            
            double range = max - min;
            if (range <= 0) return 100.0;

            double distance = value < min ? min - value : value - max;
            double penalty = (distance / range) * 100.0;
            return Math.Max(0, 100.0 - penalty);
        }

        private string CleanJsonResponse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            
            text = text.Trim();
            if (text.StartsWith("```"))
            {
                var firstNewline = text.IndexOf('\n');
                if (firstNewline >= 0) text = text[(firstNewline + 1)..];
                if (text.EndsWith("```")) text = text[..^3];
            }
            return text.Trim();
        }

        public async Task<List<AiScheduleSuggestionDto>> SuggestScheduleAsync(int domeId, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId)
                return new List<AiScheduleSuggestionDto>();

            var readings = (await _sensorRepository.GetDomeHistoryAsync(domeId, hours: 24)).ToList();
            var avgMoisture = readings.Any() ? readings.Average(r => r.SoilMoisture) : 50.0;
            var avgTemp = readings.Any() ? readings.Average(r => r.Temperature) : 25.0;

            var prompt = $@"أنت خبير زراعي. بناءً على هذه البيانات، اقترح جدول ري مثالي.

نوع النبات: {dome.PlantType ?? "عام"}
نوع التربة: {dome.SoilType ?? "عادية"}
متوسط رطوبة التربة (24 ساعة): {avgMoisture:F1}%
النطاق المثالي للرطوبة: {dome.OptimalMoistureMin}% - {dome.OptimalMoistureMax}%
متوسط درجة الحرارة: {avgTemp:F1}°C

اقترح 2-3 أوقات ري يومية مناسبة.
أجب بـ JSON فقط بدون أي نص إضافي:
[
  {{""time"": ""HH:MM"", ""durationMinutes"": <رقم>, ""reason"": ""سبب مختصر"", ""isRepeatDaily"": true}}
]";

            try
            {
                var apiKey = _configuration["Anthropic:ApiKey"];
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var requestBody = new
                {
                    model = "claude-haiku-4-5-20251001",
                    max_tokens = 512,
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var response = await client.PostAsync("https://api.anthropic.com/v1/messages",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return GetFallbackSchedule();

                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
                text = CleanJsonResponse(text);

                var suggestions = JsonSerializer.Deserialize<List<AiScheduleSuggestionDto>>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return suggestions ?? GetFallbackSchedule();
            }
            catch
            {
                return GetFallbackSchedule();
            }
        }

        private List<AiScheduleSuggestionDto> GetFallbackSchedule() => new()
        {
            new AiScheduleSuggestionDto { Time = "06:00", DurationMinutes = 20, Reason = "ري صباحي قبل ارتفاع الحرارة", IsRepeatDaily = true },
            new AiScheduleSuggestionDto { Time = "18:00", DurationMinutes = 15, Reason = "ري مسائي بعد انخفاض الحرارة", IsRepeatDaily = true }
        };

        private List<AiInsightDto> GetFallbackInsights(double soilMoisture, double minTarget)
        {
            var insights = new List<AiInsightDto>();

            if (soilMoisture < minTarget)
                insights.Add(new AiInsightDto
                {
                    Title = "رطوبة التربة منخفضة",
                    Description = $"رطوبة التربة {soilMoisture:F1}% أقل من الحد المستهدف {minTarget}%. يُنصح بالسقاية.",
                    Category = "Irrigation"
                });
            else
                insights.Add(new AiInsightDto
                {
                    Title = "رطوبة التربة مناسبة",
                    Description = $"رطوبة التربة {soilMoisture:F1}% ضمن النطاق المثالي.",
                    Category = "Growth"
                });

            return insights;
        }

        private PlantProfileDto GetDefaultProfile(string plantType)
        {
            return new PlantProfileDto
            {
                Success = false,
                Message = "تم استخدام القيم الافتراضية — تحقق من الـ API Key",
                PlantType = plantType,
                OptimalMoistureMin = 50, OptimalMoistureMax = 75,
                OptimalTempMin = 18, OptimalTempMax = 28,
                OptimalLightMin = 5000, OptimalLightMax = 10000,
                Notes = "قيم افتراضية"
            };
        }
    }
}
