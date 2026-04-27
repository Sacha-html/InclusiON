using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class InvitationsRepository : IInvitationsRepository
    {
        private readonly AppDbContext _context;

        public InvitationsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invitation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Invitations
                .Include(i => i.ForPerson)
                .Include(i => i.CreatedByProfessional)
                .FirstOrDefaultAsync(i => i.Code == code, cancellationToken);
        }

        public async Task<List<Invitation>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Invitations
                .Include(i => i.ForPerson)
                .Include(i => i.CreatedByProfessional)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Invitation>> GetByProfessionalIdAsync(Guid professionalId, CancellationToken cancellationToken = default)
        {
            return await _context.Invitations
                .Include(i => i.ForPerson)
                .AsNoTracking()
                .Where(i => i.CreatedByProfessionalId == professionalId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Invitation>> GetByInstitutionIdsAsync(List<int> institutionIds, CancellationToken cancellationToken = default)
        {
            return await _context.Invitations
                .Include(i => i.ForPerson)
                .Include(i => i.CreatedByProfessional)
                .AsNoTracking()
                .Where(i => _context.ProfessionalInstitutions.Any(pi =>
                    pi.ProfessionalId == i.CreatedByProfessionalId &&
                    institutionIds.Contains(pi.InstitutionId) &&
                    pi.IsActive))
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Invitation> CreateAsync(Invitation invitation, CancellationToken cancellationToken = default)
        {
            await _context.Invitations.AddAsync(invitation, cancellationToken);
            return invitation;
        }

        public Task UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default)
        {
            _context.Invitations.Update(invitation);
            return Task.CompletedTask;
        }

        public async Task<FamilyRepresentative> CreateFamilyRepresentativeAsync(FamilyRepresentative representative, CancellationToken cancellationToken = default)
        {
            await _context.FamilyRepresentatives.AddAsync(representative, cancellationToken);
            return representative;
        }

        public async Task CreatePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default)
        {
            await _context.PersonRepresentatives.AddAsync(personRepresentative, cancellationToken);
        }
    }
}
