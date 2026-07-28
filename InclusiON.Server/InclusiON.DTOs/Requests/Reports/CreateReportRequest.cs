using System;
using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Reports
{
    public class CreateReportRequest
    {
        [Required]
        public Guid PersonId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es requerido")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int ReportTypeId { get; set; }

        [Required]
        public DateTime ReportDate { get; set; }

        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }

        [StringLength(2000)]
        public string? AchievedGoals { get; set; }

        [StringLength(2000)]
        public string? AreasToReinforce { get; set; }

        [StringLength(2000)]
        public string? FutureRecommendations { get; set; }

        [StringLength(2000)]
        public string? NextObjectives { get; set; }
    }
}