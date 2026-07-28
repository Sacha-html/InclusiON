using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login con PIN numerico.
    /// </summary>
    public class PinLoginRequest
    {
        /// <summary>
        /// ID del usuario que intenta autenticarse.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public Guid UserId { get; set; }

        /// <summary>
        /// PIN numerico de 4-6 digitos.
        /// </summary>
        [Required(ErrorMessage = "El PIN es requerido")]
        [RegularExpression(@"^\d{4,6}$", ErrorMessage = "El PIN debe tener entre 4 y 6 digitos")]
        public string Pin { get; set; } = string.Empty;

        /// <summary>
        /// ID del dispositivo para registro de confianza.
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// Indica si se debe recordar este dispositivo como confiable.
        /// </summary>
        public bool RememberDevice { get; set; }
    }
}
