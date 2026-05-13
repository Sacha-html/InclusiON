namespace InclusiON.Application.Interfaces.Infrastructure;

public interface IRealTimeNotifier
{
    Task NotifyUserAsync(string userId, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default);
}
