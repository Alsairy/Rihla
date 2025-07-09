using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Rihla.WebAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinTenantGroup(string tenantId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
        }

        public async Task LeaveTenantGroup(string tenantId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
        }

        public async Task JoinTripGroup(string tripId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"trip_{tripId}");
        }

        public async Task LeaveTripGroup(string tripId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip_{tripId}");
        }

        public override async Task OnConnectedAsync()
        {
            var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
