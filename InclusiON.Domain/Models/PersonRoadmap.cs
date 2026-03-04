using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Hoja de ruta personalizada para una persona con discapacidad.
    /// Cada persona tiene un unico roadmap que organiza areas de habilidad y actividades secuenciales.
    /// Creado por un profesional responsable.
    /// </summary>
    public class PersonRoadmap : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del roadmap.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la persona con discapacidad. Relacion 1:1 (una persona tiene un solo roadmap).
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// ID del profesional que creo y diseno este roadmap.
        /// </summary>
        public Guid CreatedByProfessionalId { get; set; }

        /// <summary>
        /// Notas u observaciones del profesional sobre el plan de trabajo.
        /// </summary>
        public string? Notes { get; set; }

        #region Navegacion

        /// <summary>
        /// Persona con discapacidad a la que pertenece el roadmap.
        /// </summary>
        public virtual PersonWithDisability Person { get; set; } = null!;

        /// <summary>
        /// Profesional que creo el roadmap.
        /// </summary>
        public virtual Professional CreatedByProfessional { get; set; } = null!;

        /// <summary>
        /// Areas de habilidad incluidas en este roadmap.
        /// </summary>
        public virtual ICollection<PersonRoadmapArea> Areas { get; set; }

        #endregion

        public PersonRoadmap()
        {
            Areas = new HashSet<PersonRoadmapArea>();
        }
    }
}
