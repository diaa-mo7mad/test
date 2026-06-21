using Microsoft.AspNetCore.SignalR;

namespace ARGI.PL.Hubs
{
    public class DomeHub : Hub
    {
        public async Task JoinDome(string domeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"dome_{domeId}");
        }

        public async Task LeaveDome(string domeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"dome_{domeId}");
        }
    }
}
