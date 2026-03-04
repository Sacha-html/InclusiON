using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    public class ActivityResult : AuditableBaseEntity
    {
        public int Id { get; set; }
        public int PersonRoadmapActivityId { get; set; }
        public virtual PersonRoadmapActivity PersonRoadmapActivity { get; set; } = null!;
        public int AttemptNumber { get; set; }
        public string? JsonResponse { get; set; }
        public float ScorePercent { get; set; }
        public int TimeSpentSeconds { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
