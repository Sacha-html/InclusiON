using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Institutions
{
    /// <summary>
    /// Request para actualizar una institucion educativa.
    /// </summary>
    public class UpdateInstitutionRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "La direccion no puede exceder 300 caracteres")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "El telefono no puede exceder 20 caracteres")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "El formato del email es invalido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? Email { get; set; }
    }
}
