using Microsoft.EntityFrameworkCore;
using InclusiON.Infrastructure.Extensions;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

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
                .AsNoTracking()
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
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResponse<User>> GetAllAdminsPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.NormalizedName == "ADMIN")
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var adminUserIds = _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId);

            var query = _context.Users
                .Where(u => adminUserIds.Contains(u.Id))
                .Include(u => u.AdminInstitutions.Where(ai => ai.IsActive))
                    .ThenInclude(ai => ai.Institution)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search}%";
                query = query.Where(u =>
                    (u.Name != null && EF.Functions.ILike(u.Name, pattern)) ||
                    (u.Surname != null && EF.Functions.ILike(u.Surname, pattern)) ||
                    (u.Email != null && EF.Functions.ILike(u.Email, pattern)));
            }

            return await query.OrderBy(u => u.Name).ThenBy(u => u.Surname).ToPagedAsync(page, pageSize, cancellationToken);
        }

        public async Task<List<AdminInstitution>> GetInstitutionsByAdminAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            return await _context.AdminInstitutions
                .Include(ai => ai.Institution)
                .AsNoTracking()
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
