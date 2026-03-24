using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;

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
    }
}
