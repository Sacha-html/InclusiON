using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
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
                var searchLower = search.ToLower();
                query = query.Where(r =>
                    r.Title.Contains(searchLower) ||
                    r.Content.Contains(searchLower) ||
                    (r.AchievedGoals != null && r.AchievedGoals.Contains(searchLower)) ||
                    (r.AreasToReinforce != null && r.AreasToReinforce.Contains(searchLower)) ||
                    (r.FutureRecommendations != null && r.FutureRecommendations.Contains(searchLower)) ||
                    (r.NextObjectives != null && r.NextObjectives.Contains(searchLower)));
            }

            if (!string.IsNullOrWhiteSpace(personId) && Guid.TryParse(personId, out var parsedPersonId))
            {
                query = query.Where(r => r.PersonId == parsedPersonId);
            }

            if (!string.IsNullOrWhiteSpace(professionalId) && Guid.TryParse(professionalId, out var parsedProfessionalId))
            {
                query = query.Where(r => r.ProfessionalId == parsedProfessionalId);
            }

            if (!string.IsNullOrWhiteSpace(reportTypeId) && int.TryParse(reportTypeId, out var parsedReportTypeId))
            {
                query = query.Where(r => r.ReportTypeId == parsedReportTypeId);
            }

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            if (institutionIds is not null && institutionIds.Count > 0)
            {
                var professionalIdsInInstitution = _context.ProfessionalInstitutions
                    .Where(pi => institutionIds.Contains(pi.InstitutionId) && pi.IsActive)
                    .Select(pi => pi.ProfessionalId)
                    .Distinct();

                query = query.Where(r => professionalIdsInInstitution.Contains(r.ProfessionalId));
            }

            var sortMappings = new Dictionary<SortField, Expression<Func<Report, object>>>
            {
                [SortField.Id] = r => r.Id,
                [SortField.Title] = r => r.Title,
                [SortField.ReportDate] = r => r.ReportDate,
                [SortField.CreatedAt] = r => r.CreatedAt
            };

            return await query.ToPagedAsync(
                page,
                pageSize,
                sortBy,
                sortDirection,
                sortMappings,
                cancellationToken);
        }
    }
}