namespace InclusiON.Application.Interfaces.Infrastructure;

public record NotificationMessage(
    Guid UserId,
    string Type,
    string EventCode,
    string Title,
    string Message,
    string? ActionUrl = null);

public interface INotificationService
{
    Task NotifyAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
    Task NotifyManyAsync(IEnumerable<NotificationMessage> notifications, CancellationToken cancellationToken = default);
    string EncryptId(Guid id);
    string EncryptIntId(int id);
}
