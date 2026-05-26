namespace InclusiON.Domain.Models;

public record NotificationPayload
{
    public string UserId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public string? Email { get; init; }
    public bool SendEmailFallback { get; init; } = true;
}
