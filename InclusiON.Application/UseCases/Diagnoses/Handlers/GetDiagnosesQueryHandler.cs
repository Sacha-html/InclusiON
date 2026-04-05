using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Diagnoses;

namespace InclusiON.Application.UseCases.Diagnoses.Handlers
{
    public class GetDiagnosesQueryHandler : IQueryHandler<GetDiagnosesQuery, ApiResponse<List<DiagnosisListItemResponse>>>
    {
        private readonly IDiagnosesRepository _repository;

        public GetDiagnosesQueryHandler(IDiagnosesRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<DiagnosisListItemResponse>>> HandleAsync(
            GetDiagnosesQuery query, CancellationToken cancellationToken)
        {
            var diagnoses = await _repository.GetByPersonIdAsync(query.PersonId, cancellationToken);

            var response = diagnoses.Select(DiagnosisListItemResponse.MapToResponse).ToList();

            return ApiResponse<List<DiagnosisListItemResponse>>.SuccessResult(response);
        }
    }
}
