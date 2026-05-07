using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Telemetry;
using InclusiON.DTOs.Auth;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Domain.Models;
using InclusiON.Shared.Constants;

namespace InclusiON.Infrastructure.Services
{
    public class LoginSessionService : ILoginSessionService
    {
        private readonly IIdentityService _identityService;
        private readonly TokenServices _tokenServices;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextService _httpContextService;
        private readonly IVisualLoginRepository _visualLoginRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminInstitutionRepository _adminInstitutionRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly ITelemetryService _telemetryService;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILogger<LoginSessionService> _logger;

        private const int TrustedDeviceExpiryDays = 90;

        public LoginSessionService(
            IIdentityService identityService,
            TokenServices tokenServices,
            IPermissionService permissionService,
            IHttpContextService httpContextService,
            IVisualLoginRepository visualLoginRepository,
            IUnitOfWork unitOfWork,
            IAdminInstitutionRepository adminInstitutionRepository,
            IPersonsRepository personsRepository,
            IProfessionalsRepository professionalsRepository,
            ITelemetryService telemetryService,
            IDateTimeProvider dateTime,
            ILogger<LoginSessionService> logger)
        {
            _identityService = identityService;
            _tokenServices = tokenServices;
            _permissionService = permissionService;
            _httpContextService = httpContextService;
            _visualLoginRepository = visualLoginRepository;
            _unitOfWork = unitOfWork;
            _adminInstitutionRepository = adminInstitutionRepository;
            _personsRepository = personsRepository;
            _professionalsRepository = professionalsRepository;
            _telemetryService = telemetryService;
            _dateTime = dateTime;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> CreateLoginSessionAsync(
            User user,
            int refreshTokenExpiryDays,
            string revokeReason,
            string successMessage,
            CancellationToken cancellationToken)
        {
            var roles = await _identityService.GetRolesAsync(user);
            var permissions = await _permissionService.GetRolesPermissionsAsync(roles, cancellationToken);
            var primaryRole = roles.FirstOrDefault() ?? "Customer";

            var isGlobalAdmin = false;
            var institutionIds = new List<int>();

            if (primaryRole == "Admin")
            {
                institutionIds = await _adminInstitutionRepository
                    .GetActiveInstitutionIdsByAdminAsync(user.Id, cancellationToken);
                isGlobalAdmin = institutionIds.Count == 0;
            }

            // Para profesionales: resolver el entityId una sola vez al crear el token.
            // Los requests posteriores lo leen del claim sin consultar la BD.
            Guid? entityId = null;
            if (primaryRole == "Professional")
            {
                var professional = await _professionalsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                entityId = professional?.Id;
            }

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.Name!,
                Role = primaryRole,
                IsActive = user.IsActive,
                Permissions = permissions,
                IsGlobalAdmin = isGlobalAdmin,
                InstitutionIds = institutionIds,
                EntityId = entityId
            };

            var session = await CreateSessionCoreAsync(
                user, tokenUserData, refreshTokenExpiryDays,
                deviceId: null, rememberDevice: false,
                revokeReason, cancellationToken);

            // Load accessibility preferences if user has a person profile
            AccessibilityPreferences? accessibility = null;
            var person = await _personsRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (person != null)
            {
                accessibility = new AccessibilityPreferences
                {
                    RequiresLargeFont = person.RequiresLargeFont,
                    RequiresHighContrast = person.RequiresHighContrast,
                    VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                    SoundSensitivity = person.SoundSensitivity,
                    ColorBlindnessType = person.ColorBlindnessType
                };
            }

            var response = new LoginResponse
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                ExpiresAt = session.ExpiresAt,
                MustChangePassword = user.MustChangePassword,
                Accessibility = accessibility,
                User = new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name!,
                    Surname = user.Surname,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber,
                    Role = primaryRole,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate
                }
            };

