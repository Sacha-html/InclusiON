using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.DTOs.Auth;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    /// <summary>
    /// Handler para login por seleccion de perfil visual.
    /// Solo funciona desde dispositivos registrados como confiables.
    /// </summary>
    [Obsolete("Este metodo de login ha sido deprecado. Use PinLoginCommandHandler o AssistedLoginCommandHandler en su lugar.")]
    public class ProfileSelectLoginCommandHandler : ICommandHandler<ProfileSelectLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProfileSelectLoginCommandHandler> _logger;

        public ProfileSelectLoginCommandHandler(
            IVisualLoginRepository repository,
            UserManager<User> userManager,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProfileSelectLoginCommandHandler> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            ProfileSelectLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Verificar que el dispositivo es confiable
                var isTrusted = await _repository.IsTrustedDeviceAsync(command.UserId, command.DeviceId, cancellationToken);

                if (!isTrusted)
                {
                    return ApiResponse<VisualLoginResponse>.SuccessResult(
                        new VisualLoginResponse
                        {
                            Success = false,
                            ErrorMessage = "Dispositivo no autorizado para seleccion de perfil"
                        });
                }

                var person = await _repository.GetPersonByUserIdAsync(command.UserId, cancellationToken);

                if (person == null)
                {
                    return ApiResponse<VisualLoginResponse>.ErrorResult("Usuario no encontrado");
                }

                var user = person.User;

                // Verificar si requiere confirmacion con PIN
                if (command.RequiresConfirmation)
                {
                    if (string.IsNullOrEmpty(command.ConfirmationPin) || string.IsNullOrEmpty(person.PinCodeHash))
                    {
                        return ApiResponse<VisualLoginResponse>.SuccessResult(
                            new VisualLoginResponse
                            {
                                Success = false,
                                ErrorMessage = "PIN de confirmacion requerido"
                            });
                    }

                    var pinValid = _passwordHasher.VerifyPassword(person.PinCodeHash, command.ConfirmationPin);
                    if (!pinValid)
                    {
                        return ApiResponse<VisualLoginResponse>.SuccessResult(
                            new VisualLoginResponse
                            {
                                Success = false,
                                ErrorMessage = "PIN de confirmacion incorrecto"
                            });
                    }
                }

                return await GenerateLoginResponseAsync(user, person, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login por seleccion de perfil: {UserId}", command.UserId);
                return ApiResponse<VisualLoginResponse>.ErrorResult($"Error al procesar login: {ex.Message}");
            }
        }

        private async Task<ApiResponse<VisualLoginResponse>> GenerateLoginResponseAsync(
            User user,
            PersonWithDisability person,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _refreshTokensRepository.RevokeAllUserTokensAsync(user.Id, "Login por seleccion de perfil");

            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIpAddress = ipAddress;
            user.LastLoginUserAgent = userAgent;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email!,
                Name = $"{person.FirstName} {person.LastName}",
                Role = roles.FirstOrDefault() ?? "Person",
                IsActive = user.IsActive
            };

            var accessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

            var displayName = $"{person.FirstName} {person.LastName}".Trim();

            return ApiResponse<VisualLoginResponse>.SuccessResult(
                new VisualLoginResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = _jwtTokenService.GetTokenExpiration(accessToken),
                    User = new VisualLoginUserInfo
                    {
                        Id = user.Id,
                        DisplayName = displayName,
                        Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                        AvatarColor = person.AvatarColor ?? "#2196F3",
                        UserType = "Person",
                        Roles = roles.ToList(),
                        Accessibility = new AccessibilityPreferences
                        {
                            RequiresLargeFont = person.RequiresLargeFont,
                            RequiresHighContrast = person.RequiresHighContrast,
                            VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                            SoundSensitivity = person.SoundSensitivity
                        }
                    }
                },
                "Login exitoso por seleccion de perfil");
        }

        private static string? GetClientIpAddress(HttpContext? context)
        {
            if (context is null) return null;
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
                return forwardedFor.Split(',').First().Trim();
            var clientIp = context.Request.Headers["X-Client-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientIp))
                return clientIp;
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
