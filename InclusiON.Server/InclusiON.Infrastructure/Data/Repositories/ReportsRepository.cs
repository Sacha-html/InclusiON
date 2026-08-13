using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Infrastructure.Extensions;
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
            List<string>? personIds = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Reports
                .Include(r => r.Person)
                .Include(r => r.Professional)
                .ThenInclude(p => p.User)
                .Include(r => r.ReportType)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search}%";
                query = query.Where(r =>
                    EF.Functions.ILike(r.Title, pattern) ||
                    EF.Functions.ILike(r.Content, pattern) ||
                    (r.AchievedGoals != null && EF.Functions.ILike(r.AchievedGoals, pattern)) ||
                    (r.AreasToReinforce != null && EF.Functions.ILike(r.AreasToReinforce, pattern)));
            }

            if (!string.IsNullOrWhiteSpace(personId) && Guid.TryParse(personId, out var parsedPersonId))
                query = query.Where(r => r.PersonId == parsedPersonId);

            if (personIds is { Count: > 0 })
            {
                var parsedPersonIds = personIds
                    .Where(id => Guid.TryParse(id, out _))
                    .Select(id => Guid.Parse(id))
                    .ToList();
                if (parsedPersonIds.Count > 0)
                    query = query.Where(r => parsedPersonIds.Contains(r.PersonId));
            }

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
            var query = _context.Reports
                .Include(r => r.Person)
                .Include(r => r.Professional)
                .Include(r => r.ReportType)
                .AsNoTracking()
                .Where(r => r.Status == ReportStatus.Approved &&
                            _context.PersonRepresentatives.Any(pr =>
                                pr.RepresentativeId == familyRepresentativeId &&
                                pr.IsActive &&
                                pr.PersonId == r.PersonId));

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

        public async Task<(int Count, Report? Latest)> GetApprovedReportsSummaryAsync(
            Guid personId, CancellationToken cancellationToken = default)
        {
            // Single query: load ordered, count in memory (approved reports per person are typically few)
            var reports = await _context.Reports
                .AsNoTracking()
                .Where(r => r.PersonId == personId
                         && r.Status == ReportStatus.Approved
                         && r.IsActive)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync(cancellationToken);

            return (reports.Count, reports.FirstOrDefault());
        }

        public async Task<Dictionary<Guid, (int Count, Report? Latest)>> GetApprovedReportsSummaryByPersonIdsAsync(
            IEnumerable<Guid> personIds, CancellationToken cancellationToken = default)
        {
            var idList = personIds.ToList();
            if (idList.Count == 0) return new();

            var reports = await _context.Reports
                .AsNoTracking()
                .Where(r => idList.Contains(r.PersonId)
                         && r.Status == ReportStatus.Approved
                         && r.IsActive)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync(cancellationToken);

            return reports
                .GroupBy(r => r.PersonId)
                .ToDictionary(g => g.Key, g => (g.Count(), (Report?)g.First()));
        }

        public async Task<Report?> GetReportWithDetailsAsync(int reportId, CancellationToken cancellationToken = default)
        {
            return await _context.Reports
                .Include(r => r.Person)
                .Include(r => r.Professional)
                .Include(r => r.ReportType)
                .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
        }

        public async Task ReassignReportAsync(Report report, Guid newProfessionalId, DateTime assignedAt, CancellationToken cancellationToken = default)
        {
            var newProfessional = await _context.Professionals
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == newProfessionalId, cancellationToken);

            if (newProfessional is null || !newProfessional.IsActive || !(newProfessional.User?.IsActive ?? false))
                throw new InvalidOperationException("El profesional asignado no está activo o no existe.");

            var link = await _context.ProfessionalPersons
                .FirstOrDefaultAsync(pp => pp.ProfessionalId == newProfessional.Id && pp.PersonId == report.PersonId, cancellationToken);

            if (link is null)
            {
                link = new ProfessionalPerson
                {
                    ProfessionalId = newProfessional.Id,
                    PersonId = report.PersonId,
                    IsPrimaryProfessional = false,
                    CanSuperviseLogin = true,
                    IsActive = true,
                    AssignedAt = assignedAt
                };
                _context.ProfessionalPersons.Add(link);
            }
            else if (!link.IsActive)
            {
                link.IsActive = true;
                link.AssignedAt = assignedAt;
            }

            report.ProfessionalId = newProfessional.Id;
            report.Professional = newProfessional;
            report.UpdatedAt = assignedAt;
        }

        public async Task SoftDeleteReportAsync(Report report, DateTime updatedAt, CancellationToken cancellationToken = default)
        {
            report.IsActive = false;
            report.UpdatedAt = updatedAt;
        }

        public async Task<int> GetPendingReportsCountByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default)
        {
            return await _context.Reports
                .CountAsync(r => r.ProfessionalId == professionalId
                              && r.IsActive
                              && r.Status != ReportStatus.Approved, cancellationToken);
        }
    }
}
