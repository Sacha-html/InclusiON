using Microsoft.EntityFrameworkCore;
using InclusiON.Infrastructure.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

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

        public async Task<PagedResponse<Invitation>> GetPagedAsync(int page, int pageSize, string? search = null, string? status = null, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Invitations
                .Include(i => i.ForPerson)
                .Include(i => i.CreatedByProfessional)
                .AsNoTracking();

            query = ApplySearchAndStatus(query, search, status, now);

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .ToPagedAsync(page, pageSize, cancellationToken);
        }

        public async Task<PagedResponse<Invitation>> GetPagedByProfessionalIdAsync(Guid professionalId, int page, int pageSize, string? search = null, string? status = null, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Invitations
                .Include(i => i.ForPerson)
                .AsNoTracking()
                .Where(i => i.CreatedByProfessionalId == professionalId);

            query = ApplySearchAndStatus(query, search, status, now);

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .ToPagedAsync(page, pageSize, cancellationToken);
        }

        public async Task<PagedResponse<Invitation>> GetPagedByInstitutionIdsAsync(List<int> institutionIds, int page, int pageSize, string? search = null, string? status = null, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Invitations
                .Include(i => i.ForPerson)
                .Include(i => i.CreatedByProfessional)
                .AsNoTracking()
                .Where(i => _context.ProfessionalInstitutions.Any(pi =>
                    pi.ProfessionalId == i.CreatedByProfessionalId &&
                    institutionIds.Contains(pi.InstitutionId) &&
                    pi.IsActive));

            query = ApplySearchAndStatus(query, search, status, now);

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .ToPagedAsync(page, pageSize, cancellationToken);
        }

        private static IQueryable<Invitation> ApplySearchAndStatus(IQueryable<Invitation> query, string? search, string? status, DateTime now)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(i =>
                    i.Email.ToLower().Contains(term) ||
                    (i.FirstName != null && i.FirstName.ToLower().Contains(term)) ||
                    (i.LastName  != null && i.LastName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = status switch
                {
                    "Aceptada" => query.Where(i => i.IsUsed),
                    "Expirada" => query.Where(i => !i.IsUsed && i.ExpiresAt < now),
                    "Enviada"  => query.Where(i => !i.IsUsed && i.ExpiresAt >= now),
                    _          => query,
                };
            }

            return query;
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
