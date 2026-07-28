using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Catalogs
{
    public class CreateActivityCategoryRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripcion no puede exceder 500 caracteres")]
        public string? Description { get; set; }
    }
}
