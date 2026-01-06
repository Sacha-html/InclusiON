using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para login por seleccion de perfil visual.
    /// El usuario selecciona su avatar de una lista de perfiles.
    /// </summary>
    public class ProfileSelectLoginRequest
    {
        /// <summary>
        /// ID del usuario seleccionado.
        /// </summary>
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public Guid UserId { get; set; }

        /// <summary>
        /// ID del dispositivo para validacion de contexto.
        /// </summary>
        [Required(ErrorMessage = "El ID del dispositivo es requerido")]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Indica si se requiere confirmacion adicional (PIN o emoji).
        /// </summary>
        public bool RequiresConfirmation { get; set; }

        /// <summary>
        /// PIN de confirmacion si se requiere.
        /// </summary>
        public string? ConfirmationPin { get; set; }
    }
}
