using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class AdminInstitutionRepository : IAdminInstitutionRepository
    {
        private readonly AppDbContext _context;

        public AdminInstitutionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetActiveInstitutionIdsByAdminAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            return await _context.AdminInstitutions
                .Where(ai => ai.AdminUserId == adminUserId && ai.IsActive)
                .Select(ai => ai.InstitutionId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<User>> GetAllAdminsWithInstitutionsAsync(CancellationToken cancellationToken = default)
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.NormalizedName == "ADMIN")
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);

            return await _context.Users
                .Where(u => adminUserIds.Contains(u.Id))
                .Include(u => u.AdminInstitutions.Where(ai => ai.IsActive))
                .ThenInclude(ai => ai.Institution)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AdminInstitution>> GetInstitutionsByAdminAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            return await _context.AdminInstitutions
                .Include(ai => ai.Institution)
                .Where(ai => ai.AdminUserId == adminUserId)
                .ToListAsync(cancellationToken);
        }

        public async Task<AdminInstitution?> FindAssignmentAsync(Guid adminUserId, int institutionId, CancellationToken cancellationToken = default)
        {
            return await _context.AdminInstitutions
                .Include(ai => ai.Institution)
                .FirstOrDefaultAsync(ai => ai.AdminUserId == adminUserId && ai.InstitutionId == institutionId, cancellationToken);
        }

        public async Task AddAsync(AdminInstitution entity, CancellationToken cancellationToken = default)
        {
            await _context.AdminInstitutions.AddAsync(entity, cancellationToken);
        }

        public void Remove(AdminInstitution entity)
        {
            _context.AdminInstitutions.Remove(entity);
        }
    }
}
