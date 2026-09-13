using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace zenvy.api.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("UserConnected", Context.UserIdentifier ?? Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.Caller.SendAsync("UserDisconnected", Context.UserIdentifier ?? Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public Task SendMessage(string message) =>
        Clients.Others.SendAsync("ReceiveMessage", Context.UserIdentifier ?? "system", message);
}
