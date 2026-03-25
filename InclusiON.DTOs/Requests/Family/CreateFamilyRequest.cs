using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Family
{
    public class CreateFamilyRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es invalido")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El documento no puede exceder 20 caracteres")]
        public string? DocumentNumber { get; set; }

        [StringLength(20, ErrorMessage = "El telefono no puede exceder 20 caracteres")]
        public string? Phone { get; set; }

        [StringLength(50, ErrorMessage = "El parentesco no puede exceder 50 caracteres")]
        public string? Relationship { get; set; }

        [Required(ErrorMessage = "Debe indicar la persona a la que representa")]
        public Guid PersonId { get; set; }
    }
}
