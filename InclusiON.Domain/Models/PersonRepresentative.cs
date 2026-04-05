namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Relacion entre persona con discapacidad y representante familiar.
    /// Tabla intermedia que define permisos y consentimiento.
    /// </summary>
    public class PersonRepresentative
    {
        /// <summary>
        /// ID unico del registro de vinculacion.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID de la persona con discapacidad.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// ID del representante familiar.
        /// </summary>
        public Guid RepresentativeId { get; set; }

        /// <summary>
        /// Tipo de parentesco (Madre, Padre, Tutor, etc.).
        /// </summary>
        public string? Relationship { get; set; }

        /// <summary>
        /// Indica si es el representante principal.
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>
        /// Indica si se ha firmado el consentimiento informado.
        /// </summary>
        public bool HasInformedConsent { get; set; } = false;

        /// <summary>
        /// Fecha en que se firmo el consentimiento.
        /// </summary>
        public DateTime? ConsentDate { get; set; }

        /// <summary>
        /// Indica si puede supervisar el login de la persona.
        /// </summary>
        public bool CanSuperviseLogin { get; set; } = false;

        /// <summary>
        /// Indica si la relacion esta activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de creacion de la relacion.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de ultima modificacion de la relacion.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Fecha de desvinculacion (null si esta activo).
        /// </summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// Motivo de desvinculacion (cuando EndedAt no es null).
        /// </summary>
        public string? UnlinkObservation { get; set; }

        /// <summary>
        /// Persona con discapacidad.
        /// </summary>
        public virtual PersonWithDisability Person { get; set; } = null!;

        /// <summary>
        /// Representante familiar.
        /// </summary>
        public virtual FamilyRepresentative Representative { get; set; } = null!;

        public PersonRepresentative()
        {
            Id = Guid.NewGuid();
        }
    }
}
