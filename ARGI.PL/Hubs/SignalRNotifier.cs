using ARGI.BLL.Service;
using Microsoft.AspNetCore.SignalR;

namespace ARGI.PL.Hubs
{
    public class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<DomeHub> _hubContext;

        public SignalRNotifier(IHubContext<DomeHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendSensorReadingAsync(int domeId, object data)
        {
            await _hubContext.Clients.Group($"dome_{domeId}").SendAsync("SensorUpdate", data);
        }

        public async Task SendNotificationAsync(int domeId, object notification)
        {
            await _hubContext.Clients.Group($"dome_{domeId}").SendAsync("NewNotification", notification);
        }
    }
}
