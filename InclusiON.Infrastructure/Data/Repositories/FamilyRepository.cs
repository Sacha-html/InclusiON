using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly AppDbContext _context;

        public FamilyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FamilyRepresentative?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.FamilyRepresentatives
                .Include(f => f.User)
                .Include(f => f.PersonRepresentatives)
                    .ThenInclude(pr => pr.Person)
                        .ThenInclude(p => p.DisabilityType)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<FamilyRepresentative?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.FamilyRepresentatives
                .Include(f => f.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.FamilyRepresentatives
                .Where(f => f.DocumentNumber == documentNumber);

            if (excludeId.HasValue)
            {
                query = query.Where(f => f.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<FamilyRepresentative> CreateAsync(FamilyRepresentative representative, CancellationToken cancellationToken = default)
        {
            await _context.FamilyRepresentatives.AddAsync(representative, cancellationToken);
            return representative;
        }

        public Task UpdateAsync(FamilyRepresentative representative, CancellationToken cancellationToken = default)
        {
            _context.FamilyRepresentatives.Update(representative);
            return Task.CompletedTask;
        }

        public async Task<PagedResponse<FamilyRepresentative>> GetPagedAsync(
            int page, int pageSize, string? search, bool? isActive,
            SortField? sortBy, string sortDirection,
            List<int>? institutionIds = null,
            string? linkedPersonSearch = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.FamilyRepresentatives
                .Include(f => f.User)
                .Include(f => f.PersonRepresentatives.Where(pr => pr.IsActive))
                    .ThenInclude(pr => pr.Person)
                        .ThenInclude(p => p.DisabilityType)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(f =>
                    f.FirstName.Contains(searchLower) ||
                    f.LastName.Contains(searchLower) ||
                    (f.DocumentNumber != null && f.DocumentNumber.Contains(search)) ||
                    (f.Phone != null && f.Phone.Contains(search)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(f => f.User.IsActive == isActive.Value);
            }

            if (institutionIds is not null && institutionIds.Count > 0)
            {
                var representativeIdsInInstitution = _context.ProfessionalInstitutions
                    .Where(pi => institutionIds.Contains(pi.InstitutionId) && pi.IsActive)
                    .Join(_context.ProfessionalPersons.Where(pp => pp.IsActive),
                        pi => pi.ProfessionalId,
                        pp => pp.ProfessionalId,
                        (pi, pp) => pp.PersonId)
                    .Join(_context.PersonRepresentatives.Where(pr => pr.IsActive),
                        personId => personId,
                        pr => pr.PersonId,
                        (personId, pr) => pr.RepresentativeId)
                    .Distinct();

                query = query.Where(f => representativeIdsInInstitution.Contains(f.Id));
            }

            if (!string.IsNullOrWhiteSpace(linkedPersonSearch))
            {
                var linkedSearchLower = linkedPersonSearch.ToLower();
                query = query.Where(f =>
                    f.PersonRepresentatives.Any(pr => pr.IsActive &&
                        (pr.Person.FirstName.Contains(linkedSearchLower) ||
                         pr.Person.LastName.Contains(linkedSearchLower) ||
                         (pr.Person.DocumentNumber != null && pr.Person.DocumentNumber.Contains(linkedPersonSearch)))));
            }

            var sortMappings = new Dictionary<SortField, Expression<Func<FamilyRepresentative, object>>>
            {
                [SortField.Id] = f => f.Id,
                [SortField.FirstName] = f => f.FirstName,
                [SortField.LastName] = f => f.LastName,
                [SortField.CreatedAt] = f => f.CreatedAt
            };

            return await query.ToPagedAsync(
                page, pageSize,
                sortBy, sortDirection,
                sortMappings,
                cancellationToken);
        }

        public async Task<List<(FamilyRepresentative Family, bool WasPreviouslyLinked)>> GetAvailableFamiliesAsync(string? search = null, Guid? personId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.FamilyRepresentatives
                .Include(f => f.User)
                .Include(f => f.PersonRepresentatives)
                    .ThenInclude(pr => pr.Person)
                        .ThenInclude(p => p.DisabilityType)
                .AsNoTracking()
                .Where(f => f.User.IsActive && f.Status == Domain.Enums.FamilyStatusEnum.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(f =>
                    (f.FirstName + " " + f.LastName).Contains(searchLower) ||
                    f.FirstName.Contains(searchLower) ||
                    f.LastName.Contains(searchLower));
            }

            var families = await query.OrderBy(f => f.FirstName).ThenBy(f => f.LastName).ToListAsync(cancellationToken);

            if (personId.HasValue)
            {
                var existingLinks = await _context.PersonRepresentatives
                    .Where(pr => pr.PersonId == personId.Value)
                    .ToListAsync(cancellationToken);

                var alreadyLinkedIds = existingLinks
                    .Where(pr => pr.IsActive)
                    .Select(pr => pr.RepresentativeId)
                    .ToHashSet();

                var previouslyLinkedIds = existingLinks
                    .Where(pr => !pr.IsActive)
                    .Select(pr => pr.RepresentativeId)
                    .ToHashSet();

                return families
                    .Where(f => !alreadyLinkedIds.Contains(f.Id))
                    .Select(f => (f, WasPreviouslyLinked: previouslyLinkedIds.Contains(f.Id)))
                    .ToList();
            }

            return families.Select(f => (f, WasPreviouslyLinked: false)).ToList();
        }

        public async Task<List<PersonRepresentative>> GetPersonRepresentativesByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRepresentatives
                .Include(pr => pr.Representative)
                    .ThenInclude(r => r.User)
                .AsNoTracking()
                .Where(pr => pr.PersonId == personId)
                .OrderByDescending(pr => pr.IsPrimary)
                .ThenBy(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PersonRepresentative>> GetPersonRepresentativesByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRepresentatives
                .Include(pr => pr.Person)
                    .ThenInclude(p => p.DisabilityType)
                .AsNoTracking()
                .Where(pr => pr.RepresentativeId == familyId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PersonRepresentative?> GetPersonRepresentativeAsync(Guid personId, Guid representativeId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRepresentatives
                .FirstOrDefaultAsync(pr => pr.PersonId == personId && pr.RepresentativeId == representativeId, cancellationToken);
        }

        public async Task CreatePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default)
        {
            await _context.PersonRepresentatives.AddAsync(personRepresentative, cancellationToken);
        }

        public Task UpdatePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default)
        {
            _context.PersonRepresentatives.Update(personRepresentative);
            return Task.CompletedTask;
        }

        public Task DeletePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default)
        {
            _context.PersonRepresentatives.Remove(personRepresentative);
            return Task.CompletedTask;
        }

        public async Task CreateFamilyStatusHistoryAsync(FamilyStatusHistory history, CancellationToken cancellationToken = default)
        {
            await _context.FamilyStatusHistories.AddAsync(history, cancellationToken);
        }

        public async Task<List<FamilyStatusHistory>> GetFamilyStatusHistoryAsync(Guid familyId, CancellationToken cancellationToken = default)
        {
            return await _context.FamilyStatusHistories
                .AsNoTracking()
                .Where(h => h.FamilyId == familyId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task CreatePersonRepresentativeHistoryAsync(PersonRepresentativeHistory history, CancellationToken cancellationToken = default)
        {
            await _context.PersonRepresentativeHistories.AddAsync(history, cancellationToken);
        }

        public async Task<List<PersonRepresentativeHistory>> GetPersonRepresentativeHistoryAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRepresentativeHistories
                .Include(h => h.Representative)
                .AsNoTracking()
                .Where(h => h.PersonId == personId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PersonRepresentativeHistory>> GetPersonRepresentativeHistoryByFamilyAsync(Guid familyId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRepresentativeHistories
                .Include(h => h.Person)
                .AsNoTracking()
                .Where(h => h.RepresentativeId == familyId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
