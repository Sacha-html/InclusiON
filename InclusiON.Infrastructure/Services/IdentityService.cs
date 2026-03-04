using Microsoft.AspNetCore.Identity;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Implementacion de IIdentityService que delega en UserManager y SignInManager.
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IdentityService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> FindByIdAsync(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return MapIdentityResult(result);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            return MapIdentityResult(result);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> AddToRoleAsync(User user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return MapIdentityResult(result);
        }

        public async Task<IList<string>> GetRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<SignInStatus> CheckPasswordAsync(User user, string password, bool lockoutOnFailure)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);

            if (result.Succeeded)
                return SignInStatus.Success;
            if (result.IsLockedOut)
                return SignInStatus.LockedOut;
            if (result.RequiresTwoFactor)
                return SignInStatus.RequiresTwoFactor;

            return SignInStatus.Failed;
        }

        public async Task<bool> IsLockedOutAsync(User user)
        {
            return await _userManager.IsLockedOutAsync(user);
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(User user)
        {
            return await _userManager.GetLockoutEndDateAsync(user);
        }

        public async Task<int> GetAccessFailedCountAsync(User user)
        {
            return await _userManager.GetAccessFailedCountAsync(user);
        }

        public async Task AccessFailedAsync(User user)
        {
            await _userManager.AccessFailedAsync(user);
        }

        public async Task ResetAccessFailedCountAsync(User user)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        private static (bool Succeeded, IEnumerable<string> Errors) MapIdentityResult(IdentityResult result)
        {
            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }
    }
}
