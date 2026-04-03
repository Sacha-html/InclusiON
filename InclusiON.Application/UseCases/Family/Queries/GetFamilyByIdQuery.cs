using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Queries
{
    public record GetFamilyByIdQuery(Guid FamilyId)
    {
        internal static FamilyResponse MapToResponse(FamilyRepresentative f)
        {
            return new FamilyResponse
            {
                Id = f.Id,
                UserId = f.UserId,
                FirstName = f.FirstName,
                LastName = f.LastName,
                DocumentNumber = f.DocumentNumber,
                Phone = f.Phone,
                Relationship = f.Relationship,
                IsActive = f.User?.IsActive ?? false,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                Email = f.User?.Email,
                LinkedPersons = f.PersonRepresentatives?
                    .Where(pr => pr.IsActive)
                    .Select(pr => new LinkedPersonInfo
                    {
                        PersonId = pr.PersonId,
                        FullName = $"{pr.Person.FirstName} {pr.Person.LastName}".Trim(),
                        DisabilityType = pr.Person.DisabilityType?.Name,
                        IsPrimary = pr.IsPrimary
                    })
                    .ToList()
            };
        }
    }
}
