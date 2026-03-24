using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;

        /// <summary>
        /// Roles permitidos para este flujo de login.
        /// Si se envian, el backend valida que el usuario tenga uno de estos roles.
        /// </summary>
        public List<string>? AllowedRoles { get; set; }
    }
}
