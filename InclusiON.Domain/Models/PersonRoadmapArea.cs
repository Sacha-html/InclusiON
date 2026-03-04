using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Area de habilidad dentro del roadmap personalizado de una persona.
    /// Un area no puede repetirse en el mismo roadmap (indice unico PersonRoadmapId + SkillAreaId).
    /// Contiene las actividades asignadas en orden secuencial.
    /// </summary>
    public class PersonRoadmapArea : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del area dentro del roadmap.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del roadmap al que pertenece esta area.
        /// </summary>
        public int PersonRoadmapId { get; set; }

        /// <summary>
        /// ID del area de habilidad referenciada.
        /// </summary>
        public int SkillAreaId { get; set; }

        /// <summary>
        /// Orden de presentacion del area dentro del roadmap.
        /// </summary>
        public int DisplayOrder { get; set; }

        #region Navegacion

        /// <summary>
        /// Roadmap al que pertenece esta area.
        /// </summary>
        public virtual PersonRoadmap PersonRoadmap { get; set; } = null!;

        /// <summary>
        /// Area de habilidad referenciada.
        /// </summary>
        public virtual SkillArea SkillArea { get; set; } = null!;

        /// <summary>
        /// Actividades asignadas dentro de esta area del roadmap.
        /// </summary>
        public virtual ICollection<PersonRoadmapActivity> Activities { get; set; }

        #endregion

        public PersonRoadmapArea()
        {
            Activities = new HashSet<PersonRoadmapActivity>();
        }
    }
}
