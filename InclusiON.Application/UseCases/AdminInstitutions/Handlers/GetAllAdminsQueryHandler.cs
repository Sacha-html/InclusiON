using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.AdminInstitutions.Handlers
{
    public class GetAllAdminsQueryHandler : IQueryHandler<GetAllAdminsQuery, ApiResponse<List<AdminUserResponse>>>
    {
        private readonly IAdminInstitutionRepository _repository;

        public GetAllAdminsQueryHandler(IAdminInstitutionRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<AdminUserResponse>>> HandleAsync(
            GetAllAdminsQuery query, CancellationToken cancellationToken)
        {
            var admins = await _repository.GetAllAdminsWithInstitutionsAsync(cancellationToken);

            var response = admins.Select(u => new AdminUserResponse
            {
                Id          = u.Id,
                Name        = u.Name ?? string.Empty,
                Surname     = u.Surname ?? string.Empty,
                Email       = u.Email!,
                IsActive    = u.IsActive,
                CreatedAt   = u.CreatedAt,
                IsGlobalAdmin = !u.AdminInstitutions.Any(),
                Institutions = u.AdminInstitutions.Select(ai => new AdminInstitutionInfo
                {
                    InstitutionId   = ai.InstitutionId,
                    InstitutionName = ai.Institution.Name
                }).ToList()
            }).ToList();

            return ApiResponse<List<AdminUserResponse>>.SuccessResult(response);
        }
    }
}
