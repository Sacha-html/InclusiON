using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    public class ActivityEmbedding : AuditableBaseEntity
    {
        public int ActivityId { get; set; }
        public virtual Activity Activity { get; set; } = null!;
        public string Model { get; set; } = "paraphrase-multilingual-MiniLM-L12-v2";
        public int Dimensions { get; set; } = 384;

        // EF ignores this property — configured via Fluent API in ActivityEmbeddingConfiguration
        // (vector(384) managed via raw SQL / pgvector, not mapped by EF)
        public float[] Embedding { get; set; } = [];
    }
}
