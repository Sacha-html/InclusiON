using InclusiON.DTOs.Responses.Auth;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.Interfaces.Repositories
{

    public interface IRefreshTokensRepository
    {
        Task<Guid> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> RevokeAsync(string token, string? reason = null, CancellationToken cancellationToken = default);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> RevokeAllUserTokensAsync(Guid userId, string? reason = null, CancellationToken cancellationToken = default);
        Task<RefreshTokenStatsResponse> GetUserTokenStatsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
        Task<(List<RefreshToken> Tokens, int TotalCount)> GetTokensPaginatedAsync(
            int page = 1,
            int pageSize = 50,
            Guid? userId = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default);
        Task<int> GetActiveTokensCountAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
