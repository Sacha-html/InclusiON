namespace InclusiON.Application.UseCases.Roles.Commands
{
    public record UpdateRolePermissionsCommand(Guid RoleId, IEnumerable<string> Permissions);
}
