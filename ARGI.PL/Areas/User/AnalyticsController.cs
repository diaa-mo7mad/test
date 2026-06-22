using ARGI.BLL.Service;
using ARGI.DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARGI.PL.Areas.User
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IDomeRepository _domeRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly IAiInsightService _aiInsightService;

        public AnalyticsController(IDomeRepository domeRepository, ISensorRepository sensorRepository, IAiInsightService aiInsightService)
        {
            _domeRepository = domeRepository;
            _sensorRepository = sensorRepository;
            _aiInsightService = aiInsightService;
        }

        [HttpGet("{domeId}")]
        public async Task<IActionResult> GetAnalytics(int domeId, [FromQuery] string period = "7d")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId)
                return NotFound(new { success = false, message = "المزرعة غير موجودة" });

            int hours = period switch
            {
                "1d"  => 24,
                "24h" => 24,
                "7d"  => 168,
                "30d" => 720,
                "90d" => 2160,
                _     => 168
            };

            var readings = (await _sensorRepository.GetDomeHistoryAsync(domeId, hours)).ToList();

            if (!readings.Any())
                return Ok(new
                {
                    success = true,
                    avgSoilMoisture = 0,
                    avgTemperature = 0,
                    avgLightIntensity = 0,
                    totalWaterConsumption = 0,
                    healthScore = 0,
                    dailyData = new List<object>()
                });

          
            IEnumerable<object> grouped;

            double CalcHealth(double moisture, double temp, double light)
            {
                static double Score(double v, double mn, double mx)
                {
                    if (v >= mn && v <= mx) return 100;
                    double range = mx - mn; if (range <= 0) return 100;
                    double dist = v < mn ? mn - v : v - mx;
                    return Math.Max(0, 100 - (dist / range) * 100);
                }
                var scores = new List<double>();
                if (dome.OptimalMoistureMax > dome.OptimalMoistureMin)
                    scores.Add(Score(moisture, dome.OptimalMoistureMin, dome.OptimalMoistureMax));
                if (dome.OptimalTempMax > dome.OptimalTempMin)
                    scores.Add(Score(temp, dome.OptimalTempMin, dome.OptimalTempMax));
                if (dome.OptimalLightMax > dome.OptimalLightMin)
                    scores.Add(Score(light, dome.OptimalLightMin, dome.OptimalLightMax));
                return scores.Any() ? Math.Round(scores.Average(), 1) : 50;
            }

            if (period == "1d" || period == "24h")
            {
                grouped = readings
                    .GroupBy(r => new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour, 0, 0))
                    .OrderBy(g => g.Key)
                    .Select(g => {
                        var m = Math.Round(g.Average(r => r.SoilMoisture), 1);
                        var t = Math.Round(g.Average(r => r.Temperature), 1);
                        var l = Math.Round(g.Average(r => r.LightIntensity), 1);
                        return (object)new { date = g.Key.ToString("HH:mm"), avgSoilMoisture = m, avgTemperature = t, avgLightIntensity = l, waterConsumption = Math.Round(g.Count() * 0.3, 1), healthScore = CalcHealth(m, t, l) };
                    });
            }
            else if (period == "90d")
            {
                var startDate = DateTime.Now.AddHours(-hours);
                grouped = readings
                    .GroupBy(r => (int)((r.Timestamp - startDate).TotalDays / 7))
                    .OrderBy(g => g.Key)
                    .Select(g => {
                        var m = Math.Round(g.Average(r => r.SoilMoisture), 1);
                        var t = Math.Round(g.Average(r => r.Temperature), 1);
                        var l = Math.Round(g.Average(r => r.LightIntensity), 1);
                        return (object)new { date = "W" + (g.Key + 1), avgSoilMoisture = m, avgTemperature = t, avgLightIntensity = l, waterConsumption = Math.Round(g.Count() * 0.3, 1), healthScore = CalcHealth(m, t, l) };
                    });
            }
            else
            {
                grouped = readings
                    .GroupBy(r => r.Timestamp.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => {
                        var m = Math.Round(g.Average(r => r.SoilMoisture), 1);
                        var t = Math.Round(g.Average(r => r.Temperature), 1);
                        var l = Math.Round(g.Average(r => r.LightIntensity), 1);
                        return (object)new { date = period == "30d" ? g.Key.ToString("MMM dd") : g.Key.ToString("ddd"), avgSoilMoisture = m, avgTemperature = t, avgLightIntensity = l, waterConsumption = Math.Round(g.Count() * 0.3, 1), healthScore = CalcHealth(m, t, l) };
                    });
            }

            var dailyData = grouped.ToList();
            var healthScore = await _aiInsightService.CalculateHealthScoreAsync(domeId);

            return Ok(new
            {
                success = true,
                avgSoilMoisture       = Math.Round(readings.Average(r => r.SoilMoisture), 1),
                avgTemperature        = Math.Round(readings.Average(r => r.Temperature), 1),
                avgLightIntensity     = Math.Round(readings.Average(r => r.LightIntensity), 1),
                totalWaterConsumption = Math.Round(readings.Count * 0.3, 1),
                healthScore,
                optimalRanges = new
                {
                    moisture    = new { min = dome.OptimalMoistureMin, max = dome.OptimalMoistureMax },
                    temperature = new { min = dome.OptimalTempMin,     max = dome.OptimalTempMax },
                    light       = new { min = dome.OptimalLightMin,    max = dome.OptimalLightMax },
                    isCalibrated = dome.IsPlantProfileCalibrated
                },
                dailyData
            });
        }
    }
}
