using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using InclusiON.Application.Constants;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Requests.Roles;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roles;
using InclusiON.Shared.Resources;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = "settings:read")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMemoryCache _cache;

        public RolesController(
            AppDbContext context,
            RoleManager<IdentityRole<Guid>> roleManager,
            IMemoryCache cache)
        {
            _context = context;
            _roleManager = roleManager;
            _cache = cache;
        }

        /// <summary>
        /// Obtiene todos los roles con sus permisos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RoleResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<RoleResponse>>>> GetRoles(CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);

            var response = new List<RoleResponse>();

            foreach (var role in roles)
            {
                var permissions = await _context.RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == Permissions.ClaimType)
                    .Select(rc => rc.ClaimValue!)
                    .OrderBy(p => p)
                    .ToListAsync(cancellationToken);

                response.Add(new RoleResponse
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Permissions = permissions
                });
            }

            return Ok(ApiResponse<List<RoleResponse>>.SuccessResult(response));
        }

        /// <summary>
        /// Obtiene los permisos de un rol específico.
        /// </summary>
        [HttpGet("{roleId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> GetRoleById(Guid roleId, CancellationToken cancellationToken)
        {
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                return NotFound(ApiResponse<RoleResponse>.NotFound("Rol"));
            }

            var permissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => rc.ClaimValue!)
                .OrderBy(p => p)
                .ToListAsync(cancellationToken);

            var response = new RoleResponse
            {
                Id = role.Id,
                Name = role.Name!,
                Permissions = permissions
            };

            return Ok(ApiResponse<RoleResponse>.SuccessResult(response));
        }

        /// <summary>
        /// Obtiene la lista de todos los permisos disponibles en el sistema.
        /// </summary>
        [HttpGet("available-permissions")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<List<string>>> GetAvailablePermissions()
        {
            var permissions = new List<string>
            {
                // Usuarios
                "users:read", "users:create", "users:update", "users:delete",
                // Personas
                "persons:read", "persons:create", "persons:update", "persons:delete",
                // Profesionales
                "professionals:read", "professionals:create", "professionals:update", "professionals:delete",
                // Familiares
                "family:read", "family:create", "family:update", "family:delete",
                // Actividades
                "activities:read", "activities:create", "activities:update", "activities:delete", "activities:respond",
                // Reportes
                "reports:read", "reports:create", "reports:export",
                // Mensajes
                "messages:read", "messages:create",
                // Instituciones
                "institutions:read", "institutions:create", "institutions:update",
                // Invitaciones
                "invitations:read", "invitations:create",
                // Configuración
                "settings:read", "settings:update",
                // Auditoría
                "audit:read"
            };

            return Ok(ApiResponse<List<string>>.SuccessResult(permissions.OrderBy(p => p).ToList()));
        }

        /// <summary>
        /// Actualiza los permisos de un rol.
        /// </summary>
        [HttpPut("{roleId:guid}/permissions")]
        [Authorize(Policy = "settings:update")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> UpdateRolePermissions(
            Guid roleId,
            [FromBody] UpdateRolePermissionsRequest request,
            CancellationToken cancellationToken)
        {
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                return NotFound(ApiResponse<RoleResponse>.NotFound("Rol"));
            }

            // Eliminar permisos actuales
            var existingClaims = await _context.RoleClaims
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == Permissions.ClaimType)
                .ToListAsync(cancellationToken);

            _context.RoleClaims.RemoveRange(existingClaims);

            // Agregar nuevos permisos
            foreach (var permission in request.Permissions.Distinct())
            {
                _context.RoleClaims.Add(new IdentityRoleClaim<Guid>
                {
                    RoleId = roleId,
                    ClaimType = Permissions.ClaimType,
                    ClaimValue = permission
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidar cache de permisos para este rol
            _cache.Remove($"RolePermissions_{role.Name}");

            var response = new RoleResponse
            {
                Id = role.Id,
                Name = role.Name!,
                Permissions = request.Permissions.Distinct().OrderBy(p => p).ToList()
            };

            return Ok(ApiResponse<RoleResponse>.SuccessResult(response, "Permisos actualizados exitosamente"));
        }
    }
}
