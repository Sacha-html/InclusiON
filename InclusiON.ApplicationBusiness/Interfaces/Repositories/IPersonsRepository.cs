using InclusiON.Entities.Models;

namespace InclusiON.ApplicationBusiness.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones CRUD de personas con discapacidad.
    /// </summary>
    public interface IPersonsRepository
    {
        /// <summary>
        /// Obtiene una persona por su ID con todas las relaciones necesarias.
        /// </summary>
        Task<PersonWithDisability?> GetByIdAsync(Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una persona por su UserId.
        /// </summary>
        Task<PersonWithDisability?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe un documento duplicado.
        /// </summary>
        Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludePersonId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea una nueva persona en la base de datos.
        /// </summary>
        Task<PersonWithDisability> CreateAsync(PersonWithDisability person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una persona existente.
        /// </summary>
        Task UpdateAsync(PersonWithDisability person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene lista paginada de personas con filtros.
        /// </summary>
        Task<(List<PersonWithDisability> Items, int TotalCount)> GetPagedAsync(
            int skip,
            int take,
            string? search,
            int? disabilityTypeId,
            int? autonomyLevelId,
            bool? isActive,
            string? sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default);
    }
}
