using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Asignacion de una actividad a una persona con discapacidad.
    /// Registra quien asigno la actividad, cuando y con que fecha limite.
    /// </summary>
    public class ActivityAssignment : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico de la asignacion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la actividad asignada.
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// ID de la persona a quien se asigna.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// ID del profesional que realiza la asignacion.
        /// </summary>
        public Guid AssignedByProfessionalId { get; set; }

        /// <summary>
        /// Fecha y hora de la asignacion.
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha limite para completar la actividad.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// ID del estado de la asignación (FK a ActivityAssignmentStatuses).
        /// </summary>
        public int StatusId { get; set; } = 1; // Pendiente

        /// <summary>
        /// Estado de la asignación.
        /// </summary>
        public virtual ActivityAssignmentStatus Status { get; set; } = null!;

        /// <summary>
        /// Orden de secuencia si es parte de un programa estructurado.
        /// </summary>
        public int? SequenceOrder { get; set; }

        /// <summary>
        /// Indica si es una actividad de evaluacion diagnostica.
        /// </summary>
        public bool IsEvaluationActivity { get; set; } = false;

        /// <summary>
        /// Fecha y hora en la que el profesional reconoció/atendió la alerta de frustración/4 intentos.
        /// </summary>
        public DateTime? AlertAcknowledgedAt { get; set; }

        /// <summary>
        /// Duración estimada adaptada para el alumno en minutos (sobrescribe la de la plantilla si está definida).
        /// </summary>
        public int? EstimatedDurationMinutes { get; set; }

        /// <summary>
        /// Adaptación de soporte visual para este alumno (sobrescribe la de la plantilla si está definida).
        /// </summary>
        public bool? HasVisualSupport { get; set; }

        /// <summary>
        /// Adaptación de soporte auditivo para este alumno (sobrescribe la de la plantilla si está definida).
        /// </summary>
        public bool? HasAudioSupport { get; set; }

        /// <summary>
        /// Adaptación de lectura fácil para este alumno (sobrescribe la de la plantilla si está definida).
        /// </summary>
        public bool? UsesEasyReading { get; set; }

        /// <summary>
        /// Adaptación de pictogramas para este alumno (sobrescribe la de la plantilla si está definida).
        /// </summary>
        public bool? UsesPictograms { get; set; }

        /// <summary>
        /// Adaptación de requerimiento de supervisión para este alumno (sobrescribe la de la plantilla si está definida).
        /// </summary>
        public bool? RequiresSupervision { get; set; }

        /// <summary>
        /// Notas de adaptación personalizada o ajustes realizados por el profesional para este alumno.
        /// </summary>
        public string? CustomAdaptationNotes { get; set; }

        /// <summary>
        /// Actividad asignada.
        /// </summary>
        public virtual Activity Activity { get; set; } = null!;

        /// <summary>
        /// Persona a quien se asigna la actividad.
        /// </summary>
        public virtual PersonWithDisability Person { get; set; } = null!;

        /// <summary>
        /// Profesional que realizo la asignacion.
        /// </summary>
        public virtual Professional AssignedByProfessional { get; set; } = null!;

        /// <summary>
        /// Respuestas/intentos de la persona en esta actividad.
        /// </summary>
        public virtual ICollection<ActivityResponse> Responses { get; set; }

        public ActivityAssignment()
        {
            Responses = new HashSet<ActivityResponse>();
        }
    }
}