            return ApiResponse<LoginResponse>.SuccessResult(response, successMessage);
        }

        public async Task<ApiResponse<VisualLoginResponse>> CreateVisualLoginSessionAsync(
            User user,
            PersonWithDisability person,
            int refreshTokenExpiryDays,
            string? deviceId,
            bool rememberDevice,
            string revokeReason,
            string successMessage,
            CancellationToken cancellationToken)
        {
            var roles = await _identityService.GetRolesAsync(user);
            var permissions = await _permissionService.GetRolesPermissionsAsync(roles, cancellationToken);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = $"{person.FirstName} {person.LastName}",
                Role = roles.FirstOrDefault() ?? "Person",
                IsActive = user.IsActive,
                Permissions = permissions,
                EntityId = person.Id
            };

            var session = await CreateSessionCoreAsync(
                user, tokenUserData, refreshTokenExpiryDays,
                deviceId, rememberDevice,
                revokeReason, cancellationToken);

            return ApiResponse<VisualLoginResponse>.SuccessResult(
                BuildVisualLoginResponse(user, session, person.FirstName, person.LastName,
                    person.AvatarColor ?? AvatarColors.DefaultPerson, "Person", roles,
                    new AccessibilityPreferences
                    {
                        RequiresLargeFont = person.RequiresLargeFont,
                        RequiresHighContrast = person.RequiresHighContrast,
                        VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                        SoundSensitivity = person.SoundSensitivity,
                        ColorBlindnessType = person.ColorBlindnessType
                    }),
                successMessage);
        }

        public async Task<ApiResponse<VisualLoginResponse>> CreateFamilyLoginSessionAsync(
            User user,
            FamilyRepresentative family,
            int refreshTokenExpiryDays,
            string? deviceId,
            bool rememberDevice,
            string revokeReason,
            string successMessage,
            CancellationToken cancellationToken)
        {
            var roles = await _identityService.GetRolesAsync(user);
            var permissions = await _permissionService.GetRolesPermissionsAsync(roles, cancellationToken);

            var tokenUserData = new TokenUserData
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = $"{family.FirstName} {family.LastName}",
                Role = roles.FirstOrDefault() ?? "Family",
                IsActive = user.IsActive,
                Permissions = permissions,
                EntityId = family.Id
            };

            var session = await CreateSessionCoreAsync(
                user, tokenUserData, refreshTokenExpiryDays,
                deviceId, rememberDevice,
                revokeReason, cancellationToken);

            return ApiResponse<VisualLoginResponse>.SuccessResult(
                BuildVisualLoginResponse(user, session, family.FirstName, family.LastName,
                    AvatarColors.DefaultFamily, "Family", roles, accessibility: null),
                successMessage);
        }

        #region Private Helpers

        private record SessionTokens(string AccessToken, string RefreshToken, DateTime ExpiresAt);

        private async Task<SessionTokens> CreateSessionCoreAsync(
            User user,
            TokenUserData tokenUserData,
            int refreshTokenExpiryDays,
            string? deviceId,
            bool rememberDevice,
            string revokeReason,
            CancellationToken cancellationToken)
        {
            var ipAddress = _httpContextService.GetClientIpAddress();
            var userAgent = _httpContextService.GetUserAgent();

            var accessToken = _tokenServices.JwtTokenService.GenerateAccessToken(tokenUserData);
            var refreshToken = _tokenServices.JwtTokenService.GenerateRefreshToken();

            _telemetryService.RecordTokenGenerated("access_token");
            _telemetryService.RecordTokenGenerated("refresh_token");

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                CreatedAt = _dateTime.UtcNow,
                ExpiresAt = _dateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                UserId = user.Id,
                IsActive = true,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var revokedCount = await _tokenServices.RefreshTokensRepository
                    .RevokeAllUserTokensAsync(user.Id, revokeReason);

                if (revokedCount > 0)
                {
                    _logger.LogDebug("Revoked {RevokedCount} previous tokens for user {UserId}", revokedCount, user.Id);
                }

                await _tokenServices.RefreshTokensRepository.CreateAsync(refreshTokenEntity, ct);

                if (rememberDevice && !string.IsNullOrEmpty(deviceId))
                {
                    var device = new TrustedDevice
                    {
                        UserId = user.Id,
                        DeviceId = deviceId,
                        DeviceName = "Dispositivo registrado",
                        Browser = _httpContextService.ParseBrowserFromUserAgent(userAgent),
                        RegisteredAt = _dateTime.UtcNow,
                        ExpiresAt = _dateTime.UtcNow.AddDays(TrustedDeviceExpiryDays),
                        IsActive = true
                    };
                    await _visualLoginRepository.RegisterTrustedDeviceAsync(device, ct);
                }

                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            // Update login metadata outside the token transaction — Identity regenerates
            // ConcurrencyStamp on UpdateAsync, causing DbUpdateConcurrencyException when
            // concurrent logins race on the same user (e.g. parallel E2E test workers).
            try
            {
                user.LastLoginDate = _dateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;
                user.LastLoginUserAgent = userAgent;
                await _identityService.UpdateUserAsync(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrent login for user {UserId} — LastLoginDate update skipped", user.Id);
            }

            var expiresAt = _tokenServices.JwtTokenService.GetTokenExpiration(accessToken);
            return new SessionTokens(accessToken, refreshToken, expiresAt);
        }

        private static VisualLoginResponse BuildVisualLoginResponse(
            User user,
            SessionTokens session,
            string firstName,
            string lastName,
            string avatarColor,
            string userType,
            IList<string> roles,
            AccessibilityPreferences? accessibility)
        {
            var displayName = $"{firstName} {lastName}".Trim();

            return new VisualLoginResponse
            {
                Success = true,
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken,
                ExpiresAt = session.ExpiresAt,
                MustChangePassword = user.MustChangePassword,
                User = new VisualLoginUserInfo
                {
                    Id = user.Id,
                    DisplayName = displayName,
                    Initial = displayName.Length > 0 ? displayName[0].ToString().ToUpper() : "?",
                    AvatarColor = avatarColor,
                    UserType = userType,
                    Roles = roles.ToList(),
                    Accessibility = accessibility
                }
            };
        }

        #endregion
    }
}
