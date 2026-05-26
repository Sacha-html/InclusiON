using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models;

public class PersonEmbedding : AuditableBaseEntity
{
    public Guid PersonId { get; set; }
    public virtual PersonWithDisability Person { get; set; } = null!;
    public string Model { get; set; } = "paraphrase-multilingual-MiniLM-L12-v2";
    public int Dimensions { get; set; } = 384;

    // EF ignores this property — configured via Fluent API (vector(384) managed via raw SQL / pgvector)
    public float[] Embedding { get; set; } = [];
}
