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

        /// <summary>
        /// Diccionario estático de permisos por rol.
        /// Evita crear nuevas listas en cada request.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RolePermissions =
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["admin"] = new[]
                {
                    "read:profile", "update:profile", "delete:profile",
                    "read:users", "create:users", "update:users", "delete:users",
                    "read:products", "create:products", "update:products", "delete:products",
                    "read:reports", "create:reports"
                },
                ["manager"] = new[]
                {
                    "read:profile", "update:profile",
                    "read:users", "create:users", "update:users",
                    "read:products", "create:products", "update:products",
                    "read:reports"
                },
                ["professional"] = new[]
                {
                    "read:profile", "update:profile",
                    "read:persons", "update:persons",
                    "read:activities", "create:activities", "update:activities",
                    "read:reports", "create:reports"
                },
                ["family"] = new[]
                {
                    "read:profile", "update:profile",
                    "read:persons",
                    "read:activities",
                    "read:reports"
                },
                ["person"] = new[]
                {
                    "read:profile",
                    "read:activities"
                },
                ["employee"] = new[]
                {
                    "read:profile", "update:profile",
                    "read:products", "update:products"
                },
                ["customer"] = new[]
                {
                    "read:profile", "update:profile"
                }
            };

        private static readonly IReadOnlyList<string> DefaultPermissions = new[] { "read:profile" };

        public GetUserProfileQueryHandler(UserManager<User> userManager, IRefreshTokensRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
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
                    Permissions = GetUserPermissions(primaryRole)
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

        private static List<string> GetUserPermissions(string role)
        {
            // Búsqueda O(1) en diccionario + retorna copia para evitar mutaciones
            return RolePermissions.TryGetValue(role, out var permissions)
                ? permissions.ToList()
                : DefaultPermissions.ToList();
        }
    }
}
