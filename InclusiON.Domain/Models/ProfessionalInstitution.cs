namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Relacion entre profesional e institucion educativa.
    /// Define donde trabaja cada profesional.
    /// </summary>
    public class ProfessionalInstitution
    {
        /// <summary>
        /// ID del profesional.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// ID de la institucion educativa.
        /// </summary>
        public int InstitutionId { get; set; }

        /// <summary>
        /// Fecha de asignacion a la institucion.
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si la relacion esta activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Profesional asignado.
        /// </summary>
        public virtual Professional Professional { get; set; } = null!;

        /// <summary>
        /// Institucion educativa.
        /// </summary>
        public virtual EducationalInstitution Institution { get; set; } = null!;
    }
}
