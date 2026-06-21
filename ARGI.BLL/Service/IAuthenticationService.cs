using ARGI.DAL.DTO.Response;
using ARGI.DAL.DTO.Request;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.BLL.Service
{
    public interface IAuthenticationService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<bool> ConfirmEmailAsync(string email, string userId);
        Task<ForgotPasswordResponse> RequestPasswordReset(ForgotPasswordRequest request);
        Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ProfileResponse> GetProfileAsync(string userId);
        Task<BaseResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<BaseResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    }
}