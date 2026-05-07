using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class ActivityAssignmentRepository : IActivityAssignmentRepository
    {
        private readonly AppDbContext _context;

        public ActivityAssignmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityAssignment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.ActivityAssignments
                .Include(a => a.Status)
                .Include(a => a.Activity)
                    .ThenInclude(a => a.Content)
                        .ThenInclude(c => c!.TemplateType)
                .Include(a => a.Responses.OrderByDescending(r => r.StartedAt))
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<List<ActivityAssignment>> GetByPersonIdAsync(Guid personId, CancellationToken ct = default)
        {
            return await _context.ActivityAssignments
                .Include(a => a.Status)
                .Include(a => a.Activity)
                    .ThenInclude(a => a.Content)
                        .ThenInclude(c => c!.TemplateType)
                .Include(a => a.Responses.OrderByDescending(r => r.StartedAt))
                .AsNoTracking()
                .Where(a => a.PersonId == personId)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync(ct);
        }

        public async Task<ActivityAssignment> CreateAsync(ActivityAssignment assignment, CancellationToken ct = default)
        {
            _context.ActivityAssignments.Add(assignment);
            return assignment;
        }

        public async Task UpdateAsync(ActivityAssignment assignment, CancellationToken ct = default)
        {
            _context.Entry(assignment).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        public async Task<ActivityResponse?> GetResponseByIdAsync(int responseId, CancellationToken ct = default)
        {
            return await _context.ActivityResponses
                .FirstOrDefaultAsync(r => r.Id == responseId, ct);
        }

        public async Task<ActivityResponse> CreateResponseAsync(ActivityResponse response, CancellationToken ct = default)
        {
            _context.ActivityResponses.Add(response);
            return response;
        }

        public async Task UpdateResponseAsync(ActivityResponse response, CancellationToken ct = default)
        {
            _context.ActivityResponses.Update(response);
        }

        public async Task<int> CountResponsesAsync(int assignmentId, CancellationToken ct = default)
        {
            return await _context.ActivityResponses
                .CountAsync(r => r.AssignmentId == assignmentId, ct);
        }

        public async Task<List<ActivityResponse>> GetRecentCompletedResponsesAsync(
            Guid personId, int limit, CancellationToken ct = default)
        {
            return await _context.ActivityResponses
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Activity)
                .AsNoTracking()
                .Where(r => r.Assignment.PersonId == personId && r.CompletedAt != null)
                .OrderByDescending(r => r.CompletedAt)
                .Take(limit)
                .ToListAsync(ct);
        }
    }
}
