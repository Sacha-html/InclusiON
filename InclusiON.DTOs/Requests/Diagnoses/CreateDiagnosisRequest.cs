using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Diagnoses
{
    public class CreateDiagnosisRequest
    {
        [Required(ErrorMessage = "La fecha del diagnóstico es requerida")]
        public DateTime DiagnosisDate { get; set; }

        [Required(ErrorMessage = "El diagnóstico principal es requerido")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "El diagnóstico debe tener entre 5 y 500 caracteres")]
        public string PrimaryDiagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? InitialObservations { get; set; }

        [StringLength(2000)]
        public string? IdentifiedCapabilities { get; set; }

        [StringLength(2000)]
        public string? IdentifiedChallenges { get; set; }

        [StringLength(2000)]
        public string? RequiredSupports { get; set; }

        [StringLength(2000)]
        public string? PedagogicalObjectives { get; set; }

        [StringLength(2000)]
        public string? RecommendedStrategies { get; set; }
    }
}
