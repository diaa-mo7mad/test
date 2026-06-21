using ARGI.BLL.Service;
using ARGI.DAL.DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARGI.PL.Areas.User
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DomesController : ControllerBase
    {

        private readonly IDomeService _domeService;

        public DomesController(IDomeService domeService)
        {
            _domeService = domeService;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _domeService.GetUserDomesAsync(userId);
            return Ok(result);
        }

       
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _domeService.GetDomeByIdAsync(id, userId);

            if (!result.Success)
                return NotFound(result); 

            return Ok(result);
        }

       
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DomeRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _domeService.CreateDomeAsync(request, userId);

            if (!result.Success)
                return BadRequest(result); 

            return Ok(result);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DomeRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _domeService.UpdateDomeAsync(id, request, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _domeService.DeleteDomeAsync(id, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/irrigate")]
        public async Task<IActionResult> TriggerManualIrrigation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _domeService.TriggerManualIrrigationAsync(id, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{id}/stop-irrigation")]
        public async Task<IActionResult> StopIrrigation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _domeService.StopIrrigationAsync(id, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
