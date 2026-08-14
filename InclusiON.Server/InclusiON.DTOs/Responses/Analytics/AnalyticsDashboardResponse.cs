namespace InclusiON.DTOs.Responses.Analytics
{
    /// <summary>
    /// Respuesta analítica global o por aula para Dashboards de Profesionales y Administradores.
    /// </summary>
    public class AnalyticsDashboardResponse
    {
        #region KPIs Globales (Scorecards)
        /// <summary>
        /// Promedio global de la puntuación cualitativa Goal Attainment Scaling (-2 a +2).
        /// </summary>
        public decimal Promedio_GAS { get; set; }

        /// <summary>
        /// Tiempo promedio dedicado a cada nivel en segundos.
        /// </summary>
        public double Tiempo_Promedio_Nivel { get; set; }

        /// <summary>
        /// Total de personas/alumnos activos considerados en la métrica.
        /// </summary>
        public int PersonasActivas { get; set; }

        /// <summary>
        /// Total de sesiones de actividades completadas.
        /// </summary>
        public int TotalActividadesCompletadas { get; set; }

        /// <summary>
        /// Porcentaje promedio de éxito general (0-100%).
        /// </summary>
        public decimal PromedioExito { get; set; }

        /// <summary>
        /// Cantidad de alertas o situaciones de frustración detectadas (ej. éxito menor a 40% o estancamiento).
        /// </summary>
        public int AlertasFrustracion { get; set; }
        #endregion

        #region Métricas para Gráficos
        /// <summary>
        /// Distribución actual de alumnos por nivel del Roadmap (Nivel 1 al 10).
        /// </summary>
        public List<LevelDistributionItem> Distribucion_Por_Nivel { get; set; } = new();

        /// <summary>
        /// Tasa de abandono / estancamiento por cada nivel del Roadmap.
        /// </summary>
        public List<LevelDropoutRateItem> Tasa_Abandono_Por_Nivel { get; set; } = new();

        /// <summary>
        /// Porcentaje de alumnos que completaron dentro del tiempo estimado vs los que tardaron más.
        /// </summary>
        public TimeProgressItem Avance_Tiempo_Estimado { get; set; } = new();

        /// <summary>
        /// Rendimiento y tasa de éxito agrupado por categoría pedagógica.
        /// </summary>
        public List<CategoryPerformanceItem> Rendimiento_Por_Categoria { get; set; } = new();

        /// <summary>
        /// Ranking comparativo de rendimiento entre las distintas aulas del profesional (visible en vista global).
        /// </summary>
        public List<ClassroomRankingItem> RankingMisAulas { get; set; } = new();
        #endregion
    }

    public class ClassroomRankingItem
    {
        public Guid ClassroomId { get; set; }
        public string NombreAula { get; set; } = string.Empty;
        public int TotalAlumnos { get; set; }
        public decimal PromedioExitoAula { get; set; }
        public int TotalSesiones { get; set; }
    }

    public class LevelDistributionItem
    {
        public int Nivel { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public int AlumnosEstancados { get; set; }
        public int AlumnosSuperaron { get; set; }
        public int TotalAlumnos { get; set; }
    }

    public class LevelDropoutRateItem
    {
        public int Nivel { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public decimal AbandonoPct { get; set; }
        public int AlumnosAbandono { get; set; }
        public int TotalIntentos { get; set; }
    }

    public class TimeProgressItem
    {
        public decimal DentroDeTiempoPct { get; set; }
        public decimal MasTiempoPct { get; set; }
        public int TotalSesionesAnalizadas { get; set; }
    }

    public class CategoryPerformanceItem
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal PromedioExito { get; set; }
        public int TotalSesiones { get; set; }
        public string Color { get; set; } = "#1A237E";
    }
}
