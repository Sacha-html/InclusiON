using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Diagnoses;

namespace InclusiON.Application.UseCases.Diagnoses.Handlers
{
    public class GetDiagnosisByIdQueryHandler : IQueryHandler<GetDiagnosisByIdQuery, ApiResponse<DiagnosisResponse>>
    {
        private readonly IDiagnosesRepository _repository;

        public GetDiagnosisByIdQueryHandler(IDiagnosesRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<DiagnosisResponse>> HandleAsync(
            GetDiagnosisByIdQuery query, CancellationToken cancellationToken)
        {
            var diagnosis = await _repository.GetByIdAsync(query.DiagnosisId, cancellationToken);

            if (diagnosis is null)
                return ApiResponse<DiagnosisResponse>.NotFound("Diagnóstico");

            return ApiResponse<DiagnosisResponse>.SuccessResult(DiagnosisResponse.MapToResponse(diagnosis));
        }
    }
}
