using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Infrastructure.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly AppDbContext _context;

        public PersonsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PersonWithDisability?> GetByIdAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonsWithDisability
                .Include(p => p.User)
                .Include(p => p.DisabilityType)
                .Include(p => p.AutonomyLevel)
                .Include(p => p.LoginMethod)
                .Include(p => p.SupervisorUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == personId, cancellationToken);
        }

        public async Task<PersonWithDisability?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonsWithDisability
                .Include(p => p.User)
                .Include(p => p.DisabilityType)
                .Include(p => p.AutonomyLevel)
                .Include(p => p.LoginMethod)
                .Include(p => p.SupervisorUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludePersonId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.PersonsWithDisability
                .Where(p => p.DocumentNumber == documentNumber);

            if (excludePersonId.HasValue)
            {
                query = query.Where(p => p.Id != excludePersonId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<PersonWithDisability> CreateAsync(PersonWithDisability person, CancellationToken cancellationToken = default)
        {
            await _context.PersonsWithDisability.AddAsync(person, cancellationToken);

            return person;
        }

        public Task UpdateAsync(PersonWithDisability person, CancellationToken cancellationToken = default)
        {
            _context.PersonsWithDisability.Update(person);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<Professional>> GetSupervisingProfessionalsAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.ProfessionalPersons
                .Include(pp => pp.Professional)
                    .ThenInclude(p => p.User)
                .Where(pp => pp.PersonId == personId && pp.IsActive && pp.CanSuperviseLogin)
                .Select(pp => pp.Professional)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<PersonRepresentative>> GetActiveRepresentativesAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRepresentatives
                .Include(pr => pr.Representative)
                    .ThenInclude(r => r.User)
                .Where(pr => pr.PersonId == personId && pr.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<PersonSkillProfile>> GetSkillProfileAsync(
            Guid personId,
            bool activeOnly,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PersonSkillProfiles
                .Include(psp => psp.SkillArea)
                .Where(psp => psp.PersonId == personId);

            if (activeOnly)
                query = query.Where(psp => psp.IsActive);

            return await query
                .OrderBy(psp => psp.SkillArea.DisplayOrder)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<PersonSkillProfile?> GetSkillProfileEntryAsync(
            Guid personId,
            int skillAreaId,
            CancellationToken cancellationToken = default)
        {
            return await _context.PersonSkillProfiles
                .Include(psp => psp.SkillArea)
                .FirstOrDefaultAsync(
                    psp => psp.PersonId == personId && psp.SkillAreaId == skillAreaId,
                    cancellationToken);
        }

        public async Task AddSkillProfileEntryAsync(
            PersonSkillProfile entry,
            CancellationToken cancellationToken = default)
        {
            await _context.PersonSkillProfiles.AddAsync(entry, cancellationToken);
        }

        public async Task<PagedResponse<PersonWithDisability>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            int? disabilityTypeId,
            int? autonomyLevelId,
            bool? isActive,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            string? representativeSearch = null,
            IReadOnlyList<Guid>? accessiblePersonIds = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PersonsWithDisability
                .Include(p => p.User)
                .Include(p => p.DisabilityType)
                .Include(p => p.AutonomyLevel)
                .Include(p => p.LoginMethod)
                .Include(p => p.PersonRepresentatives.Where(pr => pr.IsActive))
                    .ThenInclude(pr => pr.Representative)
                .AsSplitQuery()
                .AsNoTracking()
                .AsQueryable();

            // Filtros (ILike para búsqueda case-insensitive en PostgreSQL)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search}%";
                query = query.Where(p =>
                    EF.Functions.ILike(p.FirstName, pattern) ||
                    EF.Functions.ILike(p.LastName, pattern) ||
                    (p.DocumentNumber != null && EF.Functions.ILike(p.DocumentNumber, pattern)));
            }

            if (disabilityTypeId.HasValue)
            {
                query = query.Where(p => p.DisabilityTypeId == disabilityTypeId.Value);
            }

            if (autonomyLevelId.HasValue)
            {
                query = query.Where(p => p.AutonomyLevelId == autonomyLevelId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.User.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(representativeSearch))
            {
                var repPattern = $"%{representativeSearch}%";
                query = query.Where(p => p.PersonRepresentatives.Any(pr =>
                    pr.IsActive &&
                    (EF.Functions.ILike(pr.Representative.FirstName, repPattern) ||
                     EF.Functions.ILike(pr.Representative.LastName, repPattern))));
            }

            if (accessiblePersonIds is not null)
            {
                query = query.Where(p => accessiblePersonIds.Contains(p.Id));
            }

            if (institutionIds is not null && institutionIds.Count > 0)
            {
                var personIdsInInstitution = _context.ProfessionalInstitutions
                    .Where(pi => institutionIds.Contains(pi.InstitutionId) && pi.IsActive)
                    .Join(_context.ProfessionalPersons.Where(pp => pp.IsActive),
                        pi => pi.ProfessionalId,
                        pp => pp.ProfessionalId,
                        (pi, pp) => pp.PersonId)
                    .Distinct();

                query = query.Where(p => personIdsInInstitution.Contains(p.Id));
            }

            var sortMappings = new Dictionary<SortField, Expression<Func<PersonWithDisability, object>>>
            {
                [SortField.Id] = p => p.Id,
                [SortField.FirstName] = p => p.FirstName,
                [SortField.LastName] = p => p.LastName,
                [SortField.BirthDate] = p => p.BirthDate,
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
