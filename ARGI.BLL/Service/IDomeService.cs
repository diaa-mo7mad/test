using ARGI.DAL.DTO.Request;
using ARGI.DAL.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.BLL.Service
{
    public interface IDomeService
    {
        Task<IEnumerable<DomeResponseDto>> GetUserDomesAsync(string userId);
        Task<DomeResponseDto> GetDomeByIdAsync(int id, string userId);
        Task<DomeResponseDto> CreateDomeAsync(DomeRequestDto requestDto, string userId);
        Task<DomeResponseDto> UpdateDomeAsync(int id, DomeRequestDto requestDto, string userId);
        Task<BaseResponse> DeleteDomeAsync(int id, string userId);
        Task<BaseResponse> TriggerManualIrrigationAsync(int id, string userId);
        Task<BaseResponse> StopIrrigationAsync(int id, string userId);
    }
}
