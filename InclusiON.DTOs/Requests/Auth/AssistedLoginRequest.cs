using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login asistido.
    /// Un profesional o familiar autoriza el acceso de la persona con discapacidad
    /// usando sus credenciales de email y contrasena.
    /// </summary>
    public class AssistedLoginRequest
    {
        /// <summary>
        /// ID del usuario que sera autenticado (persona con discapacidad).
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Email del supervisor que autoriza el acceso.
        /// </summary>
        [Required(ErrorMessage = "El email del supervisor es requerido")]
        [EmailAddress(ErrorMessage = "Email invalido")]
        public string SupervisorEmail { get; set; } = string.Empty;

        /// <summary>
        /// Contrasena del supervisor para validar la autorizacion.
        /// </summary>
        [Required(ErrorMessage = "La contrasena del supervisor es requerida")]
        public string SupervisorPassword { get; set; } = string.Empty;

        /// <summary>
        /// ID del dispositivo para registro (opcional).
        /// </summary>
        public string? DeviceId { get; set; }
    }
}
