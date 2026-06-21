using ARGI.DAL.DTO.Response;

namespace ARGI.BLL.Service
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetDomeNotificationsAsync(int domeId, string userId);
        Task<BaseResponse> MarkAsReadAsync(int id, string userId);
        Task CreateNotificationAsync(int domeId, string message);
    }
}
