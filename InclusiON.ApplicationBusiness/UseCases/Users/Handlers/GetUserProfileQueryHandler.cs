using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Users.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.ApplicationBusiness.UseCases.Users.Handlers
{
    public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokensRepository _refreshTokenRepository;
        private readonly IPermissionService _permissionService;

        public GetUserProfileQueryHandler(
            IIdentityService identityService,
            IRefreshTokensRepository refreshTokenRepository,
            IPermissionService permissionService)
        {
            _identityService = identityService;
            _refreshTokenRepository = refreshTokenRepository;
            _permissionService = permissionService;
        }

        public async Task<ApiResponse<UserProfileResponse>> HandleAsync(GetUserProfileQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _identityService.FindByIdAsync(query.UserId);

                if (user is null)
                {
                    return ApiResponse<UserProfileResponse>.ErrorResult(
                        ErrorCode.UserNotFound,
                        "Usuario no encontrado");
                }

                if (!user.IsActive)
                {
                    return ApiResponse<UserProfileResponse>.ErrorResult(
                        ErrorCode.AccountInactive,
                        "Cuenta de usuario desactivada");
                }

                var roles = await _identityService.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "User";

                var permissions = await _permissionService.GetRolesPermissionsAsync(roles, cancellationToken);

                var activeSessionsCount = await _refreshTokenRepository
                    .GetActiveTokensCountAsync(user.Id, cancellationToken);

                var response = new UserProfileResponse
                {
                    Id = user.Id,
                    Name = user.Name ?? "Unknown",
                    Surname = user.Surname!,
                    Email = user.Email ?? string.Empty,
                    Phone = user.PhoneNumber,
                    Role = primaryRole,
                    IsActive = user.IsActive,
                    ActiveSessionsCount = activeSessionsCount,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    Permissions = permissions
                };

                return ApiResponse<UserProfileResponse>.SuccessResult(response);
            }
            catch (Exception)
            {
                return ApiResponse<UserProfileResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al obtener perfil de usuario");
            }
        }
    }
}
