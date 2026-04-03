using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Application.UseCases.Professionals.Queries
{
    public record GetProfessionalByIdQuery(Guid ProfessionalId)
    {
        internal static ProfessionalResponse MapToResponse(Professional professional)
        {
            var statusName = professional.Status switch
            {
                ProfessionalStatusEnum.Pending => "Pendiente",
                ProfessionalStatusEnum.Approved => "Aprobado",
                ProfessionalStatusEnum.Rejected => "Rechazado",
                ProfessionalStatusEnum.Suspended => "Suspendido",
                ProfessionalStatusEnum.Terminated => "Dado de baja",
                _ => "Desconocido"
            };

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
                Status = (int)professional.Status,
                StatusName = statusName,
                ValidatedAt = professional.ValidatedAt,
                ValidatedByUserId = professional.ValidatedByUserId,
                IsActive = professional.User?.IsActive ?? false,
                CreatedAt = professional.CreatedAt,
                UpdatedAt = professional.UpdatedAt,
                Email = professional.User?.Email
            };
        }
    }
}
