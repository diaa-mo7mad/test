using ARGI.BLL.Service;
using ARGI.DAL.Repository;

namespace ARGI.PL.BackgroundServices
{
    public class IrrigationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IrrigationWorker> _logger;

        // لو سُقيت خلال هذه المدة → تجاهل الجدول القادم (قيمة عرض سريعة — أصلها ساعتان)
        private static readonly TimeSpan RecentWateringThreshold = TimeSpan.FromMinutes(2);
        // لو الجدول القادم خلال هذه المدة → يُعتبر "قريب"
        private static readonly TimeSpan UpcomingScheduleWindow = TimeSpan.FromHours(1);

        // لا نستدعي قرار الـ AI لنفس المزرعة أكثر من مرة خلال هذه المدة (قيمة عرض سريعة — أصلها 15 دقيقة)
        private static readonly TimeSpan AiCheckThrottle = TimeSpan.FromSeconds(30);
        private readonly Dictionary<int, DateTime> _lastAiCheck = new();

        public IrrigationWorker(IServiceProvider serviceProvider, ILogger<IrrigationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في IrrigationWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); // قيمة عرض سريعة — أصلها دقيقة
            }
        }

        private async Task ProcessAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var irrigationRepo = scope.ServiceProvider.GetRequiredService<IIrrigationRepository>();
            var domeRepo = scope.ServiceProvider.GetRequiredService<IDomeRepository>();
            var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var aiService = scope.ServiceProvider.GetRequiredService<IAiInsightService>();

            var allDomes = await domeRepo.GetAllAsync();

            foreach (var dome in allDomes)
            {
                // ── 1. AI Auto-Irrigation (قرار شامل: مطر + حرارة + ضوء + رطوبة + نوع التربة/النبات/المنطقة) ──
                if (dome.IsAiEnabled && !dome.IsManualWateringRequested)
                {
                    var latest = await sensorRepo.GetLatestReadingAsync(dome.Id);

                    bool wateredRecently = dome.LastWateredAt.HasValue &&
                        (DateTime.Now - dome.LastWateredAt.Value) < RecentWateringThreshold;

                    // فلاتر رخيصة قبل استدعاء الـ AI (توفير تكلفة الـ API)
                    bool soilBelowTarget = latest != null && latest.SoilMoisture < dome.MinTargetMoisture;
                    bool throttleOk = !_lastAiCheck.TryGetValue(dome.Id, out var lastCheck) ||
                                      (DateTime.Now - lastCheck) >= AiCheckThrottle;

                    if (latest != null && !wateredRecently && soilBelowTarget && throttleOk)
                    {
                        _lastAiCheck[dome.Id] = DateTime.Now;

                        // الـ AI يأخذ القرار النهائي بناءً على كل العوامل
                        var decision = await aiService.EvaluateIrrigationAsync(dome.Id);

                        if (decision.ShouldWater)
                        {
                            dome.IsManualWateringRequested = true;
                            dome.WateringSource = "AI";
                            dome.WateringDurationMinutes = decision.DurationMinutes > 0 ? decision.DurationMinutes : 15;
                            dome.LastWateredAt = DateTime.Now; // بداية جلسة الري
                            domeRepo.Update(dome);

                            await notificationService.CreateNotificationAsync(dome.Id,
                                $"الذكاء الاصطناعي بدأ السقاية — {decision.Reason}");

                            _logger.LogInformation($"AI triggered irrigation for dome '{dome.Name}': {decision.Reason}");
                        }
                        else
                        {
                            _logger.LogInformation($"AI declined irrigation for dome '{dome.Name}': {decision.Reason}");
                        }
                    }
                }

                // ── 2. Scheduled Irrigation ────────────────────────────────────────
                var schedules = await irrigationRepo.GetByDomeIdAsync(dome.Id);
                var now = DateTime.Now;

                foreach (var schedule in schedules)
                {
                    bool isDue = schedule.StartTime <= now && !schedule.IsExecuted;
                    bool isUpcoming = schedule.StartTime > now &&
                                     schedule.StartTime <= now.Add(UpcomingScheduleWindow);

                    // تحقق إن ما سُقينا مؤخراً
                    bool wateredRecently = dome.LastWateredAt.HasValue &&
                        (DateTime.Now - dome.LastWateredAt.Value) < RecentWateringThreshold;

                    // ── Smart Skip: إلغاء الجدول لو السقاية مؤخراً ──
                    if (wateredRecently && (isDue || isUpcoming))
                    {
                        string timeAgo = dome.LastWateredAt.HasValue
                            ? $"{(int)(DateTime.Now - dome.LastWateredAt.Value).TotalMinutes} دقيقة"
                            : "وقت قريب";

                        await notificationService.CreateNotificationAsync(dome.Id,
                            $"تم تخطي جدول السقاية ({schedule.StartTime:HH:mm}) — جرى السقاية منذ {timeAgo}");

                        if (!schedule.IsRepeatDaily)
                            schedule.IsExecuted = true;
                        else
                            schedule.StartTime = schedule.StartTime.AddDays(1);

                        await irrigationRepo.SaveChangesAsync();
                        _logger.LogInformation($"Skipped schedule for dome '{dome.Name}' — watered recently");
                        continue;
                    }

                    // ── تنفيذ الجدول ──
                    if (isDue && !dome.IsManualWateringRequested)
                    {
                        dome.IsManualWateringRequested = true;
                        dome.WateringSource = "Scheduled";
                        dome.WateringDurationMinutes = schedule.DurationMinutes > 0 ? schedule.DurationMinutes : 15;
                        dome.LastWateredAt = DateTime.Now; // بداية جلسة الري
                        domeRepo.Update(dome);

                        await notificationService.CreateNotificationAsync(dome.Id,
                            $"بدأت السقاية المجدولة ({schedule.StartTime:HH:mm}) لمدة {schedule.DurationMinutes} دقيقة");

                        if (!schedule.IsRepeatDaily)
                            schedule.IsExecuted = true;
                        else
                            schedule.StartTime = schedule.StartTime.AddDays(1);

                        await irrigationRepo.SaveChangesAsync();
                        _logger.LogInformation($"Scheduled irrigation triggered for dome '{dome.Name}'");
                    }
                }

                await domeRepo.SaveChangesAsync();
            }
        }
    }
}
