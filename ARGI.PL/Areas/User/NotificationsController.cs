using ARGI.BLL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARGI.PL.Areas.User
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{domeId}")]
        public async Task<IActionResult> GetByDome(int domeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notificationService.GetDomeNotificationsAsync(domeId, userId);
            return Ok(result);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _notificationService.MarkAsReadAsync(id, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
