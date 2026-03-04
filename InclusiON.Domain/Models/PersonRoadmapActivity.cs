using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Actividad asignada dentro de un area del roadmap personalizado.
    /// Incluye configuracion de desbloqueo progresivo y parametros personalizados por persona.
    /// Una actividad no puede repetirse en la misma area (indice unico PersonRoadmapAreaId + ActivityId).
    /// </summary>
    public class PersonRoadmapActivity : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico de la asignacion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del area del roadmap a la que pertenece esta actividad.
        /// </summary>
        public int PersonRoadmapAreaId { get; set; }

        /// <summary>
        /// ID de la actividad asignada.
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// Orden secuencial de la actividad dentro del area. Determina la progresion.
        /// </summary>
        public int SequenceOrder { get; set; }

        #region Desbloqueo

        /// <summary>
        /// Indica si la actividad esta desbloqueada y disponible para la persona.
        /// </summary>
        public bool IsUnlocked { get; set; } = false;

        /// <summary>
        /// Fecha y hora en que se desbloqueo la actividad. Null si aun esta bloqueada.
        /// </summary>
        public DateTime? UnlockedAt { get; set; }

        /// <summary>
        /// Porcentaje minimo de avance requerido en esta actividad para desbloquear la siguiente.
        /// Valor por defecto: 60%.
        /// </summary>
        public int UnlockThresholdPercent { get; set; } = 60;

        #endregion

        #region Configuracion Personalizada

        /// <summary>
        /// Tiempo limite en segundos para completar la actividad. Null = sin limite.
        /// </summary>
        public int? TimeLimitSeconds { get; set; }

        /// <summary>
        /// Numero maximo de intentos permitidos. Null = ilimitado.
        /// </summary>
        public int? MaxAttempts { get; set; }

        /// <summary>
        /// Indica si se muestran pistas durante la actividad.
        /// </summary>
        public bool ShowHints { get; set; } = true;

        /// <summary>
        /// Nivel de dificultad personalizado. 1 = facil, 2 = medio, 3 = dificil.
        /// </summary>
        public int DifficultyLevel { get; set; } = 1;

        #endregion

        #region Navegacion

        /// <summary>
        /// Area del roadmap a la que pertenece esta actividad.
        /// </summary>
        public virtual PersonRoadmapArea PersonRoadmapArea { get; set; } = null!;

        /// <summary>
        /// Actividad asignada.
        /// </summary>
        public virtual Activity Activity { get; set; } = null!;

        /// <summary>
        /// Configuracion del motor de dificultad adaptativa. Null si la actividad opera sin motor.
        /// </summary>
        public virtual AdaptiveEngineConfig? AdaptiveConfig { get; set; }

        #endregion

        public virtual ICollection<ActivityResult> ActivityResults { get; set; } = new List<ActivityResult>();
        public virtual ICollection<AdaptiveAdjustmentLog> AdaptiveAdjustmentLogs { get; set; } = new List<AdaptiveAdjustmentLog>();
    }
}
