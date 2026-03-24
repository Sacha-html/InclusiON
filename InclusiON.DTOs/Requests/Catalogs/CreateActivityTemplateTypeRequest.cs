using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Catalogs
{
    public class CreateActivityTemplateTypeRequest
    {
        [Required(ErrorMessage = "El area de habilidad es requerida")]
        public int SkillAreaId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El codigo es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El codigo debe tener entre 2 y 50 caracteres")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripcion no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El esquema de contenido es requerido")]
        public string ContentSchema { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del componente es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del componente debe tener entre 2 y 100 caracteres")]
        public string ComponentName { get; set; } = string.Empty;

        public bool UsesPictograms { get; set; }

        public bool HasAudio { get; set; }

        public int DisplayOrder { get; set; }
    }
}
