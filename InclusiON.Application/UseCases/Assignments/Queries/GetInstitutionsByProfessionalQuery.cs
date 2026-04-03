using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Queries
{
    public record GetInstitutionsByProfessionalQuery(Guid ProfessionalId)
    {
        internal static ProfessionalInstitutionResponse MapToResponse(ProfessionalInstitution assignment)
        {
            return new ProfessionalInstitutionResponse
            {
                ProfessionalId = assignment.ProfessionalId,
                InstitutionId = assignment.InstitutionId,
                InstitutionName = assignment.Institution?.Name ?? string.Empty,
                AssignedAt = assignment.AssignedAt,
                IsActive = assignment.IsActive
            };
        }
    }
}
