namespace InclusiON.DTOs.Responses.Family
{
    /// <summary>
    /// Resumen de una persona vinculada para el dashboard familiar.
    /// </summary>
    public class FamilyPersonSummaryResponse
    {
        public Guid   PersonId       { get; set; }
        public string FullName       { get; set; } = string.Empty;
        public string? AvatarColor   { get; set; }

        /// <summary>Últimas 3 actividades completadas.</summary>
        public List<RecentActivityResultResponse> RecentActivities { get; set; } = new();

        public int    ApprovedReportsCount { get; set; }
        public string? LatestReportTitle   { get; set; }
        public DateTime? LatestReportDate  { get; set; }

        /// <summary>Nivel actual alcanzado en el Roadmap curricular (1 al 10).</summary>
        public int CurrentRoadmapLevel { get; set; } = 1;

        /// <summary>Nombre de la actividad correspondiente al nivel del Roadmap.</summary>
        public string RoadmapLevelName { get; set; } = string.Empty;

        /// <summary>Semáforo cualitativo de logro pedagógico amigable basado en GAS.</summary>
        public string GasStatusLabel { get; set; } = "Iniciando camino";

        /// <summary>Porcentaje promedio general de éxito.</summary>
        public decimal AverageSuccessRate { get; set; }

        /// <summary>Indica si el alumno presenta alerta de frustración activa (4 fallas consecutivas).</summary>
        public bool HasFrustrationAlert { get; set; }

        /// <summary>Detalle de los 10 niveles de Mi Camino con su estado cromático (completed, struggling, active, locked).</summary>
        public List<RoadmapLevelStepResponse> RoadmapLevels { get; set; } = new();
    }

    /// <summary>
    /// Nivel individual del Stepper del Roadmap en el panel familiar.
    /// </summary>
    public class RoadmapLevelStepResponse
    {
        public int Level { get; set; }
        public string Title { get; set; } = string.Empty;
        /// <summary>"completed" (verde), "struggling" (naranja), "active" (azul), "locked" (gris)</summary>
        public string Status { get; set; } = "locked";
    }
}
