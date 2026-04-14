using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly AppDbContext _context;

        public ReportsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report?> GetByIdAsync(int reportId, CancellationToken cancellationToken = default)
        {
            return await _context.Reports
                .Include(r => r.Person)
                .Include(r => r.Professional)
                .Include(r => r.ReportType)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
        }

        public async Task<Report> CreateAsync(Report report, CancellationToken cancellationToken = default)
        {
            await _context.Reports.AddAsync(report, cancellationToken);
            return report;
        }

        public Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
        {
            _context.Reports.Update(report);
            return Task.CompletedTask;
        }

        public async Task<PagedResponse<Report>> GetPagedAsync(
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
            CancellationToken cancellationToken = default)
        {
            var query = _context.Reports
                .Include(r => r.Person)
                .Include(r => r.Professional)
                .Include(r => r.ReportType)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(s) ||
                    r.Content.ToLower().Contains(s) ||
                    (r.AchievedGoals != null && r.AchievedGoals.ToLower().Contains(s)) ||
                    (r.AreasToReinforce != null && r.AreasToReinforce.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(personId) && Guid.TryParse(personId, out var parsedPersonId))
                query = query.Where(r => r.PersonId == parsedPersonId);

            if (!string.IsNullOrWhiteSpace(professionalId) && Guid.TryParse(professionalId, out var parsedProfessionalId))
                query = query.Where(r => r.ProfessionalId == parsedProfessionalId);

            if (!string.IsNullOrWhiteSpace(reportTypeId) && int.TryParse(reportTypeId, out var parsedTypeId))
                query = query.Where(r => r.ReportTypeId == parsedTypeId);

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReportStatus>(status, ignoreCase: true, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            if (dateFrom.HasValue)
                query = query.Where(r => r.ReportDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(r => r.ReportDate <= dateTo.Value.AddDays(1).AddTicks(-1));

            if (institutionIds is { Count: > 0 })
            {
                var profIds = _context.ProfessionalInstitutions
                    .Where(pi => institutionIds.Contains(pi.InstitutionId) && pi.IsActive)
                    .Select(pi => pi.ProfessionalId)
                    .Distinct();

                query = query.Where(r => profIds.Contains(r.ProfessionalId));
            }

            var sortMappings = new Dictionary<SortField, Expression<Func<Report, object>>>
            {
                [SortField.Id]         = r => r.Id,
                [SortField.Title]      = r => r.Title,
                [SortField.ReportDate] = r => r.ReportDate,
                [SortField.CreatedAt]  = r => r.CreatedAt
            };

            return await query.ToPagedAsync(page, pageSize, sortBy, sortDirection, sortMappings, cancellationToken);
        }

        public async Task<PagedResponse<Report>> GetFamilyPagedAsync(
            Guid familyRepresentativeId,
            int page,
            int pageSize,
            string? reportTypeId,
            DateTime? dateFrom,
            DateTime? dateTo,
            SortField? sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default)
        {
            // Solo personas activamente vinculadas al familiar
            var personIds = await _context.PersonRepresentatives
                .Where(pr => pr.RepresentativeId == familyRepresentativeId && pr.IsActive)
                .Select(pr => pr.PersonId)
                .ToListAsync(cancellationToken);

            var query = _context.Reports
                .Include(r => r.Person)
                .Include(r => r.Professional)
                .Include(r => r.ReportType)
                .AsNoTracking()
                .Where(r => personIds.Contains(r.PersonId) && r.Status == ReportStatus.Approved);

            if (!string.IsNullOrWhiteSpace(reportTypeId) && int.TryParse(reportTypeId, out var parsedTypeId))
                query = query.Where(r => r.ReportTypeId == parsedTypeId);

            if (dateFrom.HasValue)
                query = query.Where(r => r.ReportDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(r => r.ReportDate <= dateTo.Value.AddDays(1).AddTicks(-1));

            var sortMappings = new Dictionary<SortField, Expression<Func<Report, object>>>
            {
                [SortField.Id]         = r => r.Id,
                [SortField.Title]      = r => r.Title,
                [SortField.ReportDate] = r => r.ReportDate,
                [SortField.CreatedAt]  = r => r.CreatedAt
            };

            return await query.ToPagedAsync(page, pageSize, sortBy, sortDirection, sortMappings, cancellationToken);
        }
    }
}
