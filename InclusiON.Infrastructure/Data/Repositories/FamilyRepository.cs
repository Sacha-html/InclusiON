using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Infrastructure.Extensions;
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
                .AsSplitQuery()
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search}%";
                query = query.Where(f =>
                    EF.Functions.ILike(f.FirstName, pattern) ||
                    EF.Functions.ILike(f.LastName, pattern) ||
                    (f.DocumentNumber != null && EF.Functions.ILike(f.DocumentNumber, pattern)) ||
                    (f.Phone != null && EF.Functions.ILike(f.Phone, pattern)));
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
                var linkedPattern = $"%{linkedPersonSearch}%";
                query = query.Where(f =>
                    f.PersonRepresentatives.Any(pr => pr.IsActive &&
                        (EF.Functions.ILike(pr.Person.FirstName, linkedPattern) ||
                         EF.Functions.ILike(pr.Person.LastName, linkedPattern) ||
                         (pr.Person.DocumentNumber != null && EF.Functions.ILike(pr.Person.DocumentNumber, linkedPattern)))));
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

        public async Task<(List<(FamilyRepresentative Family, bool WasPreviouslyLinked)> Items, int Total)> GetAvailableFamiliesAsync(
            string? search = null, Guid? personId = null,
            int page = 1, int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = _context.FamilyRepresentatives
                .Include(f => f.User)
                .Include(f => f.PersonRepresentatives)
                    .ThenInclude(pr => pr.Person)
                        .ThenInclude(p => p.DisabilityType)
                .AsSplitQuery()
                .AsNoTracking()
                .Where(f => f.User.IsActive && f.Status == Domain.Enums.FamilyStatusEnum.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search}%";
                query = query.Where(f =>
                    EF.Functions.ILike(f.FirstName, pattern) ||
                    EF.Functions.ILike(f.LastName, pattern) ||
                    EF.Functions.ILike(f.FirstName + " " + f.LastName, pattern));
            }

            // Exclude families already actively linked to the person (DB-side)
            if (personId.HasValue)
            {
                query = query.Where(f =>
                    !_context.PersonRepresentatives.Any(pr =>
                        pr.PersonId == personId.Value &&
                        pr.RepresentativeId == f.Id &&
                        pr.IsActive));
            }

            var orderedQuery = query.OrderBy(f => f.FirstName).ThenBy(f => f.LastName);

            var total   = await query.CountAsync(cancellationToken);
            var families = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            if (personId.HasValue && families.Count > 0)
            {
                // Load inactive link states for this page only (to compute WasPreviouslyLinked)
                var familyIds = families.Select(f => f.Id).ToList();
                var previouslyLinkedIds = await _context.PersonRepresentatives
                    .Where(pr => pr.PersonId == personId.Value
                              && !pr.IsActive
                              && familyIds.Contains(pr.RepresentativeId))
                    .Select(pr => pr.RepresentativeId)
                    .ToHashSetAsync(cancellationToken);

                return (families
                    .Select(f => (f, WasPreviouslyLinked: previouslyLinkedIds.Contains(f.Id)))
                    .ToList(), total);
            }

            return (families.Select(f => (f, WasPreviouslyLinked: false)).ToList(), total);
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

        public async Task<List<PersonWithDisability>> GetLinkedPersonsAsync(
            Guid familyUserId, CancellationToken cancellationToken = default)
        {
            return await (
                from fam in _context.FamilyRepresentatives
                join pr  in _context.PersonRepresentatives on fam.Id       equals pr.RepresentativeId
                join p   in _context.PersonsWithDisability  on pr.PersonId  equals p.Id
                where fam.UserId == familyUserId && pr.IsActive && p.IsActive
                select p
            ).AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}
