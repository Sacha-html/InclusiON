using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Activities
{
    public class CompleteActivityResponseRequest
    {
        [Required]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public decimal SuccessPercentage { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int TimeSpentSeconds { get; set; }

        public bool RequiredSupport { get; set; } = false;

        [Range(1, 5)]
        public int? FrustrationLevel { get; set; }

        public string? ResponsePattern { get; set; }
        public string? Observations { get; set; }
    }
}
