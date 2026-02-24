using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Actividad pedagogica o terapeutica.
    /// Creada por profesionales para asignar a personas con discapacidad.
    /// Incluye configuracion de accesibilidad y control de estimulacion.
    /// </summary>
    public class Activity : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico de la actividad.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del profesional que creo la actividad.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// ID de la categoria de la actividad.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Titulo descriptivo de la actividad.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion detallada de la actividad y sus objetivos.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Instrucciones paso a paso para realizar la actividad.
        /// </summary>
        public string? Instructions { get; set; }

        #region Configuracion de Accesibilidad
        /// <summary>
        /// Indica si incluye soporte visual (imagenes, videos).
        /// </summary>
        public bool HasVisualSupport { get; set; } = false;

        /// <summary>
        /// Indica si incluye soporte auditivo (audio, narracion).
        /// </summary>
        public bool HasAudioSupport { get; set; } = false;

        /// <summary>
        /// Indica si usa lectura facil (textos simplificados).
        /// </summary>
        public bool UsesEasyReading { get; set; } = false;

        /// <summary>
        /// Indica si usa pictogramas para comunicacion.
        /// </summary>
        public bool UsesPictograms { get; set; } = false;

        /// <summary>
        /// URL de recursos adicionales (archivos, multimedia).
        /// </summary>
        public string? ResourcesUrl { get; set; }
        #endregion

        public int? SkillAreaId { get; set; }
        public virtual SkillArea? SkillArea { get; set; }

        #region Control de Estimulacion
        /// <summary>
        /// Duracion estimada en minutos.
        /// </summary>
        public int? EstimatedDurationMinutes { get; set; }

        /// <summary>
        /// Nivel de complejidad (1-5).
        /// </summary>
        public int? ComplexityLevel { get; set; }

        /// <summary>
        /// Indica si requiere supervision durante la ejecucion.
        /// </summary>
        public bool RequiresSupervision { get; set; } = true;

        /// <summary>
        /// Indica si es una actividad estandar del sistema (no editable).
        /// </summary>
        public bool IsStandardActivity { get; set; } = false;
        #endregion

        /// <summary>
        /// Profesional creador de la actividad.
        /// </summary>
        public virtual Professional Professional { get; set; } = null!;

        /// <summary>
        /// Categoria de la actividad.
        /// </summary>
        public virtual ActivityCategory Category { get; set; } = null!;

        /// <summary>
        /// Asignaciones de esta actividad a personas.
        /// </summary>
        public virtual ICollection<ActivityAssignment> ActivityAssignments { get; set; }

        public virtual ActivityContent? Content { get; set; }

        public virtual ICollection<PersonRoadmapActivity> RoadmapActivities { get; set; }

        public Activity()
        {
            ActivityAssignments = new HashSet<ActivityAssignment>();
            RoadmapActivities = new HashSet<PersonRoadmapActivity>();
        }
    }
}
