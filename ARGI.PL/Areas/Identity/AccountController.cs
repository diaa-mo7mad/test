using ARGI.BLL.Service;
using ARGI.DAL.DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ARGI.PL.Areas.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var response = await _authenticationService.RegisterAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authenticationService.LoginAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {
            var result = await _authenticationService.ConfirmEmailAsync(token, userId);


            return Ok(result);


        }
        [HttpPost("SendCode")]
        public async Task<IActionResult> RequestPasswordReset(ForgotPasswordRequest request)
        {
            var response = await _authenticationService.RequestPasswordReset(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("PasswordReset")]
        public async Task<IActionResult> PasswordReset(ResetPasswordRequest request)
        {
            var response = await _authenticationService.ResetPasswordAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _authenticationService.GetProfileAsync(userId);
            if (!response.Success) return BadRequest(response);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("Profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _authenticationService.UpdateProfileAsync(userId, request);
            if (!response.Success) return BadRequest(response);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _authenticationService.ChangePasswordAsync(userId, request);
            if (!response.Success) return BadRequest(response);
            return Ok(response);
        }
    }
}