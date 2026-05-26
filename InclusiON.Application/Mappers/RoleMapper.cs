using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Responses.Roles;

namespace InclusiON.Application.Mappers
{
    public static class RoleMapper
    {
        public static RoleResponse ToResponse(RoleDto role) => new()
        {
            Id          = role.Id,
            Name        = role.Name,
            Permissions = role.Permissions,
        };

        public static RoleResponse ToResponse(RoleDto role, List<string> permissions) => new()
        {
            Id          = role.Id,
            Name        = role.Name,
            Permissions = permissions,
        };
    }
}
