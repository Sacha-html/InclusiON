using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class GetProfessionalStatusHistoryQueryHandler : IQueryHandler<GetProfessionalStatusHistoryQuery, ApiResponse<List<ProfessionalStatusHistoryResponse>>>
    {
        private readonly IProfessionalsRepository _repository;

        public GetProfessionalStatusHistoryQueryHandler(IProfessionalsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<ProfessionalStatusHistoryResponse>>> HandleAsync(
            GetProfessionalStatusHistoryQuery query,
            CancellationToken cancellationToken)
        {
            var history = await _repository.GetStatusHistoryAsync(query.ProfessionalId, cancellationToken);

            var response = history.Select(h => new ProfessionalStatusHistoryResponse
            {
                Id = h.Id,
                OldStatus = h.OldStatus?.ToString(),
                NewStatus = h.NewStatus.ToString(),
                Observation = h.Observation,
                ChangedByUserId = h.ChangedByUserId,
                CreatedAt = h.CreatedAt
            }).ToList();

            return ApiResponse<List<ProfessionalStatusHistoryResponse>>.SuccessResult(response);
        }
    }
}
