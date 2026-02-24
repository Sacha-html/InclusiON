namespace InclusiON.Domain.Models.BaseEntities
{
    public abstract class AuditableBaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
