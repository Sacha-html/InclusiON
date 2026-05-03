using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using InclusiON.Api.Extensions;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Roles.Commands;
using InclusiON.Application.UseCases.Roles.Queries;
using InclusiON.DTOs.Requests.Roles;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roles;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = Permissions.Settings.Read)]
    public class RolesController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public RolesController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Obtiene todos los roles con sus permisos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RoleResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<RoleResponse>>>> GetRoles(
            [FromServices] IQueryHandler<GetRolesQuery, ApiResponse<List<RoleResponse>>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(new GetRolesQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los permisos de un rol específico.
        /// </summary>
        [HttpGet("{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> GetRoleById(
            Guid roleId,
            [FromServices] IQueryHandler<GetRoleByIdQuery, ApiResponse<RoleResponse>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(new GetRoleByIdQuery(roleId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene la lista de todos los permisos disponibles en el sistema.
        /// </summary>
        [HttpGet("available-permissions")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<List<string>>> GetAvailablePermissions()
        {
            var permissions = Permissions.GetAll().OrderBy(p => p).ToList();
            return Ok(ApiResponse<List<string>>.SuccessResult(permissions));
        }

        /// <summary>
        /// Actualiza los permisos de un rol.
        /// Invalida el cache de permisos del rol afectado (ASP.NET concern, permanece en controller).
        /// </summary>
        [HttpPut("{roleId}/permissions")]
        [Authorize(Policy = Permissions.Settings.Update)]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> UpdateRolePermissions(
            Guid roleId,
            [FromBody] UpdateRolePermissionsRequest request,
            [FromServices] ICommandHandler<UpdateRolePermissionsCommand, ApiResponse<RoleResponse>> handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdateRolePermissionsCommand(roleId, request.Permissions);
            var result  = await handler.HandleAsync(command, cancellationToken);

            if (result.Success)
            {
                // Invalidar cache de permisos para que el próximo request recargue desde DB.
                // La clave usa NormalizedName; al no tenerla aquí la removemos por el nombre del rol.
                _cache.Remove($"RolePermissions_{result.Data!.Name.ToUpperInvariant()}");
            }

            return result.ToActionResult();
        }
    }
}
