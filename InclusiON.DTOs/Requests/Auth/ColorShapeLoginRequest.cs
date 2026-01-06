using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login con seleccion de color y forma.
    /// </summary>
    public class ColorShapeLoginRequest
    {
        /// <summary>
        /// ID del usuario que intenta autenticarse.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public Guid UserId { get; set; }

        /// <summary>
        /// ID de la combinacion color-forma seleccionada (1-24).
        /// </summary>
        [Required(ErrorMessage = "La seleccion es requerida")]
        [Range(1, 24, ErrorMessage = "La seleccion debe ser un valor entre 1 y 24")]
        public int ColorShapeId { get; set; }

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
