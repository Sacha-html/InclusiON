using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Auth;
using InclusiON.Domain.Enums;
using InclusiON.Infrastructure.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InclusiON.Application.Constants;

namespace InclusiON.Infrastructure.Authentication
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _tokenHandler = new JwtSecurityTokenHandler();

            ValidateSettings();
        }

        public string GenerateAccessToken(TokenUserData userData)
        {
            try
            {
                if (userData is null)
                {
                    throw new ArgumentNullException(nameof(userData));
                }

                var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
                var utcNow = DateTime.UtcNow;

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userData.Id.ToString()),
                new Claim(ClaimTypes.Name, userData.Name ?? string.Empty),
                new Claim(ClaimTypes.Email, userData.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, userData.Role ?? IdentityRoles.PersonWithDisability.ToString()),
                new Claim(Permissions.IsActiveClaimType, userData.IsActive.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                if (userData.Permissions is not null && userData.Permissions.Any())
                {
                    foreach (var permission in userData.Permissions)
                    {
                        claims.Add(new Claim("permission", permission));
                    }
                }

                if (userData.Role == "Admin")
                {
                    claims.Add(new Claim(Permissions.GlobalAdminClaimType, userData.IsGlobalAdmin.ToString().ToLower()));

                    if (!userData.IsGlobalAdmin && userData.InstitutionIds is not null)
                    {
                        foreach (var instId in userData.InstitutionIds)
                        {
                            claims.Add(new Claim(Permissions.InstitutionIdClaimType, instId.ToString()));
                        }
                    }
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = utcNow.AddHours(_jwtSettings.ExpirationHours),
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    IssuedAt = utcNow,
                    NotBefore = utcNow,
                    SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature
                        )
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                return _tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating access token {ex.Message}", ex);
            }

        }

        public string GenerateRefreshToken()
        {
            try
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating refresh token: {ex.Message}", ex);
            }
        }

        public DateTime GetTokenExpiration(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return DateTime.MinValue;
            }

            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public Guid? GetUserGuidFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            try
            {
                var principal = ValidateToken(token);
                var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdClaim, out Guid userId))
                    return userId;

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsTokenValid(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                var main = ValidateToken(token);
                return main != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null!;
            }

            try
            {
                var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero
                };

                var main = _tokenHandler
                    .ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                     !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null!;
                }

                return main;
            }
            catch (SecurityTokenExpiredException)
            {
                return null!;
            }
            catch (SecurityTokenException)
            {
                return null!;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
            {
                throw new ArgumentException("JWT Secret cannot be null or empty");
            }

            if (_jwtSettings.Secret.Length < 32)
            {
                throw new ArgumentException("JWT Secret must be at least 32 characters long");
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
            {
                throw new ArgumentException("JWT Issuer cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            {
                throw new ArgumentException("JWT Audience cannot be null or empty");
            }
        }
    }
}
