using ARGI.DAL.DTO.Request;
using ARGI.DAL.DTO.Response;
using ARGI.DAL.Models;
using ARGI.DAL.Repository;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.BLL.Service
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IDomeRepository _domeRepository;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly INotificationService _notificationService;

        public SensorService(ISensorRepository sensorRepository, IDomeRepository domeRepository, IRealtimeNotifier realtimeNotifier, INotificationService notificationService)
        {
            _sensorRepository = sensorRepository;
            _domeRepository = domeRepository;
            _realtimeNotifier = realtimeNotifier;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse> SaveReadingAsync(SensorReadingRequestDto request)
        {
            
            var dome = await _domeRepository.GetByMacAddressAsync(request.MacAddress);
            if (dome == null)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "الجهاز غير مسجل لأي مزرعة حظر الطلب."
                };
            }

            try
            {
                
                var reading = request.Adapt<SensorReading>();
                reading.DomeId = dome.Id;
                reading.Timestamp = request.Timestamp ?? DateTime.Now;

                
                dome.LastPingTime = DateTime.Now;
                _domeRepository.Update(dome);

                
                await _sensorRepository.AddReadingAsync(reading);
                await _sensorRepository.SaveChangesAsync();

                await _realtimeNotifier.SendSensorReadingAsync(dome.Id, new
                {
                    domeId = dome.Id,
                    temperature = reading.Temperature,
                    humidity = reading.Humidity,
                    soilMoisture = reading.SoilMoisture,
                    lightIntensity = reading.LightIntensity,
                    rainState = reading.RainState,
                    timestamp = reading.Timestamp
                });

                return new BaseResponse
                {
                    Success = true,
                    Message = "تم استقبال القراءة وتحديث حالة المزرعة بنجاح."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "حدث خطأ أثناء معالجة البيانات",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<IEnumerable<SensorReadingResponseDto>> GetHistoricalDataAsync(int domeId, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId)
                return Enumerable.Empty<SensorReadingResponseDto>();

            var history = await _sensorRepository.GetDomeHistoryAsync(domeId, hours: 24);
            return history.Adapt<IEnumerable<SensorReadingResponseDto>>();
        }

        public async Task<SensorReadingResponseDto> GetLatestReadingAsync(int domeId, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId) return null;

            var reading = await _sensorRepository.GetLatestReadingAsync(domeId);
            if (reading == null) return null;

            return reading.Adapt<SensorReadingResponseDto>();
        }

        // تحكّم مغلق: القطعة تضخ ما دام يرجع water=true، ونوقف فوراً عند تحقّق الشروط
        public async Task<object> GetEsp32CommandAsync(string macAddress)
        {
            var dome = await _domeRepository.GetByMacAddressAsync(macAddress);
            if (dome == null)
                return new { water = false, durationMinutes = 0, source = "None" };

            // لا توجد جلسة ري نشطة
            if (!dome.IsManualWateringRequested)
                return new { water = false, durationMinutes = 0, source = "None" };

            // جلسة ري نشطة — نقرر هل نكمّل أم نوقف
            var latest = await _sensorRepository.GetLatestReadingAsync(dome.Id);
            const double StopBuffer = 5.0; // نوقف عند تجاوز الحد بـ 5% لتجنّب التذبذب

            bool soilReached = latest != null && latest.SoilMoisture >= dome.MinTargetMoisture + StopBuffer;
            bool rainStarted = latest != null && latest.RainState >= 1.0;
            bool maxDurationElapsed = dome.LastWateredAt.HasValue &&
                (DateTime.Now - dome.LastWateredAt.Value).TotalMinutes >= dome.WateringDurationMinutes;

            if (soilReached || rainStarted || maxDurationElapsed)
            {
                dome.IsManualWateringRequested = false; // إنهاء الجلسة → أمر إيقاف
                _domeRepository.Update(dome);
                await _domeRepository.SaveChangesAsync();

                string reason = soilReached ? $"الرطوبة وصلت {latest.SoilMoisture:F1}%"
                              : rainStarted ? "بدأ المطر"
                              : "انتهت مدة الأمان القصوى";
                await _notificationService.CreateNotificationAsync(dome.Id, $"توقفت السقاية تلقائياً — {reason}");

                return new { water = false, durationMinutes = 0, source = dome.WateringSource };
            }

            // استمرار الري
            return new { water = true, durationMinutes = dome.WateringDurationMinutes, source = dome.WateringSource };
        }
    }
}
