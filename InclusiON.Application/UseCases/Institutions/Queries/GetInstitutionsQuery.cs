using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Institutions;

namespace InclusiON.Application.UseCases.Institutions.Queries
{
    public record GetInstitutionsQuery()
    {
        internal static InstitutionResponse MapToResponse(EducationalInstitution institution)
        {
            return new InstitutionResponse
            {
                Id = institution.Id,
                Name = institution.Name,
                Address = institution.Address,
                Phone = institution.Phone,
                Email = institution.Email,
                IsActive = institution.IsActive,
                CreatedAt = institution.CreatedAt,
                UpdatedAt = institution.UpdatedAt
            };
        }
    }
}
