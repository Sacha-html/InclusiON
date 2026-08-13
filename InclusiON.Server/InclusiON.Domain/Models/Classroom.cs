using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Representa un aula asociada a un profesional.
    /// Contiene un grupo de alumnos asignados.
    /// </summary>
    public class Classroom : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador único del aula.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del aula.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ID del profesional responsable.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// Indica si el aula está activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Profesional responsable del aula.
        /// </summary>
        public virtual Professional Professional { get; set; } = null!;

        /// <summary>
        /// Personas con discapacidad (alumnos) asignados a este aula.
        /// </summary>
        public virtual ICollection<ProfessionalPerson> ProfessionalPersons { get; set; }

        public Classroom()
        {
            ProfessionalPersons = new HashSet<ProfessionalPerson>();
        }
    }
}
