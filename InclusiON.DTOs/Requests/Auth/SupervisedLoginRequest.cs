using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login supervisado.
    /// Un profesional o familiar autoriza el acceso del usuario.
    /// </summary>
    public class SupervisedLoginRequest
    {
        /// <summary>
        /// ID del usuario que sera autenticado (persona con discapacidad).
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public int UserId { get; set; }

        /// <summary>
        /// ID del supervisor que autoriza el acceso.
        /// </summary>
        [Required(ErrorMessage = "El ID del supervisor es requerido")]
        public int SupervisorId { get; set; }

        /// <summary>
        /// PIN del supervisor para validar la autorizacion.
        /// </summary>
        [Required(ErrorMessage = "El PIN del supervisor es requerido")]
        public string SupervisorPin { get; set; } = string.Empty;

        /// <summary>
        /// ID del dispositivo para registro.
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// Motivo de la sesion supervisada (opcional).
        /// </summary>
        public string? SessionReason { get; set; }
    }
}
