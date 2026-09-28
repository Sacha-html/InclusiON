using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Queries;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Diagnoses.Handlers
{
    public class ExportDiagnosisPdfQueryHandler : IQueryHandler<ExportDiagnosisPdfQuery, ApiResponse<byte[]>>
    {
        private readonly IDiagnosesRepository _repository;
        private readonly IDiagnosisPdfService _pdfService;

        public ExportDiagnosisPdfQueryHandler(IDiagnosesRepository repository, IDiagnosisPdfService pdfService)
        {
            _repository = repository;
            _pdfService = pdfService;
        }

        public async Task<ApiResponse<byte[]>> HandleAsync(ExportDiagnosisPdfQuery query, CancellationToken cancellationToken)
        {
            var diagnosis = await _repository.GetByIdAsync(query.DiagnosisId, cancellationToken);

            if (diagnosis is null)
                return ApiResponse<byte[]>.NotFound("Diagnóstico");

            var bytes = _pdfService.Generate(diagnosis);
            return ApiResponse<byte[]>.SuccessResult(bytes);
        }
    }
}
