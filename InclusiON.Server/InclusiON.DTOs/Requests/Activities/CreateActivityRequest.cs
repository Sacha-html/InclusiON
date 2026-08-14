using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Activities
{
    public class CreateActivityRequest
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }

        [StringLength(2000, ErrorMessage = "Las instrucciones no pueden exceder 2000 caracteres")]
        public string? Instructions { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoryId { get; set; }

        public int? SkillAreaId { get; set; }

        [Range(1, 5, ErrorMessage = "La complejidad debe estar entre 1 y 5")]
        public int? ComplexityLevel { get; set; }

        [Range(1, 600, ErrorMessage = "La duración estimada debe estar entre 1 y 600 minutos")]
        public int? EstimatedDurationMinutes { get; set; }

        public bool RequiresSupervision { get; set; } = false;
        public bool HasVisualSupport { get; set; } = false;
        public bool HasAudioSupport { get; set; } = false;
        public bool UsesEasyReading { get; set; } = false;
        public bool UsesPictograms { get; set; } = false;

        [StringLength(500, ErrorMessage = "La URL de recursos no puede exceder 500 caracteres")]
        public string? ResourcesUrl { get; set; }

        [Required(ErrorMessage = "El tipo de template es requerido")]
        public int TemplateTypeId { get; set; }

        [Required(ErrorMessage = "El contenido es requerido")]
        public string ContentJson { get; set; } = string.Empty;

        public bool IsTemplate { get; set; } = false;
    }
}
