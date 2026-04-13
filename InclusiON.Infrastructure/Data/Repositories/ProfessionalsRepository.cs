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
    public class ProfessionalsRepository : IProfessionalsRepository
    {
        private readonly AppDbContext _context;

        public ProfessionalsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Professional?> GetByIdAsync(Guid professionalId, CancellationToken cancellationToken = default)
        {
            return await _context.Professionals
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == professionalId, cancellationToken);
        }

        public async Task<Professional?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Professionals
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludeProfessionalId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Professionals
                .Where(p => p.DocumentNumber == documentNumber);

            if (excludeProfessionalId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProfessionalId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsLicenseNumberAsync(string licenseNumber, Guid? excludeProfessionalId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Professionals
                .Where(p => p.LicenseNumber == licenseNumber);

            if (excludeProfessionalId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProfessionalId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsProfessionalEmailAsync(string email, Guid? excludeProfessionalId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Professionals
                .Where(p => p.Email == email);

            if (excludeProfessionalId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProfessionalId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<Professional> CreateAsync(Professional professional, CancellationToken cancellationToken = default)
        {
            await _context.Professionals.AddAsync(professional, cancellationToken);

            return professional;
        }

        public Task UpdateAsync(Professional professional, CancellationToken cancellationToken = default)
        {
            _context.Professionals.Update(professional);
            return Task.CompletedTask;
        }

        public async Task<PagedResponse<Professional>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            string? specialty,
            bool? isActive,
            string? status,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Professionals
                .Include(p => p.User)
                .AsNoTracking()
                .Where(p => p.Status != ProfessionalStatusEnum.Pending && p.Status != ProfessionalStatusEnum.Rejected);

            // Filtros
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.FirstName.Contains(searchLower) ||
                    p.LastName.Contains(searchLower) ||
                    (p.DocumentNumber != null && p.DocumentNumber.Contains(search)) ||
                    (p.Phone != null && p.Phone.Contains(search)) ||
                    (p.User.Email != null && p.User.Email.Contains(searchLower)));
            }

            if (!string.IsNullOrWhiteSpace(specialty))
            {
                var specialtyLower = specialty.ToLower();
                query = query.Where(p => p.Specialty != null && p.Specialty.Contains(specialtyLower));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Status == ProfessionalStatusEnum.Approved && p.User.IsActive);
                }
                else if (status.Equals("suspended", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Status == ProfessionalStatusEnum.Suspended);
                }
                else if (status.Equals("terminated", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Status == ProfessionalStatusEnum.Terminated);
                }
            }
            else if (isActive.HasValue)
            {
                query = query.Where(p => p.User.IsActive == isActive.Value);
            }

            if (institutionIds is not null && institutionIds.Count > 0)
            {
                var professionalIdsInInstitution = _context.ProfessionalInstitutions
                    .Where(pi => institutionIds.Contains(pi.InstitutionId) && pi.IsActive)
                    .Select(pi => pi.ProfessionalId)
                    .Distinct();

                query = query.Where(p => professionalIdsInInstitution.Contains(p.Id));
            }

            var sortMappings = new Dictionary<SortField, Expression<Func<Professional, object>>>
            {
                [SortField.Id] = p => p.Id,
                [SortField.FirstName] = p => p.FirstName,
                [SortField.LastName] = p => p.LastName,
                [SortField.CreatedAt] = p => p.CreatedAt,
                [SortField.Email] = p => p.User.Email ?? "",
                [SortField.Specialty] = p => p.Specialty ?? "",
                [SortField.LicenseNumber] = p => p.LicenseNumber ?? "",
                [SortField.Status] = p => p.Status
            };

            return await query.ToPagedAsync(
                page, pageSize,
                sortBy, sortDirection,
                sortMappings,
                cancellationToken);
        }

        public async Task<List<int>> GetInstitutionIdsAsync(Guid professionalId, CancellationToken cancellationToken = default)
        {
            return await _context.ProfessionalInstitutions
                .Where(pi => pi.ProfessionalId == professionalId && pi.IsActive)
                .Select(pi => pi.InstitutionId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResponse<Professional>> GetPendingPagedAsync(
            int page,
            int pageSize,
            string? search,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Professionals
                .Include(p => p.User)
                .Include(p => p.ProfessionalInstitutions)
                .AsNoTracking()
                .Where(p => p.Status == ProfessionalStatusEnum.Pending)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.FirstName.Contains(searchLower) ||
                    p.LastName.Contains(searchLower) ||
                    (p.DocumentNumber != null && p.DocumentNumber.Contains(search)) ||
                    (p.User.Email != null && p.User.Email.Contains(searchLower)));
            }

            if (institutionIds is not null && institutionIds.Count > 0)
            {
                query = query.Where(p => p.ProfessionalInstitutions.Any(pi => institutionIds.Contains(pi.InstitutionId) && pi.IsActive));
            }

            var sortMappings = new Dictionary<SortField, Expression<Func<Professional, object>>>
            {
                [SortField.Id] = p => p.Id,
                [SortField.FirstName] = p => p.FirstName,
                [SortField.LastName] = p => p.LastName,
                [SortField.CreatedAt] = p => p.CreatedAt,
                [SortField.Email] = p => p.Email ?? "",
                [SortField.Specialty] = p => p.Specialty ?? ""
            };

            return await query.ToPagedAsync(
                page, pageSize,
                sortBy, sortDirection,
                sortMappings,
                cancellationToken);
        }

        public async Task<int> GetPendingCountAsync(List<int>? institutionIds = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Professionals
                .AsNoTracking()
                .Where(p => p.Status == ProfessionalStatusEnum.Pending);

            if (institutionIds is not null && institutionIds.Count > 0)
            {
                var professionalIdsInInstitution = _context.ProfessionalInstitutions
                    .Where(pi => institutionIds.Contains(pi.InstitutionId) && pi.IsActive)
                    .Select(pi => pi.ProfessionalId)
                    .Distinct();

                query = query.Where(p => professionalIdsInInstitution.Contains(p.Id));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task AddStatusHistoryAsync(ProfessionalStatusHistory history, CancellationToken cancellationToken = default)
        {
            await _context.ProfessionalStatusHistories.AddAsync(history, cancellationToken);
        }

        public async Task<List<ProfessionalStatusHistory>> GetStatusHistoryAsync(Guid professionalId, CancellationToken cancellationToken = default)
        {
            return await _context.ProfessionalStatusHistories
                .AsNoTracking()
                .Where(h => h.ProfessionalId == professionalId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Professional>> GetInactiveProfessionalsAsync(int inactiveDays, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-inactiveDays);
            return await _context.Professionals
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.Status == ProfessionalStatusEnum.Approved
                    && p.User.IsActive
                    && (p.User.LastLoginDate == null || p.User.LastLoginDate < cutoffDate))
                .ToListAsync(cancellationToken);
        }
    }
}
