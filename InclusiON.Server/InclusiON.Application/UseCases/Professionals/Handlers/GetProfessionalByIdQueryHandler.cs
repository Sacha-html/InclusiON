using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
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

            var response = ProfessionalResponse.MapToResponse(professional);
            return ApiResponse<ProfessionalResponse>.SuccessResult(response);
        }
    }
}
