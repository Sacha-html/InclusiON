using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Institutions;

namespace InclusiON.Application.UseCases.Institutions.Handlers
{
    public class GetInstitutionsQueryHandler
        : IQueryHandler<GetInstitutionsQuery, ApiResponse<List<InstitutionResponse>>>
    {
        private readonly IInstitutionsRepository _repository;

        public GetInstitutionsQueryHandler(IInstitutionsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<InstitutionResponse>>> HandleAsync(
            GetInstitutionsQuery query, CancellationToken cancellationToken)
        {
            var institutions = await _repository.GetAllAsync(cancellationToken);

            var response = institutions.Select(InstitutionResponse.MapToResponse).ToList();
            return ApiResponse<List<InstitutionResponse>>.SuccessResult(response);
        }
    }
}
