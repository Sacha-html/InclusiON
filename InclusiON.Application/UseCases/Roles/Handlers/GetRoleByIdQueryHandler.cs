using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Roles.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roles;

namespace InclusiON.Application.UseCases.Roles.Handlers
{
    public class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, ApiResponse<RoleResponse>>
    {
        private readonly IRoleService _roleService;

        public GetRoleByIdQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<ApiResponse<RoleResponse>> HandleAsync(
            GetRoleByIdQuery query, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetByIdAsync(query.RoleId, cancellationToken);

            if (role is null)
                return ApiResponse<RoleResponse>.NotFound("Rol");

            return ApiResponse<RoleResponse>.SuccessResult(
                new RoleResponse { Id = role.Id, Name = role.Name, Permissions = role.Permissions });
        }
    }
}
