using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class AssignmentsRepository : IAssignmentsRepository
    {
        private readonly AppDbContext _context;

        public AssignmentsRepository(AppDbContext context)
        {
            _context = context;
        }

        // Professional-Person

        public async Task<List<ProfessionalPerson>> GetPersonsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default)
        {
            return await _context.ProfessionalPersons
                .Include(pp => pp.Person)
                .Where(pp => pp.ProfessionalId == professionalId)
                .OrderByDescending(pp => pp.AssignedAt)
                .ToListAsync(ct);
        }

        public async Task<ProfessionalPerson?> GetAssignmentAsync(Guid professionalId, Guid personId, CancellationToken ct = default)
        {
            return await _context.ProfessionalPersons
                .Include(pp => pp.Person)
                .FirstOrDefaultAsync(pp => pp.ProfessionalId == professionalId && pp.PersonId == personId, ct);
        }

        public async Task<ProfessionalPerson> CreateAssignmentAsync(ProfessionalPerson assignment, CancellationToken ct = default)
        {
            await _context.ProfessionalPersons.AddAsync(assignment, ct);
            return assignment;
        }

        // Professional-Institution

        public async Task<List<ProfessionalInstitution>> GetInstitutionsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default)
        {
            return await _context.ProfessionalInstitutions
                .Include(pi => pi.Institution)
                .Where(pi => pi.ProfessionalId == professionalId)
                .OrderByDescending(pi => pi.AssignedAt)
                .ToListAsync(ct);
        }

        public async Task<ProfessionalInstitution?> GetInstitutionAssignmentAsync(Guid professionalId, int institutionId, CancellationToken ct = default)
        {
            return await _context.ProfessionalInstitutions
                .Include(pi => pi.Institution)
                .FirstOrDefaultAsync(pi => pi.ProfessionalId == professionalId && pi.InstitutionId == institutionId, ct);
        }

        public async Task<ProfessionalInstitution> CreateInstitutionAssignmentAsync(ProfessionalInstitution assignment, CancellationToken ct = default)
        {
            await _context.ProfessionalInstitutions.AddAsync(assignment, ct);
            return assignment;
        }
    }
}
