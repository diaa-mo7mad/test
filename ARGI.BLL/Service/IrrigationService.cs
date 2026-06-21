using ARGI.DAL.DTO.Request;
using ARGI.DAL.DTO.Response;
using ARGI.DAL.Models;
using ARGI.DAL.Repository;
using Mapster;

namespace ARGI.BLL.Service
{
    public class IrrigationService : IIrrigationService
    {
        private readonly IIrrigationRepository _irrigationRepository;
        private readonly IDomeRepository _domeRepository;

        public IrrigationService(IIrrigationRepository irrigationRepository, IDomeRepository domeRepository)
        {
            _irrigationRepository = irrigationRepository;
            _domeRepository = domeRepository;
        }

        public async Task<IEnumerable<IrrigationScheduleResponseDto>> GetDomeSchedulesAsync(int domeId, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId)
                return Enumerable.Empty<IrrigationScheduleResponseDto>();

            var schedules = await _irrigationRepository.GetByDomeIdAsync(domeId);
            return schedules.Adapt<IEnumerable<IrrigationScheduleResponseDto>>();
        }

        public async Task<IrrigationScheduleResponseDto> CreateScheduleAsync(IrrigationScheduleRequestDto request, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(request.DomeId);
            if (dome == null || dome.UserId != userId)
                return new IrrigationScheduleResponseDto { Success = false, Message = "المزرعة غير موجودة أو لا تملك صلاحية الوصول" };

            var schedule = request.Adapt<IrrigationSchedule>();
            await _irrigationRepository.AddAsync(schedule);
            await _irrigationRepository.SaveChangesAsync();

            var response = schedule.Adapt<IrrigationScheduleResponseDto>();
            response.Success = true;
            response.Message = "تم إضافة الجدول بنجاح";
            return response;
        }

        public async Task<BaseResponse> DeleteScheduleAsync(int id, string userId)
        {
            var schedule = await _irrigationRepository.GetByIdAsync(id);
            if (schedule == null)
                return new BaseResponse { Success = false, Message = "الجدول غير موجود" };

            var dome = await _domeRepository.GetByIdAsync(schedule.DomeId);
            if (dome == null || dome.UserId != userId)
                return new BaseResponse { Success = false, Message = "لا تملك صلاحية حذف هذا الجدول" };

            _irrigationRepository.Delete(schedule);
            await _irrigationRepository.SaveChangesAsync();
            return new BaseResponse { Success = true, Message = "تم حذف الجدول بنجاح" };
        }
    }
}
