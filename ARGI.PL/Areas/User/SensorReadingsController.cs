using ARGI.BLL.Service;
using ARGI.DAL.DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARGI.PL.Areas.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorReadingsController : ControllerBase
    {
        private readonly ISensorService _sensorService;

        public SensorReadingsController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

       
        [HttpPost("ingest")]
        public async Task<IActionResult> IngestData([FromBody] SensorReadingRequestDto request)
        {
            var result = await _sensorService.SaveReadingAsync(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

       
        [Authorize]
        [HttpGet("history/{domeId}")]
        public async Task<IActionResult> GetHistory(int domeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _sensorService.GetHistoricalDataAsync(domeId, userId);
            return Ok(result);
        }

        [HttpGet("command/{macAddress}")]
        public async Task<IActionResult> GetCommand(string macAddress)
        {
            var result = await _sensorService.GetEsp32CommandAsync(macAddress);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("latest/{domeId}")]
        public async Task<IActionResult> GetLatest(int domeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _sensorService.GetLatestReadingAsync(domeId, userId);
            return Ok(result);
        }
    }
}
