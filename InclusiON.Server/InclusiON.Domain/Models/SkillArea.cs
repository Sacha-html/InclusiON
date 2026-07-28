using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Area de habilidad que agrupa actividades y plantillas por dominio de desarrollo.
    /// Ejemplos: Comunicacion, Motricidad, Lectoescritura, etc.
    /// </summary>
    public class SkillArea : AuditableBaseEntity, IHasIntId
    {
        /// <summary>
        /// Identificador unico del area de habilidad.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del area de habilidad. Debe ser unico.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion del proposito y alcance del area.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Nombre del icono representativo (ej: "chat", "brush", "calculate").
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Color hexadecimal para identificacion visual (ej: "#2E5FA3").
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Orden de presentacion en la interfaz.
        /// </summary>
        public int DisplayOrder { get; set; }

        #region Navegacion

        /// <summary>
        /// Tipos de plantilla disponibles en esta area.
        /// </summary>
        public virtual ICollection<ActivityTemplateType> TemplateTypes { get; set; }

        /// <summary>
        /// Actividades clasificadas en esta area.
        /// </summary>
        public virtual ICollection<Activity> Activities { get; set; }

        /// <summary>
        /// Areas de roadmap personalizadas que referencian esta area de habilidad.
        /// </summary>
        public virtual ICollection<PersonRoadmapArea> RoadmapAreas { get; set; }

        #endregion

        public SkillArea()
        {
            TemplateTypes = new HashSet<ActivityTemplateType>();
            Activities = new HashSet<Activity>();
            RoadmapAreas = new HashSet<PersonRoadmapArea>();
        }
    }
}
