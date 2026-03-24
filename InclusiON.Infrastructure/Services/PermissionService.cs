using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Servicio que obtiene permisos de AspNetRoleClaims.
    /// Implementa cache a nivel de rol y usuario para evitar consultas repetidas.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan RoleCacheDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(10);

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
            var cacheKey = $"UserPermissions_{userId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
                return cached;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetRolesPermissionsAsync(roles, cancellationToken);

            _cache.Set(cacheKey, permissions, UserCacheDuration);
            return permissions;
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var normalizedName = roleName.ToUpperInvariant();
            var cacheKey = $"RolePermissions_{normalizedName}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
                return cachedPermissions;

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);

            if (role == null)
                return new List<string>();

            var permissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => rc.ClaimValue!)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, permissions, RoleCacheDuration);
            return permissions;
        }

        public async Task<List<string>> GetRolesPermissionsAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            var allPermissions = new HashSet<string>();
            var uncachedRoles = new List<string>();

            // Recolectar permisos cacheados y detectar roles sin cache
            foreach (var roleName in roleNames)
            {
                var normalizedName = roleName.ToUpperInvariant();
                var cacheKey = $"RolePermissions_{normalizedName}";

                if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
                {
                    foreach (var p in cached)
                        allPermissions.Add(p);
                }
                else
                {
                    uncachedRoles.Add(roleName);
                }
            }

            // Batch query para roles no cacheados
            if (uncachedRoles.Count > 0)
            {
                var normalizedNames = uncachedRoles.Select(r => r.ToUpperInvariant()).ToList();

                var rolePermissions = await _context.Roles
                    .Where(r => normalizedNames.Contains(r.NormalizedName!))
                    .Join(_context.RoleClaims.Where(rc => rc.ClaimType == Permissions.ClaimType),
                        r => r.Id,
                        rc => rc.RoleId,
                        (r, rc) => new { RoleName = r.NormalizedName!, Permission = rc.ClaimValue! })
                    .ToListAsync(cancellationToken);

                // Agrupar por rol y cachear cada uno
                var grouped = rolePermissions.GroupBy(x => x.RoleName);
                foreach (var group in grouped)
                {
                    var permissions = group.Select(x => x.Permission).ToList();
                    _cache.Set($"RolePermissions_{group.Key}", permissions, RoleCacheDuration);

                    foreach (var p in permissions)
                        allPermissions.Add(p);
                }

                // Cachear roles que no existen (evitar queries repetidas)
                foreach (var normalizedName in normalizedNames)
                {
                    var cacheKey = $"RolePermissions_{normalizedName}";
                    if (!_cache.TryGetValue(cacheKey, out _))
                        _cache.Set(cacheKey, new List<string>(), RoleCacheDuration);
                }
            }

            return allPermissions.ToList();
        }
    }
}
