using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Invitations
{
    public class AcceptInvitationRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es invalido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es requerida")]
        [MinLength(8, ErrorMessage = "La contrasena debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmacion de contrasena es requerida")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
