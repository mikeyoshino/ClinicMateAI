using Microsoft.AspNetCore.SignalR;

namespace ClinicMateAI.Web.Hubs;

public sealed class InboxHub : Hub
{
    public async Task JoinClinic(string clinicId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"clinic-{clinicId}");
    }

    public async Task LeaveClinic(string clinicId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"clinic-{clinicId}");
    }
}
