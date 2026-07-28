namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Relacion entre profesional y persona con discapacidad.
    /// Define la asignacion de profesionales a personas y sus permisos.
    /// </summary>
    public class ProfessionalPerson
    {
        /// <summary>
        /// ID del profesional.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// ID de la persona con discapacidad.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// Fecha de asignacion del profesional.
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// Indica si es el profesional principal responsable.
        /// </summary>
        public bool IsPrimaryProfessional { get; set; } = false;

        /// <summary>
        /// Indica si puede supervisar el login de la persona.
        /// </summary>
        public bool CanSuperviseLogin { get; set; } = false;

        /// <summary>
        /// Indica si la relacion esta activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Profesional asignado.
        /// </summary>
        public virtual Professional Professional { get; set; } = null!;

        /// <summary>
        /// Persona con discapacidad.
        /// </summary>
        public virtual PersonWithDisability Person { get; set; } = null!;
    }
}
