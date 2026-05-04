using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses.Admin;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class GetUserActivityQueryHandler
        : IQueryHandler<GetUserActivityQuery, ApiResponse<List<UserRecentSessionResponse>>>
    {
        private readonly IRefreshTokensRepository _tokensRepository;

        public GetUserActivityQueryHandler(IRefreshTokensRepository tokensRepository)
        {
            _tokensRepository = tokensRepository;
        }

        public async Task<ApiResponse<List<UserRecentSessionResponse>>> HandleAsync(
            GetUserActivityQuery query, CancellationToken cancellationToken)
        {
            var limit = Math.Clamp(query.Limit, 1, 50);
            var tokens = await _tokensRepository.GetRecentByUserIdAsync(query.UserId, limit, cancellationToken);

            var sessions = tokens.Select(t => new UserRecentSessionResponse
            {
                CreatedAt     = t.CreatedAt,
                IpAddress     = t.CreatedByIp,
                UserAgent     = t.UserAgent,
                IsActive      = t.IsActive,
                ExpiresAt     = t.ExpiresAt,
                RevokedAt     = t.RevokedAt,
                RevokedReason = t.RevokedReason,
            }).ToList();

            return ApiResponse<List<UserRecentSessionResponse>>.SuccessResult(sessions);
        }
    }
}
