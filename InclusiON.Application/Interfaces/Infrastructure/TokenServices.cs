using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Agrupa los servicios de tokens (generacion JWT + persistencia de refresh tokens).
    /// Reduce dependencias en servicios que necesitan ambos.
    /// </summary>
    public record TokenServices(
        IJwtTokenService JwtTokenService,
        IRefreshTokensRepository RefreshTokensRepository);
}
