namespace InclusiON.DTOs.Responses.Roadmap
{
    /// <summary>
    /// Actividad asignada dentro de un area del roadmap de una persona.
    /// </summary>
    public class RoadmapActivityResponse
    {
        /// <summary>ID de la entrada PersonRoadmapActivity.</summary>
        public int Id { get; set; }

        /// <summary>ID de la actividad referenciada.</summary>
        public int ActivityId { get; set; }

        /// <summary>Titulo de la actividad.</summary>
        public string ActivityTitle { get; set; } = string.Empty;

        /// <summary>Orden secuencial dentro del area.</summary>
        public int SequenceOrder { get; set; }

        /// <summary>Indica si la actividad esta desbloqueada.</summary>
        public bool IsUnlocked { get; set; }

        /// <summary>Fecha de desbloqueo. Null si aun esta bloqueada.</summary>
        public DateTime? UnlockedAt { get; set; }

        /// <summary>Porcentaje minimo de avance requerido para desbloquear la siguiente actividad.</summary>
        public int UnlockThresholdPercent { get; set; }

        /// <summary>Tiempo limite en segundos. Null = sin limite.</summary>
        public int? TimeLimitSeconds { get; set; }

        /// <summary>Numero maximo de intentos. Null = ilimitado.</summary>
        public int? MaxAttempts { get; set; }

        /// <summary>Indica si se muestran pistas durante la actividad.</summary>
        public bool ShowHints { get; set; }

        /// <summary>Nivel de dificultad. 1 = facil, 2 = medio, 3 = dificil.</summary>
        public int DifficultyLevel { get; set; }
    }
}
