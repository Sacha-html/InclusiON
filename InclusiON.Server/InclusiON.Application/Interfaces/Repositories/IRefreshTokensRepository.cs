using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
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
        Task<int> GetActiveTokensCountAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retorna los N tokens de refresco más recientes del usuario, ordenados por fecha de creación desc.
        /// </summary>
        Task<List<RefreshToken>> GetRecentByUserIdAsync(Guid userId, int limit, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoca todos los refresh tokens activos de una lista de usuarios.
        /// Útil para invalidar sesiones tras un cambio de permisos de rol.
        /// </summary>
        Task<int> RevokeAllUsersTokensAsync(
            IEnumerable<Guid> userIds,
            string? reason = null,
            CancellationToken cancellationToken = default);
    }
}
