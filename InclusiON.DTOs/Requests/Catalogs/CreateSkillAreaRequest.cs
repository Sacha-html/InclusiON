using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Catalogs
{
    public class CreateSkillAreaRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripcion no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "El icono no puede exceder 50 caracteres")]
        public string? Icon { get; set; }

        [StringLength(20, ErrorMessage = "El color no puede exceder 20 caracteres")]
        public string? Color { get; set; }

        public int DisplayOrder { get; set; }
    }
}
