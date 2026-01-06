using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login automatico desde dispositivo confiable.
    /// </summary>
    public class TrustedDeviceLoginRequest
    {
        /// <summary>
        /// ID del usuario que intenta autenticarse.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public Guid UserId { get; set; }

        /// <summary>
        /// ID unico del dispositivo registrado como confiable.
        /// </summary>
        [Required(ErrorMessage = "El ID del dispositivo es requerido")]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Token de dispositivo para validacion adicional.
        /// </summary>
        public string? DeviceToken { get; set; }
    }
}
