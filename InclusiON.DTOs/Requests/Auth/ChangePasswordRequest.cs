using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "La contrasena actual es requerida")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contrasena es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contrasena debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
            ErrorMessage = "La contrasena debe contener al menos: 1 mayuscula, 1 minuscula, 1 numero y 1 caracter especial (@$!%*?&)")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmacion de contrasena es requerida")]
        [Compare("NewPassword", ErrorMessage = "Las contrasenas no coinciden")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
