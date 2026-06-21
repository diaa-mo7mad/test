using ARGI.BLL.Service;
using ARGI.DAL.DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARGI.PL.Areas.User
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IrrigationSchedulesController : ControllerBase
    {
        private readonly IIrrigationService _irrigationService;

        public IrrigationSchedulesController(IIrrigationService irrigationService)
        {
            _irrigationService = irrigationService;
        }

        [HttpGet("{domeId}")]
        public async Task<IActionResult> GetByDome(int domeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _irrigationService.GetDomeSchedulesAsync(domeId, userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IrrigationScheduleRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _irrigationService.CreateScheduleAsync(request, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _irrigationService.DeleteScheduleAsync(id, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
