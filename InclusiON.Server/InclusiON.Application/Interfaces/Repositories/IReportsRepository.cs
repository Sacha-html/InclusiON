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
            List<string>? personIds = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta y devuelve el reporte aprobado más reciente de una persona.
        /// Usado por el dashboard familiar.
        /// </summary>
        Task<(int Count, Report? Latest)> GetApprovedReportsSummaryAsync(
            Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk: cuenta y devuelve el reporte aprobado más reciente para un conjunto de personas.
        /// Devuelve un diccionario PersonId → (Count, Latest). Personas sin reportes no aparecen en el dict.
        /// </summary>
        Task<Dictionary<Guid, (int Count, Report? Latest)>> GetApprovedReportsSummaryByPersonIdsAsync(
            IEnumerable<Guid> personIds, CancellationToken cancellationToken = default);

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
        Task<Report?> GetReportWithDetailsAsync(int reportId, CancellationToken cancellationToken = default);
        Task ReassignReportAsync(Report report, Guid newProfessionalId, DateTime assignedAt, CancellationToken cancellationToken = default);
        Task SoftDeleteReportAsync(Report report, DateTime updatedAt, CancellationToken cancellationToken = default);
        Task<int> GetPendingReportsCountByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default);
    }
}
