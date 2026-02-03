using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.Entities.Models;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Servicio que cachea roles por request (scoped).
    /// Evita múltiples llamadas a BD para el mismo usuario en un request.
    /// </summary>
    public class UserRoleService : IUserRoleService
    {
        private readonly UserManager<User> _userManager;
        private readonly ConcurrentDictionary<Guid, IList<string>> _roleCache = new();

        public UserRoleService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Intentar obtener del cache primero
            if (_roleCache.TryGetValue(userId, out var cachedRoles))
            {
                return cachedRoles;
            }

            // Si no está en cache, obtener de BD
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return Array.Empty<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Guardar en cache (thread-safe)
            _roleCache.TryAdd(userId, roles);

            return roles;
        }

        public async Task<string> GetPrimaryRoleAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var roles = await GetRolesAsync(userId, cancellationToken);
            return roles.FirstOrDefault() ?? "User";
        }

        public void InvalidateCache(Guid userId)
        {
            _roleCache.TryRemove(userId, out _);
        }
    }
}
