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
        /// Estado de la asignacion (Pendiente, EnProgreso, Completada, Cancelada).
        /// </summary>
        public string Status { get; set; } = "Pendiente";

        /// <summary>
        /// Orden de secuencia si es parte de un programa estructurado.
        /// </summary>
        public int? SequenceOrder { get; set; }

        /// <summary>
        /// Indica si es una actividad de evaluacion diagnostica.
        /// </summary>
        public bool IsEvaluationActivity { get; set; } = false;

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
