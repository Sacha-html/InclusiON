using InclusiON.Domain.Enums;

namespace InclusiON.Domain.Models;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Type { get; set; } = NotificationTypes.System;
    public string EventCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDismissed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
}
