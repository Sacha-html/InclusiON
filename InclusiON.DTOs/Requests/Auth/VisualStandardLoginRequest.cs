using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login visual estandar.
    /// La persona con discapacidad se identifica por nombre y luego ingresa su contrasena.
    /// </summary>
    public class VisualStandardLoginRequest
    {
        /// <summary>
        /// ID del usuario que sera autenticado.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Contrasena del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contrasena es requerida")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// ID del dispositivo para registro (opcional).
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// Recordar dispositivo como confiable (opcional).
        /// </summary>
        public bool RememberDevice { get; set; }
    }
}
