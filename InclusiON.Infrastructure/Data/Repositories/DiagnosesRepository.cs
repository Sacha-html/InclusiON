using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class DiagnosesRepository : IDiagnosesRepository
    {
        private readonly AppDbContext _context;

        public DiagnosesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Diagnosis?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Diagnosis>()
                .Include(d => d.Professional)
                .Include(d => d.Person)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken);
        }

        public async Task<Diagnosis?> GetByIdIgnoreActiveAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Diagnosis>()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<List<Diagnosis>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Diagnosis>()
                .Include(d => d.Professional)
                .AsNoTracking()
                .Where(d => d.PersonId == personId && d.IsActive)
                .OrderByDescending(d => d.DiagnosisDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Diagnosis> CreateAsync(Diagnosis diagnosis, CancellationToken cancellationToken = default)
        {
            await _context.Set<Diagnosis>().AddAsync(diagnosis, cancellationToken);
            return diagnosis;
        }

        public Task UpdateAsync(Diagnosis diagnosis, CancellationToken cancellationToken = default)
        {
            _context.Set<Diagnosis>().Update(diagnosis);
            return Task.CompletedTask;
        }
    }
}
