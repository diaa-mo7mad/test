using ARGI.DAL.DTO.Request;
using ARGI.DAL.DTO.Response;

namespace ARGI.BLL.Service
{
    public interface IIrrigationService
    {
        Task<IEnumerable<IrrigationScheduleResponseDto>> GetDomeSchedulesAsync(int domeId, string userId);
        Task<IrrigationScheduleResponseDto> CreateScheduleAsync(IrrigationScheduleRequestDto request, string userId);
        Task<BaseResponse> DeleteScheduleAsync(int id, string userId);
    }
}
