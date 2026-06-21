using ARGI.BLL.Service;
using ARGI.DAL.Models;
using ARGI.DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARGI.PL.Areas.User
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AiInsightsController : ControllerBase
    {
        private readonly IAiInsightService _aiInsightService;
        private readonly IIrrigationRepository _irrigationRepository;

        public AiInsightsController(IAiInsightService aiInsightService, IIrrigationRepository irrigationRepository)
        {
            _aiInsightService = aiInsightService;
            _irrigationRepository = irrigationRepository;
        }

        [HttpPost("{domeId}")]
        public async Task<IActionResult> Generate(int domeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _aiInsightService.GenerateInsightsAsync(domeId, userId);
            return Ok(result);
        }

        // يُستدعى مرة عند إنشاء المزرعة أو تغيير نوع النبات
        [HttpPost("{domeId}/calibrate")]
        public async Task<IActionResult> Calibrate(int domeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _aiInsightService.CalibrateProfileAsync(domeId, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{domeId}/health")]
        public async Task<IActionResult> GetHealth(int domeId)
        {
            var score = await _aiInsightService.CalculateHealthScoreAsync(domeId);
            return Ok(new { domeId, healthScore = score });
        }

        // AI يقترح جدول ري ويحفظه مباشرة
        [HttpPost("{domeId}/suggest-schedule")]
        public async Task<IActionResult> SuggestSchedule(int domeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var suggestions = await _aiInsightService.SuggestScheduleAsync(domeId, userId);
            if (!suggestions.Any()) return BadRequest("لا توجد اقتراحات");

            var saved = new List<IrrigationSchedule>();
            var today = DateTime.Today;

            foreach (var s in suggestions)
            {
                if (!TimeSpan.TryParse(s.Time, out var timeOfDay)) continue;
                var startTime = today.Add(timeOfDay);
                if (startTime < DateTime.Now) startTime = startTime.AddDays(1);

                var schedule = new IrrigationSchedule
                {
                    DomeId = domeId,
                    StartTime = startTime,
                    DurationMinutes = s.DurationMinutes,
                    IsRepeatDaily = s.IsRepeatDaily,
                    IsExecuted = false
                };
                await _irrigationRepository.AddAsync(schedule);
                saved.Add(schedule);
            }

            await _irrigationRepository.SaveChangesAsync();
            return Ok(new { count = saved.Count, suggestions, message = $"تم حفظ {saved.Count} جداول ري بواسطة الذكاء الاصطناعي" });
        }
    }
}
