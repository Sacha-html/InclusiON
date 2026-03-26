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

            var response = diagnoses.Select(d => new DiagnosisListItemResponse
            {
                Id = d.Id,
                DiagnosisDate = d.DiagnosisDate,
                PrimaryDiagnosis = d.PrimaryDiagnosis,
                ProfessionalName = $"{d.Professional.FirstName} {d.Professional.LastName}".Trim(),
                ProfessionalId = d.ProfessionalId,
                CreatedAt = d.CreatedAt
            }).ToList();

            return ApiResponse<List<DiagnosisListItemResponse>>.SuccessResult(response);
        }
    }
}
