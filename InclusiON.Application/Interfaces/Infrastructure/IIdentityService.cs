using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Abstraccion sobre la gestion de identidad de usuarios (UserManager + SignInManager).
    /// Permite desacoplar la capa de aplicacion de Microsoft.AspNetCore.Identity.
    /// </summary>
    public interface IIdentityService
    {
        // Busqueda
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByIdAsync(Guid userId);

        // Gestion de usuarios
        Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password);
        Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(User user);
        Task<(bool Succeeded, IEnumerable<string> Errors)> AddToRoleAsync(User user, string role);
        Task<IList<string>> GetRolesAsync(User user);

        // Autenticacion
        Task<SignInStatus> CheckPasswordAsync(User user, string password, bool lockoutOnFailure);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(User user, string currentPassword, string newPassword);

        // Lockout
        Task<bool> IsLockedOutAsync(User user);
        Task<DateTimeOffset?> GetLockoutEndDateAsync(User user);
        Task<int> GetAccessFailedCountAsync(User user);
        Task AccessFailedAsync(User user);
        Task ResetAccessFailedCountAsync(User user);
    }

    public enum SignInStatus
    {
        Success,
        Failed,
        LockedOut,
        RequiresTwoFactor
    }
}
