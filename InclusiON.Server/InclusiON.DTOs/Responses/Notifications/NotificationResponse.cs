using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public static NotificationResponse From(Notification n) => new()
    {
        Id        = n.Id,
        Type      = n.Type,
        EventCode = n.EventCode,
        Title     = n.Title,
        Message   = n.Message,
        ActionUrl = n.ActionUrl,
        IsRead    = n.IsRead,
        ReadAt    = n.ReadAt,
        CreatedAt = n.CreatedAt
    };
}

public class UnreadNotificationsCountResponse
{
    public int Count { get; set; }
}
