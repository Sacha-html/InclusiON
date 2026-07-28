using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Registrar cada ajuste realizado por el motor de dificultad adaptativa.
    /// Permitir al profesional consultar el historial completo de adaptaciones.
    /// </summary>
    public class AdaptiveAdjustmentLog : AuditableBaseEntity
    {
        /// <summary>
        /// Identificar de forma unica el registro de ajuste.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Referenciar la actividad del roadmap que fue ajustada.
        /// </summary>
        public int PersonRoadmapActivityId { get; set; }

        /// <summary>
        /// Referenciar la respuesta que disparo el ajuste.
        /// </summary>
        public int ActivityResponseId { get; set; }

        /// <summary>
        /// Indicar el tipo de ajuste realizado.
        /// Valores: DifficultyUp, DifficultyDown, HintsEnabled, HintsDisabled,
        /// TimeLimitIncreased, TimeLimitDecreased, AttemptsIncreased, FrustrationIntervention.
        /// </summary>
        public string AdjustmentType { get; set; } = null!;

        /// <summary>
        /// Almacenar el valor anterior serializado como JSON. Ej: {"DifficultyLevel": 2}
        /// </summary>
        public string PreviousValue { get; set; } = null!;

        /// <summary>
        /// Almacenar el valor nuevo serializado como JSON. Ej: {"DifficultyLevel": 3}
        /// </summary>
        public string NewValue { get; set; } = null!;

        /// <summary>
        /// Describir la razon del ajuste. Ej: "3 aciertos consecutivos con SuccessPercentage >= 70%"
        /// </summary>
        public string Reason { get; set; } = null!;

        /// <summary>
        /// Registrar el timestamp UTC del ajuste.
        /// </summary>
        public DateTime AdjustedAt { get; set; }

        #region Navegacion

        /// <summary>
        /// Obtener la actividad del roadmap que fue ajustada.
        /// </summary>
        public virtual PersonRoadmapActivity PersonRoadmapActivity { get; set; } = null!;

        /// <summary>
        /// Obtener la respuesta que disparo el ajuste.
        /// </summary>
        public virtual ActivityResponse ActivityResponse { get; set; } = null!;

        #endregion
    }
}
