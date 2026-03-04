using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class RefreshTokensRepository : IRefreshTokensRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RefreshTokensRepository> _logger;

        public RefreshTokensRepository(AppDbContext context, ILogger<RefreshTokensRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

                _logger.LogDebug("Refresh token created for user: {UserId}", refreshToken.UserId);
                return refreshToken.Id;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Create refresh token operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refresh token");
                throw;
            }
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetActiveTokensQuery()
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Get refresh token operation was cancelled");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refresh token");
                return null;
            }
        }

        public async Task<bool> RevokeAsync(string token, string? reason = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var affectedRows = await GetBaseQuery()
                    .Where(rt => rt.Token == token && rt.IsActive)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(rt => rt.IsActive, false)
                        .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow)
                        .SetProperty(rt => rt.RevokedReason, reason ?? "Token revoked"),
                    cancellationToken);

                if (affectedRows > 0)
                {
                    _logger.LogDebug("Refresh token revoked");
                    return true;
                }

                _logger.LogWarning("Refresh token not found or already revoked");
                return false;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Revoke refresh token operation was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return false;
            }
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetUserTokensQuery(userId)
                    .Where(rt => rt.IsActive && rt.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Get active tokens operation was cancelled");
                return new List<RefreshToken>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active tokens for user {UserId}", userId);
                return new List<RefreshToken>();
            }
        }

        public async Task<int> RevokeAllUserTokensAsync(Guid userId, string? reason = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var revokedCount = await GetUserTokensQuery(userId)
                    .Where(rt => rt.IsActive)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(rt => rt.IsActive, false)
                        .SetProperty(rt => rt.RevokedAt, DateTime.UtcNow)
                        .SetProperty(rt => rt.RevokedReason, reason ?? "All tokens revoked"),
                    cancellationToken);

                if (revokedCount > 0)
                {
                    _logger.LogDebug("Revoked {RevokedCount} refresh tokens for user: {UserId}", revokedCount, userId);
                }
                else
                {
                    _logger.LogDebug("No active tokens found for user: {UserId}", userId);
                }

                return revokedCount;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Revoke all tokens operation was cancelled");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<RefreshTokenStatsResponse> GetUserTokenStatsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var now = DateTime.UtcNow;

                var stats = await GetUserTokensQuery(userId)
                    .GroupBy(rt => rt.UserId)
                    .Select(g => new RefreshTokenStatsResponse
                    {
                        UserId = userId,
                        TotalTokens = g.Count(),
                        ActiveTokens = g.Count(t => t.IsActive && t.ExpiresAt > now),
                        ExpiredTokens = g.Count(t => t.IsActive && t.ExpiresAt <= now),
                        RevokedTokens = g.Count(t => !t.IsActive),
                        LastTokenCreated = g.Max(t => t.CreatedAt),
                        OldestActiveToken = g
                            .Where(t => t.IsActive && t.ExpiresAt > now)
                            .Min(t => (DateTime?)t.CreatedAt)
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                return stats ?? new RefreshTokenStatsResponse { UserId = userId };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Get token stats operation was cancelled");
                return new RefreshTokenStatsResponse { UserId = userId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token stats for user {UserId}", userId);
                return new RefreshTokenStatsResponse { UserId = userId };
            }
        }

        public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var deletedCount = await GetBaseQuery()
                    .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
                    .ExecuteDeleteAsync(cancellationToken);

                if (deletedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {DeletedCount} expired refresh tokens", deletedCount);
                }
                else
                {
                    _logger.LogDebug("No expired tokens found for cleanup");
                }

                return deletedCount;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Cleanup expired tokens operation was cancelled");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
                return 0;
            }
        }

        public async Task<int> GetActiveTokensCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetUserTokensQuery(userId)
                    .Where(rt => rt.IsActive && rt.ExpiresAt > DateTime.UtcNow)
                    .CountAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Count active tokens operation was cancelled");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting active tokens for user {UserId}", userId);
                return 0;
            }
        }

        #region private
        private IQueryable<RefreshToken> GetBaseQuery()
        {
            return _context.RefreshTokens.AsQueryable();
        }

        private IQueryable<RefreshToken> GetActiveTokensQuery()
        {
            var now = DateTime.UtcNow;
            return GetBaseQuery()
                .Where(rt => rt.IsActive && rt.ExpiresAt > now);
        }

        private IQueryable<RefreshToken> GetUserTokensQuery(Guid userId)
        {
            return GetBaseQuery()
                .Where(rt => rt.UserId == userId);
        }
        #endregion
    }
}