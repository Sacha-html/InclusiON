namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Datos de un rol con sus permisos.
    /// </summary>
    public record RoleDto(Guid Id, string Name, string NormalizedName, List<string> Permissions);

    /// <summary>
    /// Abstraccion sobre la gestion de roles e Identity RoleClaims.
    /// Desacopla la capa de aplicacion de RoleManager y AppDbContext.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>Devuelve todos los roles con sus permisos.</summary>
        Task<List<RoleDto>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>Devuelve un rol por ID, o null si no existe.</summary>
        Task<RoleDto?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken);

        /// <summary>
        /// Reemplaza los permisos del rol por la lista indicada.
        /// Devuelve false si el rol no existe.
        /// </summary>
        Task<bool> UpdatePermissionsAsync(
            Guid roleId,
            IEnumerable<string> permissions,
            CancellationToken cancellationToken);

        /// <summary>Devuelve los IDs de todos los usuarios que tienen el rol.</summary>
        Task<IList<Guid>> GetUserIdsByRoleAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
