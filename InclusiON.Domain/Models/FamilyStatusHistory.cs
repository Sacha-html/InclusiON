using InclusiON.Domain.Enums;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Historial de cambios de estado de un familiar.
    /// Registra cada vez que el estado del familiar cambia (active/terminated).
    /// </summary>
    public class FamilyStatusHistory : AuditableBaseEntity
    {
        /// <summary>
        /// ID del registro de historial.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del familiar al que pertenece este historial.
        /// </summary>
        public Guid FamilyId { get; set; }

        /// <summary>
        /// Estado anterior del familiar.
        /// </summary>
        public FamilyStatusEnum? OldStatus { get; set; }

        /// <summary>
        /// Nuevo estado del familiar.
        /// </summary>
        public FamilyStatusEnum NewStatus { get; set; }

        /// <summary>
        /// Observación o motivo del cambio.
        /// </summary>
        public string? Observation { get; set; }

        /// <summary>
        /// Usuario que realizó el cambio.
        /// </summary>
        public Guid? ChangedByUserId { get; set; }

        /// <summary>
        /// Familiar asociado a este historial.
        /// </summary>
        public virtual FamilyRepresentative Family { get; set; } = null!;

        public FamilyStatusHistory()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
