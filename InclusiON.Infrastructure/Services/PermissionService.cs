using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using InclusiON.ApplicationBusiness.Constants;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.Entities.Models;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Servicio que obtiene permisos de AspNetRoleClaims.
    /// Implementa cache para evitar consultas repetidas.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public PermissionService(
            AppDbContext context,
            UserManager<User> userManager,
            IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return await GetRolesPermissionsAsync(roles, cancellationToken);
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"RolePermissions_{roleName}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
            {
                return cachedPermissions;
            }

            // Obtener el rol
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpperInvariant(), cancellationToken)
                .ConfigureAwait(false);

            if (role == null)
            {
                return new List<string>();
            }

            // Obtener los claims de tipo "permission" para este rol
            var permissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => rc.ClaimValue!)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Cachear resultado
            _cache.Set(cacheKey, permissions, CacheDuration);

            return permissions;
        }

        public async Task<List<string>> GetRolesPermissionsAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            var allPermissions = new HashSet<string>();

            foreach (var roleName in roleNames)
            {
                var rolePermissions = await GetRolePermissionsAsync(roleName, cancellationToken);
                foreach (var permission in rolePermissions)
                {
                    allPermissions.Add(permission);
                }
            }

            return allPermissions.ToList();
        }
    }
}
