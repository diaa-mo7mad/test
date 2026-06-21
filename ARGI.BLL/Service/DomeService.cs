using ARGI.DAL.DTO.Request;
using ARGI.DAL.DTO.Response;
using ARGI.DAL.Models;
using ARGI.DAL.Repository;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.BLL.Service
{
    public class DomeService : IDomeService
    {
        private readonly IDomeRepository _domeRepository;
        private readonly IAiInsightService _aiInsightService;

        public DomeService(IDomeRepository domeRepository, IAiInsightService aiInsightService)
        {
            _domeRepository = domeRepository;
            _aiInsightService = aiInsightService;
        }

       
        public async Task<IEnumerable<DomeResponseDto>> GetUserDomesAsync(string userId)
        {
            var domes = await _domeRepository.GetUserDomesAsync(userId);

            
            var response = domes.Adapt<IEnumerable<DomeResponseDto>>();
            foreach (var item in response)
            {
                item.Success = true;
                item.Message = "Data retrieved successfully";
            }
            return response;
        }

       
        public async Task<DomeResponseDto> GetDomeByIdAsync(int id, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(id);

            if (dome == null || dome.UserId != userId)
            {
                return new DomeResponseDto
                {
                    Success = false,
                    Message = "المزرعة غير موجودة أو لا تملك صلاحية الوصول",
                    Errors = new List<string> { "NotFound or Unauthorized Access" }
                };
            }

            var response = dome.Adapt<DomeResponseDto>();
            response.Success = true;
            response.Message = "Success";
            return response;
        }

       
        public async Task<DomeResponseDto> CreateDomeAsync(DomeRequestDto requestDto, string userId)
        {
           
            if (await _domeRepository.IsMacAddressExistsAsync(requestDto.MacAddress))
            {
                return new DomeResponseDto
                {
                    Success = false,
                    Message = "خطأ في الإضافة",
                    Errors = new List<string> { "الـ Mac Address مستخدم مسبقاً لجهاز آخر" }
                };
            }

            try
            {
                var dome = requestDto.Adapt<Dome>();
                dome.UserId = userId;

                await _domeRepository.AddAsync(dome);
                await _domeRepository.SaveChangesAsync();

                // معايرة النطاقات المثالية بالذكاء الاصطناعي تلقائياً
                await _aiInsightService.CalibrateProfileAsync(dome.Id, userId);

                var response = dome.Adapt<DomeResponseDto>();
                response.Success = true;
                response.Message = "تم إنشاء المزرعة بنجاح";
                return response;
            }
            catch (Exception ex)
            {
                return new DomeResponseDto
                {
                    Success = false,
                    Message = "فشلت عملية الحفظ",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        
        public async Task<DomeResponseDto> UpdateDomeAsync(int id, DomeRequestDto requestDto, string userId)
        {
            var existingDome = await _domeRepository.GetByIdAsync(id);

            if (existingDome == null || existingDome.UserId != userId)
            {
                return new DomeResponseDto
                {
                    Success = false,
                    Message = "المزرعة غير موجودة للتعديل",
                    Errors = new List<string> { "Update target not found" }
                };
            }

            try
            {
                
                bool plantTypeChanged = existingDome.PlantType != requestDto.PlantType;
                requestDto.Adapt(existingDome);

                _domeRepository.Update(existingDome);
                await _domeRepository.SaveChangesAsync();

                // إعادة المعايرة لو تغيّر نوع النبات
                if (plantTypeChanged && !string.IsNullOrEmpty(existingDome.PlantType))
                    await _aiInsightService.CalibrateProfileAsync(existingDome.Id, userId);

                var response = existingDome.Adapt<DomeResponseDto>();
                response.Success = true;
                response.Message = "تم تحديث البيانات بنجاح";
                return response;
            }
            catch (Exception ex)
            {
                return new DomeResponseDto
                {
                    Success = false,
                    Message = "فشل التعديل",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

       
        public async Task<BaseResponse> DeleteDomeAsync(int id, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(id);

            if (dome == null || dome.UserId != userId)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "لا يمكن الحذف: المزرعة غير موجودة",
                    Errors = new List<string> { "Deletion target not found" }
                };
            }

            _domeRepository.Delete(dome);
            var result = await _domeRepository.SaveChangesAsync();

            return new BaseResponse
            {
                Success = result,
                Message = result ? "تم حذف المزرعة نهائياً" : "فشل الحذف من قاعدة البيانات"
            };
        }

        public async Task<BaseResponse> TriggerManualIrrigationAsync(int id, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(id);
            if (dome == null || dome.UserId != userId)
                return new BaseResponse { Success = false, Message = "المزرعة غير موجودة أو لا تملك صلاحية الوصول" };

            dome.IsManualWateringRequested = true;
            dome.WateringSource = "Manual";
            dome.WateringDurationMinutes = 15;
            dome.LastWateredAt = DateTime.Now; // بداية جلسة الري
            _domeRepository.Update(dome);
            await _domeRepository.SaveChangesAsync();

            return new BaseResponse { Success = true, Message = "تم إرسال أمر السقاية اليدوية بنجاح" };
        }

        public async Task<BaseResponse> StopIrrigationAsync(int id, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(id);
            if (dome == null || dome.UserId != userId)
                return new BaseResponse { Success = false, Message = "المزرعة غير موجودة أو لا تملك صلاحية الوصول" };

            dome.IsManualWateringRequested = false; // أمر إيقاف للقطعة
            // نضع LastWateredAt الآن حتى لا يعيد الذكاء الاصطناعي التشغيل فوراً (فترة سماح)
            dome.LastWateredAt = DateTime.Now;
            _domeRepository.Update(dome);
            await _domeRepository.SaveChangesAsync();

            return new BaseResponse { Success = true, Message = "تم إيقاف السقاية" };
        }
    }
}
