using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IReportsRepository
    {
        /// <summary>
        /// Obtiene un reporte por su ID con las relaciones necesarias.
        /// </summary>
        Task<Report?> GetByIdAsync(int reportId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea un nuevo reporte en la base de datos.
        /// </summary>
        Task<Report> CreateAsync(Report report, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un reporte existente.
        /// </summary>
        Task UpdateAsync(Report report, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene lista paginada de reportes con filtros.
        /// </summary>
        Task<PagedResponse<Report>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            string? personId,
            string? professionalId,
            string? reportTypeId,
            bool? isActive,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default);
    }
}