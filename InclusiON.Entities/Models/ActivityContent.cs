using InclusiON.Entities.Models.BaseEntities;

namespace InclusiON.Entities.Models
{
    /// <summary>
    /// Contenido interactivo de una actividad. Relacion 1:1 con Activity.
    /// Almacena el JSON del contenido real segun el schema definido en el ActivityTemplateType.
    /// </summary>
    public class ActivityContent : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del contenido.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la actividad asociada. Relacion 1:1 (una actividad tiene un solo contenido).
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// ID del tipo de plantilla que define la estructura del contenido.
        /// </summary>
        public int TemplateTypeId { get; set; }

        /// <summary>
        /// Contenido real de la actividad en formato JSON, validado contra ContentSchema del TemplateType.
        /// Incluye opciones, imagenes, respuestas correctas, textos, etc.
        /// </summary>
        public string ContentJson { get; set; } = string.Empty;

        #region Navegacion

        /// <summary>
        /// Actividad a la que pertenece este contenido.
        /// </summary>
        public virtual Activity Activity { get; set; } = null!;

        /// <summary>
        /// Tipo de plantilla que define la estructura esperada del contenido.
        /// </summary>
        public virtual ActivityTemplateType TemplateType { get; set; } = null!;

        #endregion
    }
}
