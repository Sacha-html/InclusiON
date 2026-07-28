using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Auth
{
    /// <summary>
    /// Request para identificar un usuario antes de autenticarse.
    /// Permite obtener el metodo de login configurado sin revelar datos sensibles.
    /// </summary>
    public class IdentifyUserRequest
    {
        /// <summary>
        /// Identificador del usuario (nombre, username o ID).
        /// </summary>
        [Required(ErrorMessage = "El identificador es requerido")]
        [MinLength(2, ErrorMessage = "El identificador debe tener al menos 2 caracteres")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// ID del dispositivo para verificar si es confiable.
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// Tipo de usuario que busca (Person, Professional, Family).
        /// </summary>
        public string? UserType { get; set; }
    }
}
