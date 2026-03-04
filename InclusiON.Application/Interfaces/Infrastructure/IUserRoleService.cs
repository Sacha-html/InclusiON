namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Servicio para obtener roles de usuario con cache por request.
    /// Evita múltiples llamadas a GetRolesAsync (N+1).
    /// </summary>
    public interface IUserRoleService
    {
        /// <summary>
        /// Obtiene los roles del usuario. Cachea el resultado por request.
        /// </summary>
        Task<IList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el rol principal del usuario.
        /// </summary>
        Task<string> GetPrimaryRoleAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalida el cache de roles para un usuario específico.
        /// Útil después de cambiar roles.
        /// </summary>
        void InvalidateCache(Guid userId);
    }
}
