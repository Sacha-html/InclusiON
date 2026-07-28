using Microsoft.EntityFrameworkCore;
using InclusiON.Infrastructure.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class InstitutionsRepository : IInstitutionsRepository
    {
        private readonly AppDbContext _context;

        public InstitutionsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EducationalInstitution>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.EducationalInstitutions
                .AsNoTracking()
                .OrderBy(i => i.Name)
                .ToListAsync(ct);
        }

        public async Task<PagedResponse<EducationalInstitution>> GetPagedAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken ct = default)
        {
            var query = _context.EducationalInstitutions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search}%";
                query = query.Where(i =>
                    EF.Functions.ILike(i.Name, pattern) ||
                    (i.Address != null && EF.Functions.ILike(i.Address, pattern)));
            }

            if (isActive.HasValue)
                query = query.Where(i => i.IsActive == isActive.Value);

            return await query.OrderBy(i => i.Name).ToPagedAsync(page, pageSize, ct);
        }

        public async Task<EducationalInstitution?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.EducationalInstitutions
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id, ct);
        }

        public async Task<EducationalInstitution> CreateAsync(EducationalInstitution institution, CancellationToken ct = default)
        {
            await _context.EducationalInstitutions.AddAsync(institution, ct);
            return institution;
        }

        public Task UpdateAsync(EducationalInstitution institution, CancellationToken ct = default)
        {
            _context.EducationalInstitutions.Update(institution);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var query = _context.EducationalInstitutions
                .Where(i => EF.Functions.ILike(i.Name, name));

            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }

            return await query.AnyAsync(ct);
        }

        public async Task<bool> HasActiveProfessionalsAsync(int institutionId, CancellationToken ct = default)
        {
            return await _context.ProfessionalInstitutions
                .AnyAsync(pi => pi.InstitutionId == institutionId && pi.IsActive, ct);
        }
    }
}
