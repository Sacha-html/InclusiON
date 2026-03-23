using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Invitations
{
    public class CreateInvitationRequest
    {
        public Guid? PersonId { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es invalido")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string? LastName { get; set; }

        [StringLength(50, ErrorMessage = "El parentesco no puede exceder 50 caracteres")]
        public string? Relationship { get; set; }
    }
}
