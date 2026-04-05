using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Historial de cambios de vinculación entre persona con discapacidad y familiar.
    /// Registra cada vez que se vincula/desvincula un familiar de una persona.
    /// </summary>
    public class PersonRepresentativeHistory : AuditableBaseEntity
    {
        /// <summary>
        /// ID del registro de historial.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del registro de PersonRepresentative que se modificó.
        /// </summary>
        public Guid PersonRepresentativeId { get; set; }

        /// <summary>
        /// ID de la persona con discapacidad.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// ID del familiar/representante familiar.
        /// </summary>
        public Guid RepresentativeId { get; set; }

        /// <summary>
        /// Tipo de cambio realizado.
        /// </summary>
        public PersonRepresentativeChangeType ChangeType { get; set; }

        /// <summary>
        /// Relación del familiar con la persona en el momento del cambio.
        /// </summary>
        public string? Relationship { get; set; }

        /// <summary>
        /// Indica si era familiar principal en el momento del cambio.
        /// </summary>
        public bool? WasPrimary { get; set; }

        /// <summary>
        /// Observación o motivo del cambio (especialmente para desvincular).
        /// </summary>
        public string? Observation { get; set; }

        /// <summary>
        /// Usuario que realizó el cambio.
        /// </summary>
        public Guid? ChangedByUserId { get; set; }

        /// <summary>
        /// Persona con discapacidad.
        /// </summary>
        public virtual PersonWithDisability Person { get; set; } = null!;

        /// <summary>
        /// Representante familiar.
        /// </summary>
        public virtual FamilyRepresentative Representative { get; set; } = null!;

        public PersonRepresentativeHistory()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Tipos de cambios en la vinculación familiar.
    /// </summary>
    public enum PersonRepresentativeChangeType
    {
        /// <summary>
        /// Nuevo vínculo creado.
        /// </summary>
        Linked = 0,

        /// <summary>
        /// Vinculación actualizada (relación, es principal).
        /// </summary>
        Updated = 1,

        /// <summary>
        /// Familiar desvinculado.
        /// </summary>
        Unlinked = 2
    }
}
