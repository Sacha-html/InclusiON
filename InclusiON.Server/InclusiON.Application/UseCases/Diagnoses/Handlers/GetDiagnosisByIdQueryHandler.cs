using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
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
        private readonly IEncryptionService   _encryption;

        public GetDiagnosisByIdQueryHandler(IDiagnosesRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<DiagnosisResponse>> HandleAsync(
            GetDiagnosisByIdQuery query, CancellationToken cancellationToken)
        {
            var diagnosis = await _repository.GetByIdAsync(query.DiagnosisId, cancellationToken);

            if (diagnosis is null)
                return ApiResponse<DiagnosisResponse>.NotFound("Diagnóstico");

            var dto = DiagnosisResponse.MapToResponse(diagnosis);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(diagnosis.Id.ToString()));
            return ApiResponse<DiagnosisResponse>.SuccessResult(dto);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
