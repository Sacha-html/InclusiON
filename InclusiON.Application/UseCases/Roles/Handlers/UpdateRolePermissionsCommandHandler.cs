using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roles.Commands;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roles;
using Microsoft.Extensions.Logging;

namespace InclusiON.Application.UseCases.Roles.Handlers
{
    public class UpdateRolePermissionsCommandHandler
        : ICommandHandler<UpdateRolePermissionsCommand, ApiResponse<RoleResponse>>
    {
        private readonly IRoleService _roleService;
        private readonly IRefreshTokensRepository _refreshTokens;
        private readonly ILogger<UpdateRolePermissionsCommandHandler> _logger;

        public UpdateRolePermissionsCommandHandler(
            IRoleService roleService,
            IRefreshTokensRepository refreshTokens,
            ILogger<UpdateRolePermissionsCommandHandler> logger)
        {
            _roleService    = roleService;
            _refreshTokens  = refreshTokens;
            _logger         = logger;
        }

        public async Task<ApiResponse<RoleResponse>> HandleAsync(
            UpdateRolePermissionsCommand command, CancellationToken cancellationToken)
        {
            // 1. Verificar que el rol existe y obtener sus datos antes del update
            var role = await _roleService.GetByIdAsync(command.RoleId, cancellationToken);
            if (role is null)
                return ApiResponse<RoleResponse>.NotFound("Rol");

            // 2. Reemplazar los permisos
            var updated = await _roleService.UpdatePermissionsAsync(
                command.RoleId, command.Permissions, cancellationToken);

            if (!updated)
                return ApiResponse<RoleResponse>.NotFound("Rol");

            // 3. Revocar sesiones de todos los usuarios con este rol
            var affectedUserIds = await _roleService.GetUserIdsByRoleAsync(
                command.RoleId, cancellationToken);

            if (affectedUserIds.Count > 0)
            {
                var revoked = await _refreshTokens.RevokeAllUsersTokensAsync(
                    affectedUserIds,
                    RevokeReasons.RolePermissionsUpdated,
                    cancellationToken);

                _logger.LogInformation(
                    "Permisos actualizados para rol {RoleId}. {Revoked} sesiones revocadas.",
                    command.RoleId, revoked);
            }

            var permissions = command.Permissions.Distinct().OrderBy(p => p).ToList();

            return ApiResponse<RoleResponse>.SuccessResult(
                RoleMapper.ToResponse(role, permissions),
                "Permisos actualizados exitosamente");
        }
    }
}
