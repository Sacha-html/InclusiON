using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models;

public class BackgroundJob : AuditableBaseEntity
{
    public int Id { get; set; }
    public int JobTypeId { get; set; }
    public int StatusId { get; set; } = 1;

    public string Payload { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string? ErrorMessage { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual JobType JobType { get; set; } = null!;
    public virtual BackgroundJobStatus Status { get; set; } = null!;
}
