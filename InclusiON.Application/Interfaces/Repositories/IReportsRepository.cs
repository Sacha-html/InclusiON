using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IReportsRepository
    {
        Task<Report?> GetByIdAsync(int reportId, CancellationToken cancellationToken = default);
        Task<Report> CreateAsync(Report report, CancellationToken cancellationToken = default);
        Task UpdateAsync(Report report, CancellationToken cancellationToken = default);

        Task<PagedResponse<Report>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            string? personId,
            string? professionalId,
            string? reportTypeId,
            bool? isActive,
            string? status,
            DateTime? dateFrom,
            DateTime? dateTo,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reportes aprobados visibles para un familiar (filtra por las personas a cargo del familiar).
        /// </summary>
        Task<PagedResponse<Report>> GetFamilyPagedAsync(
            Guid familyRepresentativeId,
            int page,
            int pageSize,
            string? reportTypeId,
            DateTime? dateFrom,
            DateTime? dateTo,
            SortField? sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default);
    }
}
