using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.DTOs.Auth;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, ApiResponse<LoginResponse>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signinManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            UserManager<User> userManager,
            SignInManager<User> signinManager,
            IJwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LoginCommandHandler> logger)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _jwtTokenService = jwtTokenService;
            _refreshTokensRepository = refreshTokensRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var user = await _userManager.FindByEmailAsync(command.Email.ToLower().Trim());

                if (user is null)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                if (!user.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("User is inactive. Please contact support.");
                }

                // TODO: Check if user is locked out
                var signInResult = await _signinManager
                    .CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);

                if (!signInResult.Succeeded)
                {
                    if (signInResult.IsLockedOut)
                    {
                        return ApiResponse<LoginResponse>.ErrorResult("Account was locked");
                    }

                    if (signInResult.RequiresTwoFactor)
                    {
                        return ApiResponse<LoginResponse>.ErrorResult("Two factor auth is required");
                    }

                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

                var revokedCount = await _refreshTokensRepository
                    .RevokeAllUserTokensAsync(user.Id, "New login detectect - previous sessions was invalidated");

                if (revokedCount > 0)
                {
                    _logger.LogDebug("Revoked {RevokedCount} previous tokens for user {UserId}", revokedCount, user.Id);
                }

                user.LastLoginDate = DateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;
                user.LastLoginUserAgent = userAgent;

                await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);

                var tokenUserData = new TokenUserData
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Name = user.Name!,
                    Role = roles.FirstOrDefault() ?? "Customer",
                    IsActive = user.IsActive
                };

                var accessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // TODO: change to 30:7 or review
                var refreshTokenExpiryDays = command.RememberMe ? 7:1;

                var refreshTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    UserId = user.Id,
                    IsActive = true,
                    CreatedByIp = ipAddress,
                    UserAgent = userAgent
                };

                await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

                var response = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = _jwtTokenService.GetTokenExpiration(accessToken),
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Name = user.Name!,
                        Surname = user.Surname,
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber,
                        Role = roles.FirstOrDefault() ?? "User",
                        CreatedAt = user.CreatedAt,
                        IsActive = user.IsActive,
                        LastLoginDate = user.LastLoginDate
                    }
                };

                return ApiResponse<LoginResponse>.SuccessResult(response, "Login succesfull");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.ErrorResult($"An error occurred while login was requested: {ex.Message}");
            }
        }

        private static string? GetClientIpAddress(HttpContext? context)
        {
            if (context is null)
            {
                return null;
            }

            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }

            var clientIp = context.Request.Headers["X-Client-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientIp))
            {
                return clientIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
