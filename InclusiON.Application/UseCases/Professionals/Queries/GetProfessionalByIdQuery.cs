using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Application.UseCases.Professionals.Queries
{
    public record GetProfessionalByIdQuery(Guid ProfessionalId)
    {
        internal static ProfessionalResponse MapToResponse(Professional professional)
        {
            return new ProfessionalResponse
            {
                Id = professional.Id,
                UserId = professional.UserId,
                FirstName = professional.FirstName,
                LastName = professional.LastName,
                DocumentNumber = professional.DocumentNumber,
                Phone = professional.Phone,
                Specialty = professional.Specialty,
                LicenseNumber = professional.LicenseNumber,
                BirthDate = professional.BirthDate,
                IsActive = professional.User?.IsActive ?? false,
                CreatedAt = professional.CreatedAt,
                UpdatedAt = professional.UpdatedAt,
                Email = professional.User?.Email
            };
        }
    }
}
