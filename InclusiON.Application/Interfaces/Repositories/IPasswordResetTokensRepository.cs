using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IPasswordResetTokensRepository
    {
        Task<Guid> CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);

        /// <summary>Busca por hash SHA-256 del token plano. Retorna null si no existe, expiró o ya fue usado.</summary>
        Task<PasswordResetToken?> GetValidByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task<bool> InvalidatePreviousTokensAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}
