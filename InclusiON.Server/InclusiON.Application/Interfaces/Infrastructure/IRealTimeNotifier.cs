namespace InclusiON.Application.Interfaces.Infrastructure;

public record NotificationPushPayload(
    string Id,
    string Type,
    string EventCode,
    string Title,
    string Message,
    string? ActionUrl,
    bool IsRead,
    DateTime CreatedAt);

public interface IRealTimeNotifier
{
    Task NotifyUserAsync(string userId, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(string userId, NotificationPushPayload payload, CancellationToken cancellationToken = default);
}
