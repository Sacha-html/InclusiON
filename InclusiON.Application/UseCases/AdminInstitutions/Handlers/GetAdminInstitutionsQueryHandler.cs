using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.AdminInstitutions.Handlers
{
    public class GetAdminInstitutionsQueryHandler : IQueryHandler<GetAdminInstitutionsQuery, ApiResponse<List<AdminInstitutionResponse>>>
    {
        private readonly IAdminInstitutionRepository _repository;

        public GetAdminInstitutionsQueryHandler(IAdminInstitutionRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<AdminInstitutionResponse>>> HandleAsync(
            GetAdminInstitutionsQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _repository.GetInstitutionsByAdminAsync(query.AdminUserId, cancellationToken);

            var response = assignments.Select(ai => new AdminInstitutionResponse
            {
                AdminUserId     = ai.AdminUserId,
                InstitutionId   = ai.InstitutionId,
                InstitutionName = ai.Institution.Name,
                AssignedAt      = ai.AssignedAt,
                IsActive        = ai.IsActive
            }).ToList();

            return ApiResponse<List<AdminInstitutionResponse>>.SuccessResult(response);
        }
    }
}
