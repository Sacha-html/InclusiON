using Microsoft.AspNetCore.Identity;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.UseCases.Users.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Users.Handlers
{
    public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IRefreshTokensRepository _refreshTokenRepository;
        private readonly IPermissionService _permissionService;

        public GetUserProfileQueryHandler(
            UserManager<User> userManager,
            IRefreshTokensRepository refreshTokenRepository,
            IPermissionService permissionService)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _permissionService = permissionService;
        }

        public async Task<ApiResponse<UserProfileResponse>> HandleAsync(GetUserProfileQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(query.UserId.ToString());

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

                var roles = await _userManager.GetRolesAsync(user);
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
