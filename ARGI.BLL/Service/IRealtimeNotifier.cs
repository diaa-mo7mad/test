namespace ARGI.BLL.Service
{
    public interface IRealtimeNotifier
    {
        Task SendSensorReadingAsync(int domeId, object data);
        Task SendNotificationAsync(int domeId, object notification);
    }
}
