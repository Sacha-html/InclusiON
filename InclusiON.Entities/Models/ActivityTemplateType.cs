using InclusiON.Entities.Models.BaseEntities;

namespace InclusiON.Entities.Models
{
    /// <summary>
    /// Tipo de plantilla que define la estructura y comportamiento de una actividad interactiva.
    /// Cada tipo mapea a un componente Angular especifico y tiene un JSON Schema para validar el contenido.
    /// Valores de Code: SELECT_FIGURE, VISUAL_SUM, COMPLETE_LETTER, ORDER_SEQUENCE, MATCH_IMAGE_WORD.
    /// </summary>
    public class ActivityTemplateType : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del tipo de plantilla.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del area de habilidad a la que pertenece esta plantilla.
        /// </summary>
        public int SkillAreaId { get; set; }

        /// <summary>
        /// Nombre descriptivo del tipo de plantilla (ej: "Seleccionar figura correcta").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Codigo unico que identifica el tipo de plantilla.
        /// Valores: SELECT_FIGURE | VISUAL_SUM | COMPLETE_LETTER | ORDER_SEQUENCE | MATCH_IMAGE_WORD.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion del proposito y mecanica de la plantilla.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// JSON Schema que define la estructura esperada del contenido (ContentJson en ActivityContent).
        /// Se usa para validar y generar formularios dinamicos en el frontend.
        /// </summary>
        public string ContentSchema { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del componente Angular que renderiza esta plantilla (ej: "SelectFigureComponent").
        /// </summary>
        public string ComponentName { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la plantilla utiliza pictogramas (ARASAAC u otros sistemas de CAA).
        /// </summary>
        public bool UsesPictograms { get; set; }

        /// <summary>
        /// Indica si la plantilla incluye soporte de audio (instrucciones habladas, feedback sonoro).
        /// </summary>
        public bool HasAudio { get; set; }

        /// <summary>
        /// Orden de presentacion en la interfaz.
        /// </summary>
        public int DisplayOrder { get; set; }

        #region Navegacion

        /// <summary>
        /// Area de habilidad a la que pertenece esta plantilla.
        /// </summary>
        public virtual SkillArea SkillArea { get; set; } = null!;

        /// <summary>
        /// Contenidos de actividad creados con esta plantilla.
        /// </summary>
        public virtual ICollection<ActivityContent> ActivityContents { get; set; }

        #endregion

        public ActivityTemplateType()
        {
            ActivityContents = new HashSet<ActivityContent>();
        }
    }
}
