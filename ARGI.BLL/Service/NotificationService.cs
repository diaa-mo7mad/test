using ARGI.DAL.DTO.Response;
using ARGI.DAL.Models;
using ARGI.DAL.Repository;
using Mapster;

namespace ARGI.BLL.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IDomeRepository _domeRepository;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public NotificationService(INotificationRepository notificationRepository, IDomeRepository domeRepository, IRealtimeNotifier realtimeNotifier)
        {
            _notificationRepository = notificationRepository;
            _domeRepository = domeRepository;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetDomeNotificationsAsync(int domeId, string userId)
        {
            var dome = await _domeRepository.GetByIdAsync(domeId);
            if (dome == null || dome.UserId != userId)
                return Enumerable.Empty<NotificationResponseDto>();

            var notifications = await _notificationRepository.GetByDomeIdAsync(domeId);
            return notifications.Adapt<IEnumerable<NotificationResponseDto>>();
        }

        public async Task<BaseResponse> MarkAsReadAsync(int id, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                return new BaseResponse { Success = false, Message = "الإشعار غير موجود" };

            var dome = await _domeRepository.GetByIdAsync(notification.DomeId);
            if (dome == null || dome.UserId != userId)
                return new BaseResponse { Success = false, Message = "لا تملك صلاحية الوصول لهذا الإشعار" };

            notification.IsRead = true;
            _notificationRepository.Update(notification);
            await _notificationRepository.SaveChangesAsync();
            return new BaseResponse { Success = true, Message = "تم تحديث الإشعار" };
        }

        public async Task CreateNotificationAsync(int domeId, string message)
        {
            var notification = new Notification
            {
                DomeId = domeId,
                Message = message,
                CreatedAt = DateTime.Now
            };
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            await _realtimeNotifier.SendNotificationAsync(domeId, new
            {
                id = notification.Id,
                domeId = notification.DomeId,
                message = notification.Message,
                isRead = false,
                createdAt = notification.CreatedAt
            });
        }
    }
}
