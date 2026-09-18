using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Invitations
{
    public class AcceptInvitationRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es invalido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es requerido")]
        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos numéricos")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmacion de contraseña es requerida")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
