using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones CRUD de profesionales.
    /// </summary>
    public interface IProfessionalsRepository
    {
        /// <summary>
        /// Obtiene un profesional por su ID con las relaciones necesarias.
        /// </summary>
        Task<Professional?> GetByIdAsync(Guid professionalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un profesional por su UserId.
        /// </summary>
        Task<Professional?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe un documento duplicado.
        /// </summary>
        Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludeProfessionalId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea un nuevo profesional en la base de datos.
        /// </summary>
        Task<Professional> CreateAsync(Professional professional, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un profesional existente.
        /// </summary>
        Task UpdateAsync(Professional professional, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene lista paginada de profesionales con filtros.
        /// </summary>
        Task<PagedResponse<Professional>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            string? specialty,
            bool? isActive,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default);
    }
}
