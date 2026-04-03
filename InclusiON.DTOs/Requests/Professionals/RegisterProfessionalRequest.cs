using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Professionals
{
    /// <summary>
    /// Request para registro público de un profesional.
    /// El profesional se registra y queda pendiente de validación.
    /// </summary>
    public class RegisterProfessionalRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El documento no puede exceder 20 caracteres")]
        public string? DocumentNumber { get; set; }

        [StringLength(20, ErrorMessage = "El telefono no puede exceder 20 caracteres")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "La especialidad es requerida")]
        [StringLength(100, ErrorMessage = "La especialidad no puede exceder 100 caracteres")]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El numero de licencia no puede exceder 50 caracteres")]
        public string? LicenseNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es invalido")]
        public string Email { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una institución válida")]
        public int? InstitutionId { get; set; }
    }
}