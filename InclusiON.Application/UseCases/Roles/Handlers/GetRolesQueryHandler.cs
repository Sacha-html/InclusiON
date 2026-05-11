using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roles.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roles;

namespace InclusiON.Application.UseCases.Roles.Handlers
{
    public class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, ApiResponse<List<RoleResponse>>>
    {
        private readonly IRoleService _roleService;

        public GetRolesQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<ApiResponse<List<RoleResponse>>> HandleAsync(
            GetRolesQuery query, CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetAllAsync(cancellationToken);

            var response = roles.Select(RoleMapper.ToResponse).ToList();

            return ApiResponse<List<RoleResponse>>.SuccessResult(response);
        }
    }
}
