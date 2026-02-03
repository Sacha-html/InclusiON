namespace InclusiON.ApplicationBusiness.Interfaces.Infrastructure
{
    /// <summary>
    /// Servicio para obtener permisos de usuarios/roles.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Obtiene los permisos de un usuario basado en sus roles.
        /// </summary>
        Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los permisos de un rol específico.
        /// </summary>
        Task<List<string>> GetRolePermissionsAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los permisos de múltiples roles.
        /// </summary>
        Task<List<string>> GetRolesPermissionsAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
    }
}
