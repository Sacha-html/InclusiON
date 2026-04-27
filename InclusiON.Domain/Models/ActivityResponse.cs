using InclusiON.Domain.Attributes;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Respuesta o intento de una persona en una actividad asignada.
    /// Registra metricas de desempeno, tiempo, resultados y observaciones.
    /// </summary>
    public class ActivityResponse : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico de la respuesta.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la asignacion a la que pertenece esta respuesta.
        /// </summary>
        public int AssignmentId { get; set; }

        /// <summary>
        /// Fecha y hora de inicio del intento.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Fecha y hora de finalizacion del intento.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Tiempo total en segundos dedicado a la actividad.
        /// </summary>
        public int? TimeSpentSeconds { get; set; }

        /// <summary>
        /// Resultado del intento (Exito, Parcial, Fallido, Abandonado).
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// Porcentaje de exito (0-100).
        /// </summary>
        public decimal? SuccessPercentage { get; set; }

        /// <summary>
        /// Numero de intento (1, 2, 3...).
        /// </summary>
        public int AttemptCount { get; set; } = 1;

        /// <summary>
        /// Patron de respuestas en formato JSON para analisis.
        /// </summary>
        [Encrypted]
        public string? ResponsePattern { get; set; }

        /// <summary>
        /// Indica si requirio ayuda durante la actividad.
        /// </summary>
        public bool RequiredSupport { get; set; } = false;

        /// <summary>
        /// Nivel de frustracion observado (1-5).
        /// </summary>
        public int? FrustrationLevel { get; set; }

        /// <summary>
        /// Observaciones del profesional o sistema sobre el intento.
        /// </summary>
        [Encrypted]
        public string? Observations { get; set; }

        /// <summary>
        /// Asignacion a la que pertenece esta respuesta.
        /// </summary>
        public virtual ActivityAssignment Assignment { get; set; } = null!;
        public virtual ICollection<AdaptiveAdjustmentLog> AdaptiveAdjustmentLogs { get; set; } = new List<AdaptiveAdjustmentLog>();
    }
}
