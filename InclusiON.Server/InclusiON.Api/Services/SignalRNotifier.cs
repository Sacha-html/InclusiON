using Microsoft.AspNetCore.SignalR;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Api.Hubs;

namespace InclusiON.Api.Services;

public class SignalRNotifier : IRealTimeNotifier
{
    readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(string userId, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(userId).SendAsync("Notification", new
        {
            title,
            message,
            actionUrl
        }, cancellationToken);
    }

    public async Task SendNotificationAsync(string userId, NotificationPushPayload payload, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(userId).SendAsync("Notification", new
        {
            id        = payload.Id,
            type      = payload.Type,
            eventCode = payload.EventCode,
            title     = payload.Title,
            message   = payload.Message,
            actionUrl = payload.ActionUrl,
            isRead    = payload.IsRead,
            createdAt = payload.CreatedAt
        }, cancellationToken);
    }
}
