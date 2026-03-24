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
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<FamilyRepresentative?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.FamilyRepresentatives
                .Include(f => f.User)
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
            CancellationToken cancellationToken = default)
        {
            var query = _context.FamilyRepresentatives
                .Include(f => f.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(f =>
                    f.FirstName.ToLower().Contains(searchLower) ||
                    f.LastName.ToLower().Contains(searchLower) ||
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
    }
}
