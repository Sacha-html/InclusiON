using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses.Admin;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class GetUserActivityQueryHandler
        : IQueryHandler<GetUserActivityQuery, ApiResponse<PagedResponse<UserRecentSessionResponse>>>
    {
        private readonly IRefreshTokensRepository    _tokensRepository;
        private readonly IIdentityService            _identityService;
        private readonly IAdminInstitutionRepository _adminInstitutionRepository;

        public GetUserActivityQueryHandler(
            IRefreshTokensRepository    tokensRepository,
            IIdentityService            identityService,
            IAdminInstitutionRepository adminInstitutionRepository)
        {
            _tokensRepository            = tokensRepository;
            _identityService             = identityService;
            _adminInstitutionRepository  = adminInstitutionRepository;
        }

        public async Task<ApiResponse<PagedResponse<UserRecentSessionResponse>>> HandleAsync(
            GetUserActivityQuery query, CancellationToken cancellationToken)
        {
            // Validación de alcance institucional
            if (query.InstitutionIds is { Count: > 0 })
            {
                var user = await _identityService.FindByIdAsync(query.UserId);
                if (user is null)
                    return ApiResponse<PagedResponse<UserRecentSessionResponse>>.NotFound("Usuario");

                var targetInstitutions = await _adminInstitutionRepository
                    .GetActiveInstitutionIdsByAdminAsync(query.UserId, cancellationToken);

                if (targetInstitutions.Count > 0)
                {
                    var hasOverlap = targetInstitutions.Any(id => query.InstitutionIds!.Contains(id));
                    if (!hasOverlap)
                        return ApiResponse<PagedResponse<UserRecentSessionResponse>>.Forbidden(
                            "No tiene permisos para ver la actividad de un usuario de otra institución.");
                }
            }

            var pageSize = Math.Clamp(query.PageSize, 1, 50);
            var limit    = query.Page * pageSize;
            var tokens   = await _tokensRepository.GetRecentByUserIdAsync(query.UserId, limit, cancellationToken);

            var all = tokens.Select(AdminUserMapper.ToSessionResponse).ToList();

            var totalRecords = all.Count;
            var totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var data         = all.Skip((query.Page - 1) * pageSize).Take(pageSize).ToList();

            var response = new PagedResponse<UserRecentSessionResponse>
            {
                Data            = data,
                TotalRecords    = totalRecords,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = pageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<UserRecentSessionResponse>>.SuccessResult(response);
        }
    }
}
