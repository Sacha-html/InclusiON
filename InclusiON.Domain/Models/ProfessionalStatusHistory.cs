using InclusiON.Domain.Enums;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Historial de cambios de estado de un profesional.
    /// Registra cada vez que el estado del profesional cambia (pendiente/aprobado/rechazado).
    /// </summary>
    public class ProfessionalStatusHistory : AuditableBaseEntity
    {
        /// <summary>
        /// ID del registro de historial.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del profesional al que pertenece este historial.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// Estado anterior del profesional.
        /// </summary>
        public ProfessionalStatusEnum? OldStatus { get; set; }

        /// <summary>
        /// Nuevo estado del profesional.
        /// </summary>
        public ProfessionalStatusEnum NewStatus { get; set; }

        /// <summary>
        /// Observación o motivo del cambio (opcional).
        /// </summary>
        public string? Observation { get; set; }

        /// <summary>
        /// Usuario admin que realizó el cambio.
        /// </summary>
        public Guid? ChangedByUserId { get; set; }

        /// <summary>
        /// Profesional asociado a este historial.
        /// </summary>
        public virtual Professional Professional { get; set; } = null!;

        public ProfessionalStatusHistory()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}