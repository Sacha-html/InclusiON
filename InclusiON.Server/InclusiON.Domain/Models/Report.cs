using InclusiON.Domain.Attributes;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Reporte de seguimiento o evaluacion de una persona con discapacidad.
    /// Generado por profesionales como borrador → enviado al admin → aprobado/rechazado.
    /// </summary>
    public class Report : AuditableBaseEntity
    {
        public int Id { get; set; }
        public Guid PersonId { get; set; }
        public Guid ProfessionalId { get; set; }
        public int ReportTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        [Encrypted]
        public string Content { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        [Encrypted]
        public string? AchievedGoals { get; set; }
        [Encrypted]
        public string? AreasToReinforce { get; set; }
        [Encrypted]
        public string? FutureRecommendations { get; set; }
        [Encrypted]
        public string? NextObjectives { get; set; }

        // ── Flujo de aprobación ──────────────────────────────────────────
        /// <summary>Estado del reporte en el flujo de revisión.</summary>
        public ReportStatus Status { get; set; } = ReportStatus.Draft;

        /// <summary>Comentario del admin al aprobar o rechazar.</summary>
        public string? AdminComment { get; set; }

        /// <summary>Fecha en que el admin aprobó el reporte.</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>ID del admin que aprobó el reporte.</summary>
        public Guid? ApprovedBy { get; set; }

        /// <summary>
        /// Indica si el familiar ya leyó el reporte aprobado.
        /// Se pone en false cuando el admin aprueba; pasa a true al abrir el detalle.
        /// </summary>
        public bool IsReadByFamily { get; set; } = false;

        // ── Navegación ──────────────────────────────────────────────────
        public virtual PersonWithDisability Person { get; set; } = null!;
        public virtual Professional Professional { get; set; } = null!;
        public virtual ReportType ReportType { get; set; } = null!;
    }
}
