using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    public class ActivityEmbedding : AuditableBaseEntity
    {
        public int ActivityId { get; set; }
        public virtual Activity Activity { get; set; } = null!;
        public string Model { get; set; } = "all-Mini-L6-v2";
        public int Dimensions { get; set; } = 384;
        public string EmbeddingJson { get; set; } = string.Empty;
    }
}
