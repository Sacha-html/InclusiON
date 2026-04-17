using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

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
    }
}
