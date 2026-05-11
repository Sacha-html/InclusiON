using InclusiON.Application.Interfaces.Common;
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
        private readonly IRefreshTokensRepository _tokensRepository;

        public GetUserActivityQueryHandler(IRefreshTokensRepository tokensRepository)
        {
            _tokensRepository = tokensRepository;
        }

        public async Task<ApiResponse<PagedResponse<UserRecentSessionResponse>>> HandleAsync(
            GetUserActivityQuery query, CancellationToken cancellationToken)
        {
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
