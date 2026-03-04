using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Invitacion para registro de representantes familiares.
    /// Generada por un profesional y enviada por email al familiar.
    /// Contiene un codigo unico con fecha de expiracion.
    /// </summary>
    public class Invitation : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico de la invitacion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del profesional que creo la invitacion.
        /// </summary>
        public Guid CreatedByProfessionalId { get; set; }

        /// <summary>
        /// ID de la persona con discapacidad para la cual se invita al familiar (opcional).
        /// </summary>
        public Guid? ForPersonId { get; set; }

        /// <summary>
        /// Codigo unico de la invitacion (usado para registrarse).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Email del familiar invitado.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre sugerido del familiar (opcional, prellenado en registro).
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Apellido sugerido del familiar (opcional, prellenado en registro).
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Parentesco sugerido con la persona (ej: Madre, Padre).
        /// </summary>
        public string? Relationship { get; set; }

        /// <summary>
        /// Fecha y hora de expiracion de la invitacion.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Indica si la invitacion ya fue utilizada para registrarse.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Fecha y hora en que se uso la invitacion.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// ID del usuario creado al usar la invitacion.
        /// </summary>
        public Guid? UsedByUserId { get; set; }

        /// <summary>
        /// Profesional que creo la invitacion.
        /// </summary>
        public virtual Professional CreatedByProfessional { get; set; } = null!;

        /// <summary>
        /// Persona con discapacidad asociada a la invitacion.
        /// </summary>
        public virtual PersonWithDisability? ForPerson { get; set; }

        /// <summary>
        /// Usuario creado al usar la invitacion.
        /// </summary>
        public virtual User? UsedByUser { get; set; }
    }
}
