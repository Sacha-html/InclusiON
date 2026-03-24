using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
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
                .FirstOrDefaultAsync(p => p.Id == professionalId, cancellationToken);
        }

        public async Task<Professional?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Professionals
                .Include(p => p.User)
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
            SortField? sortBy,
            string sortDirection,
            int? institutionId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Professionals
                .Include(p => p.User)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(searchLower) ||
                    p.LastName.ToLower().Contains(searchLower) ||
                    (p.DocumentNumber != null && p.DocumentNumber.Contains(search)) ||
                    (p.Phone != null && p.Phone.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(specialty))
            {
                var specialtyLower = specialty.ToLower();
                query = query.Where(p => p.Specialty != null && p.Specialty.ToLower().Contains(specialtyLower));
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.User.IsActive == isActive.Value);
            }

            if (institutionId.HasValue)
            {
                var professionalIdsInInstitution = _context.ProfessionalInstitutions
                    .Where(pi => pi.InstitutionId == institutionId.Value && pi.IsActive)
                    .Select(pi => pi.ProfessionalId)
                    .Distinct();

                query = query.Where(p => professionalIdsInInstitution.Contains(p.Id));
            }

            var sortMappings = new Dictionary<SortField, Expression<Func<Professional, object>>>
            {
                [SortField.Id] = p => p.Id,
                [SortField.FirstName] = p => p.FirstName,
                [SortField.LastName] = p => p.LastName,
                [SortField.CreatedAt] = p => p.CreatedAt
            };

            return await query.ToPagedAsync(
                page, pageSize,
                sortBy, sortDirection,
                sortMappings,
                cancellationToken);
        }
    }
}
