using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Extensions;
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

        public async Task<PagedResponse<PersonWithDisability>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            int? disabilityTypeId,
            int? autonomyLevelId,
            bool? isActive,
            SortField? sortBy,
            string sortDirection,
            int? institutionId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PersonsWithDisability
                .Include(p => p.User)
                .Include(p => p.DisabilityType)
                .Include(p => p.AutonomyLevel)
                .Include(p => p.LoginMethod)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(searchLower) ||
                    p.LastName.ToLower().Contains(searchLower) ||
                    (p.DocumentNumber != null && p.DocumentNumber.Contains(search)));
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

            if (institutionId.HasValue)
            {
                var personIdsInInstitution = _context.ProfessionalInstitutions
                    .Where(pi => pi.InstitutionId == institutionId.Value && pi.IsActive)
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
