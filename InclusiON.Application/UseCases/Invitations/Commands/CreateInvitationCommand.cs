using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Invitations;

namespace InclusiON.Application.UseCases.Invitations.Commands
{
    public record CreateInvitationCommand(
        Guid ProfessionalId,
        Guid? PersonId,
        string Email,
        string? FirstName,
        string? LastName,
        string? Relationship,
        string? BaseUrl = null
    )
    {
        internal static InvitationResponse MapToResponse(Invitation invitation)
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
