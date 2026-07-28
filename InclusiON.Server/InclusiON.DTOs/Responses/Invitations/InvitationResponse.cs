using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Invitations
{
    public class InvitationResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Relationship { get; set; }
        public string? PersonName { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CreatedByProfessionalName { get; set; }
        public DateTime CreatedAt { get; set; }

        public static InvitationResponse MapToResponse(Invitation invitation)
        {
            var now = DateTime.UtcNow;
            string status;

            if (invitation.IsUsed)
                status = "Aceptada";
            else if (invitation.ExpiresAt < now)
                status = "Expirada";
            else
                status = "Enviada";

            return new InvitationResponse
            {
                Id = invitation.Id,
                Code = invitation.Code,
                Email = invitation.Email,
                FirstName = invitation.FirstName,
                LastName = invitation.LastName,
                Relationship = invitation.Relationship,
                PersonName = invitation.ForPerson != null
                    ? $"{invitation.ForPerson.FirstName} {invitation.ForPerson.LastName}".Trim()
                    : null,
                ExpiresAt = invitation.ExpiresAt,
                IsUsed = invitation.IsUsed,
                UsedAt = invitation.UsedAt,
                Status = status,
                CreatedByProfessionalName = invitation.CreatedByProfessional != null
                    ? $"{invitation.CreatedByProfessional.FirstName} {invitation.CreatedByProfessional.LastName}".Trim()
                    : null,
                CreatedAt = invitation.CreatedAt
            };
        }
    }
}
