using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Registra la sesión de ejecución y métricas analíticas de una actividad finalizada por un alumno.
    /// Utilizado para alimentar dashboards de KPIs pedagógicos (Goal Attainment Scaling, tasa de éxito, tiempos y errores).
    /// </summary>
    public class ActivitySession : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador único de la sesión.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del alumno (Persona con discapacidad) que ejecutó la actividad.
        /// </summary>
        public Guid StudentId { get; set; }
        public virtual PersonWithDisability Student { get; set; } = null!;

        /// <summary>
        /// ID del profesional responsable o supervisor del alumno.
        /// </summary>
        public Guid ProfessionalId { get; set; }
        public virtual Professional Professional { get; set; } = null!;

        /// <summary>
        /// ID de la actividad ejecutada.
        /// </summary>
        public int ActivityId { get; set; }
        public virtual Activity Activity { get; set; } = null!;

        /// <summary>
        /// Fecha y hora en la que se completó la sesión.
        /// </summary>
        public DateTime DateCompleted { get; set; }

        /// <summary>
        /// Porcentaje de éxito obtenido en la sesión (40 - 100%).
        /// </summary>
        public decimal SuccessRate { get; set; }

        /// <summary>
        /// Cantidad de errores cometidos durante la sesión (0 - 6).
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Tiempo total dedicado a la actividad en segundos (30 - 300s).
        /// </summary>
        public int TimeSpentSeconds { get; set; }

        /// <summary>
        /// Puntuación Goal Attainment Scaling (GAS): rango [-2, +2].
        /// -2: Mucho menor que lo esperado
        /// -1: Menor que lo esperado
        ///  0: Nivel esperado de logro
        /// +1: Mayor que lo esperado
        /// +2: Mucho mayor que lo esperado
        /// </summary>
        public int GasScore { get; set; }
    }
}
