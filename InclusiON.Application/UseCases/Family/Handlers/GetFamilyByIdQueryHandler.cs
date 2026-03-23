using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetFamilyByIdQueryHandler : IQueryHandler<GetFamilyByIdQuery, ApiResponse<FamilyResponse>>
    {
        private readonly IFamilyRepository _repository;

        public GetFamilyByIdQueryHandler(IFamilyRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(GetFamilyByIdQuery query, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdAsync(query.FamilyId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<FamilyResponse>.NotFound("Familiar");
            }

            var response = MapToResponse(family);
            return ApiResponse<FamilyResponse>.SuccessResult(response);
        }

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
                Email = f.User?.Email
            };
        }
    }
}
