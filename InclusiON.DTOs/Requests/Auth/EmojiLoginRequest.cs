using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login con secuencia de emojis.
    /// El usuario debe seleccionar 3 emojis en el orden correcto.
    /// </summary>
    public class EmojiLoginRequest
    {
        /// <summary>
        /// ID del usuario que intenta autenticarse.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Secuencia de emojis seleccionados (3 emojis).
        /// </summary>
        [Required(ErrorMessage = "La secuencia de emojis es requerida")]
        [MinLength(3, ErrorMessage = "La secuencia debe tener 3 emojis")]
        [MaxLength(3, ErrorMessage = "La secuencia debe tener 3 emojis")]
        public string[] EmojiSequence { get; set; } = Array.Empty<string>();

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
