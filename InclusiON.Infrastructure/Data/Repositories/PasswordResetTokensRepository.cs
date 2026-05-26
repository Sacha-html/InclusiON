using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class PasswordResetTokensRepository : IPasswordResetTokensRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PasswordResetTokensRepository> _logger;

        public PasswordResetTokensRepository(AppDbContext context, ILogger<PasswordResetTokensRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
            _logger.LogDebug("Password reset token created for user {UserId}", token.UserId);
            return token.Id;
        }

        public async Task<PasswordResetToken?> GetValidByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    !t.IsUsed &&
                    t.ExpiresAt > now,
                    cancellationToken);
        }

        public async Task<bool> InvalidatePreviousTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var affected = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.IsUsed, true)
                    .SetProperty(t => t.UsedAt, DateTime.UtcNow),
                    cancellationToken);

            if (affected > 0)
                _logger.LogDebug("Invalidated {Count} previous reset tokens for user {UserId}", affected, userId);

            return affected > 0;
        }

        public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var deleted = await _context.PasswordResetTokens
                .Where(t => t.ExpiresAt <= DateTime.UtcNow)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
                _logger.LogInformation("Cleaned up {Count} expired password reset tokens", deleted);

            return deleted;
        }
    }
}
