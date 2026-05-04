namespace InclusiON.DTOs.Requests.Roadmap
{
    public class AddRoadmapActivityRequest
    {
        /// <summary>ID de la actividad a asignar.</summary>
        public int ActivityId { get; set; }

        /// <summary>Orden secuencial dentro del area.</summary>
        public int SequenceOrder { get; set; }

        /// <summary>Porcentaje minimo de avance para desbloquear la siguiente actividad. Default: 60.</summary>
        public int UnlockThresholdPercent { get; set; } = 60;

        /// <summary>Tiempo limite en segundos. Null = sin limite.</summary>
        public int? TimeLimitSeconds { get; set; }

        /// <summary>Numero maximo de intentos. Null = ilimitado.</summary>
        public int? MaxAttempts { get; set; }

        /// <summary>Indica si se muestran pistas. Default: true.</summary>
        public bool ShowHints { get; set; } = true;

        /// <summary>Nivel de dificultad. 1 = facil, 2 = medio, 3 = dificil. Default: 1.</summary>
        public int DifficultyLevel { get; set; } = 1;
    }
}
