using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Entities.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PersonsRepository(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<PersonWithDisability> CreateAsync(PersonWithDisability person, User user, string password, CancellationToken cancellationToken = default)
        {
            // Crear usuario con Identity
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al crear usuario: {errors}");
            }

            // Asignar rol Person
            await _userManager.AddToRoleAsync(user, "Person");

            // Asignar UserId a la persona
            person.UserId = user.Id;

            // Crear persona
            await _context.PersonsWithDisability.AddAsync(person, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return person;
        }

        public async Task UpdateAsync(PersonWithDisability person, CancellationToken cancellationToken = default)
        {
            _context.PersonsWithDisability.Update(person);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<(List<PersonWithDisability> Items, int TotalCount)> GetPagedAsync(
            int skip,
            int take,
            string? search,
            int? disabilityTypeId,
            int? autonomyLevelId,
            bool? isActive,
            string? sortBy,
            string sortDirection,
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

            // Contar total
            var totalCount = await query.CountAsync(cancellationToken);

            // Ordenamiento
            query = sortBy?.ToLower() switch
            {
                "firstname" => sortDirection == "ASC"
                    ? query.OrderBy(p => p.FirstName)
                    : query.OrderByDescending(p => p.FirstName),
                "lastname" => sortDirection == "ASC"
                    ? query.OrderBy(p => p.LastName)
                    : query.OrderByDescending(p => p.LastName),
                "birthdate" => sortDirection == "ASC"
                    ? query.OrderBy(p => p.BirthDate)
                    : query.OrderByDescending(p => p.BirthDate),
                "createdat" => sortDirection == "ASC"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Paginacion
            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
