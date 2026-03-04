using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Reporte de seguimiento o evaluacion de una persona con discapacidad.
    /// Generado por profesionales para documentar progreso, logros y recomendaciones.
    /// </summary>
    public class Report : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del reporte.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la persona sobre quien se genera el reporte.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// ID del profesional que genera el reporte.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// ID del tipo de reporte.
        /// </summary>
        public int ReportTypeId { get; set; }

        /// <summary>
        /// Titulo del reporte.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Contenido principal del reporte.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Fecha del reporte.
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Fecha de inicio del periodo evaluado.
        /// </summary>
        public DateTime? PeriodStartDate { get; set; }

        /// <summary>
        /// Fecha de fin del periodo evaluado.
        /// </summary>
        public DateTime? PeriodEndDate { get; set; }

        /// <summary>
        /// Objetivos alcanzados durante el periodo.
        /// </summary>
        public string? AchievedGoals { get; set; }

        /// <summary>
        /// Areas que requieren refuerzo.
        /// </summary>
        public string? AreasToReinforce { get; set; }

        /// <summary>
        /// Recomendaciones para el futuro.
        /// </summary>
        public string? FutureRecommendations { get; set; }

        /// <summary>
        /// Proximos objetivos a trabajar.
        /// </summary>
        public string? NextObjectives { get; set; }

        /// <summary>
        /// Persona sobre quien se genera el reporte.
        /// </summary>
        public virtual PersonWithDisability Person { get; set; } = null!;

        /// <summary>
        /// Profesional que genera el reporte.
        /// </summary>
        public virtual Professional Professional { get; set; } = null!;

        /// <summary>
        /// Tipo de reporte.
        /// </summary>
        public virtual ReportType ReportType { get; set; } = null!;
    }
}
