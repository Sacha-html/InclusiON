using InclusiON.DTOs.Auth;
using System.Security.Claims;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(TokenUserData userData);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        Guid? GetUserGuidFromToken(string token);
        bool IsTokenValid(string token);
        DateTime GetTokenExpiration(string token);
    }
}
