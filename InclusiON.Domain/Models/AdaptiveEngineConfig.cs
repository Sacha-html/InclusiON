using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Representar la configuracion del motor de dificultad adaptativa para una actividad del roadmap.
    /// Definir rangos y umbrales que el motor usa para ajustar automaticamente la dificultad.
    /// Relacion 1:0..1 con PersonRoadmapActivity (opcional: si no existe, la actividad opera sin motor).
    /// </summary>
    public class AdaptiveEngineConfig : AuditableBaseEntity
    {
        /// <summary>
        /// Identificar de forma unica la configuracion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Referenciar la actividad del roadmap personalizado. Relacion 1:0..1.
        /// </summary>
        public int PersonRoadmapActivityId { get; set; }

        /// <summary>
        /// Habilitar o deshabilitar el motor sin eliminar la configuracion.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #region Rangos de dificultad

        /// <summary>
        /// Establecer el piso de dificultad. El motor nunca baja por debajo de este valor.
        /// </summary>
        public int MinDifficultyLevel { get; set; } = 1;

        /// <summary>
        /// Establecer el techo de dificultad. El motor nunca sube por encima de este valor.
        /// </summary>
        public int MaxDifficultyLevel { get; set; } = 5;

        #endregion

        #region Rangos de tiempo

        /// <summary>
        /// Definir el tiempo minimo en segundos. null = sin limite inferior.
        /// </summary>
        public int? MinTimeLimitSeconds { get; set; }

        /// <summary>
        /// Definir el tiempo maximo en segundos. null = sin limite de tiempo.
        /// </summary>
        public int? MaxTimeLimitSeconds { get; set; }

        #endregion

        #region Umbrales del motor

        /// <summary>
        /// Indicar los aciertos consecutivos necesarios para subir dificultad.
        /// </summary>
        public int ConsecutiveSuccessToUpgrade { get; set; } = 3;

        /// <summary>
        /// Indicar los fallos consecutivos necesarios para bajar dificultad.
        /// </summary>
        public int ConsecutiveFailuresToDowngrade { get; set; } = 2;

        /// <summary>
        /// Definir el porcentaje minimo de exito para considerar un intento como aprobado.
        /// </summary>
        public int SuccessThresholdPercent { get; set; } = 70;

        /// <summary>
        /// Establecer el nivel de frustracion (1-5) que dispara intervencion inmediata.
        /// </summary>
        public int FrustrationThreshold { get; set; } = 3;

        #endregion

        #region Navegacion

        /// <summary>
        /// Obtener la actividad del roadmap asociada.
        /// </summary>
        public virtual PersonRoadmapActivity PersonRoadmapActivity { get; set; } = null!;

        #endregion
    }
}
