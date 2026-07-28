using System.Linq.Expressions;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Servicio genérico para operaciones de administración de catálogos.
    /// Encapsula la lógica de duplicado, persistencia e integridad referencial
    /// para que el controller no dependa de AppDbContext directamente.
    /// </summary>
    public interface ICatalogAdminService
    {
        /// <summary>
        /// Verifica si existe al menos un registro que cumpla el predicado.
        /// Útil para chequeos de integridad referencial en deactivationCheck lambdas.
        /// </summary>
        Task<bool> AnyAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken)
            where TEntity : class;

        /// <summary>
        /// Crea una nueva entidad de catálogo verificando duplicados.
        /// </summary>
        Task<ApiResponse<TResponse>> CreateAsync<TEntity, TResponse>(
            Expression<Func<TEntity, bool>> duplicateCheck,
            Func<TEntity> createEntity,
            Func<TEntity, TResponse> toResponse,
            string entityDisplayName,
            CancellationToken cancellationToken)
            where TEntity : class
            where TResponse : class;

        /// <summary>
        /// Actualiza una entidad de catálogo existente verificando duplicados.
        /// </summary>
        Task<ApiResponse<TResponse>> UpdateAsync<TEntity, TResponse>(
            int id,
            Expression<Func<TEntity, bool>> duplicateCheck,
            Action<TEntity> updateEntity,
            Func<TEntity, TResponse> toResponse,
            string entityDisplayName,
            CancellationToken cancellationToken)
            where TEntity : class
            where TResponse : class;

        /// <summary>
        /// Cambia el estado activo/inactivo de una entidad de catálogo.
        /// Verifica integridad referencial antes de dar de baja (deactivationCheck).
        /// </summary>
        Task<ApiResponse<TResponse>> PatchStatusAsync<TEntity, TResponse>(
            int id,
            bool requestedIsActive,
            Func<TEntity, bool> getIsActive,
            Action<TEntity, bool> applyStatus,
            Func<TEntity, TResponse> toResponse,
            string entityDisplayName,
            CancellationToken cancellationToken,
            Func<int, CancellationToken, Task<string?>>? deactivationCheck = null)
            where TEntity : class
            where TResponse : class;
    }
}
