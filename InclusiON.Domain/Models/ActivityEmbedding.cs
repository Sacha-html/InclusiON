using System.ComponentModel.DataAnnotations.Schema;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    public class ActivityEmbedding : AuditableBaseEntity
    {
        public int ActivityId { get; set; }
        public virtual Activity Activity { get; set; } = null!;
        public string Model { get; set; } = "paraphrase-multilingual-MiniLM-L12-v2";
        public int Dimensions { get; set; } = 384;

        // EF does not map this column — it is managed via raw SQL (pgvector type)
        [NotMapped]
        public float[] Embedding { get; set; } = [];
    }
}
