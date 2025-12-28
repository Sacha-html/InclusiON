using Microsoft.AspNetCore.Identity;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.UseCases.Users.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.UseCases.Users.Handlers
{
    public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IRefreshTokensRepository _refreshTokenRepository;

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
                    return ApiResponse<UserProfileResponse>.ErrorResult("User not found");
                }

                if (!user.IsActive)
                {
                    return ApiResponse<UserProfileResponse>.ErrorResult("User account is deactivated");
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
                return ApiResponse<UserProfileResponse>.ErrorResult("An error occurred while processing the request");
            }

        }

        private static List<string> GetUserPermissions(string role)
        {
            return role.ToLower() switch
            {
                "admin" => new List<string>
                {
                    "read:profile", "update:profile", "delete:profile",
                    "read:users", "create:users", "update:users", "delete:users",
                    "read:products", "create:products", "update:products", "delete:products",
                    "read:reports", "create:reports"
                },
                "manager" => new List<string>
                {
                    "read:profile", "update:profile",
                    "read:users", "create:users", "update:users",
                    "read:products", "create:products", "update:products",
                    "read:reports"
                },
                "employee" => new List<string>
                {
                    "read:profile", "update:profile",
                    "read:products", "update:products"
                },
                "customer" => new List<string>
                {
                    "read:profile", "update:profile"
                },
                _ => new List<string> { "read:profile" }
            };
        }
    }
}
