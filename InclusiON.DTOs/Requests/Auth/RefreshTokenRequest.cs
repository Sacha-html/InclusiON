using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para refrescar el token de acceso.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// El refresh token obtenido durante el login.
        /// </summary>
        [Required(ErrorMessage = "El refresh token es requerido")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
