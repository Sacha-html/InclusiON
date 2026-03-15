using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class GetProfessionalByIdQueryHandler : IQueryHandler<GetProfessionalByIdQuery, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;

        public GetProfessionalByIdQueryHandler(IProfessionalsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(GetProfessionalByIdQuery query, CancellationToken cancellationToken)
        {
            var professional = await _repository.GetByIdAsync(query.ProfessionalId, cancellationToken);

            if (professional == null)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            var response = MapToResponse(professional);
            return ApiResponse<ProfessionalResponse>.SuccessResult(response);
        }

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
                Address = professional.Address,
                IsActive = professional.User?.IsActive ?? false,
                CreatedAt = professional.CreatedAt,
                UpdatedAt = professional.UpdatedAt,
                Email = professional.User?.Email
            };
        }
    }
}
