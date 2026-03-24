using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Admin
{
    /// <summary>
    /// Request para crear un nuevo usuario administrador con asignacion de institucion.
    /// </summary>
    public class CreateAdminUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public int InstitutionId { get; set; }
    }
}
