using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Implementación de IRoleService usando RoleManager y AppDbContext.
    /// Encapsula toda la interacción con Identity Roles y RoleClaims.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly AppDbContext _context;

        public RoleService(RoleManager<IdentityRole<Guid>> roleManager, AppDbContext context)
        {
            _roleManager = roleManager;
            _context     = context;
        }

        public async Task<List<RoleDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);

            // Carga todos los claims de una sola query para evitar N+1
            var allClaims = await _context.RoleClaims
                .Where(rc => rc.ClaimType == Permissions.ClaimType)
                .ToListAsync(cancellationToken);

            var claimsByRole = allClaims
                .GroupBy(rc => rc.RoleId)
                .ToDictionary(g => g.Key, g => g.Select(rc => rc.ClaimValue!).OrderBy(p => p).ToList());

            return roles.Select(r => new RoleDto(
                r.Id,
                r.Name!,
                r.NormalizedName!,
                claimsByRole.TryGetValue(r.Id, out var perms) ? perms : new List<string>()))
                .ToList();
        }

        public async Task<RoleDto?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role is null) return null;

            var permissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => rc.ClaimValue!)
                .OrderBy(p => p)
                .ToListAsync(cancellationToken);

            return new RoleDto(role.Id, role.Name!, role.NormalizedName!, permissions);
        }

        public async Task<bool> UpdatePermissionsAsync(
            Guid roleId,
            IEnumerable<string> permissions,
            CancellationToken cancellationToken)
        {
            var roleExists = await _roleManager.Roles
                .AnyAsync(r => r.Id == roleId, cancellationToken);

            if (!roleExists) return false;

            // Eliminar permisos actuales y agregar los nuevos en una transacción implícita de EF
            var existing = await _context.RoleClaims
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == Permissions.ClaimType)
                .ToListAsync(cancellationToken);

            _context.RoleClaims.RemoveRange(existing);

            foreach (var permission in permissions.Distinct())
            {
                _context.RoleClaims.Add(new IdentityRoleClaim<Guid>
                {
                    RoleId     = roleId,
                    ClaimType  = Permissions.ClaimType,
                    ClaimValue = permission
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IList<Guid>> GetUserIdsByRoleAsync(Guid roleId, CancellationToken cancellationToken)
        {
            return await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);
        }
    }
}
