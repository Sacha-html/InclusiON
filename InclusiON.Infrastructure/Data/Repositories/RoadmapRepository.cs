using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class RoadmapRepository : IRoadmapRepository
    {
        private readonly AppDbContext _context;

        public RoadmapRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PersonRoadmap?> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRoadmaps
                .Include(r => r.CreatedByProfessional)
                .Include(r => r.Areas)
                    .ThenInclude(a => a.SkillArea)
                .Include(r => r.Areas)
                    .ThenInclude(a => a.Activities)
                        .ThenInclude(act => act.Activity)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.PersonId == personId, cancellationToken);
        }

        public async Task<bool> ExistsForPersonAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRoadmaps
                .AnyAsync(r => r.PersonId == personId, cancellationToken);
        }

        public async Task<PersonRoadmap> CreateAsync(PersonRoadmap roadmap, CancellationToken cancellationToken = default)
        {
            await _context.PersonRoadmaps.AddAsync(roadmap, cancellationToken);
            return roadmap;
        }

        public async Task<PersonRoadmapArea?> GetAreaByIdAsync(int areaId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRoadmapAreas
                .Include(a => a.SkillArea)
                .FirstOrDefaultAsync(a => a.Id == areaId, cancellationToken);
        }

        public async Task<bool> AreaExistsInRoadmapAsync(int roadmapId, int skillAreaId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRoadmapAreas
                .AnyAsync(a => a.PersonRoadmapId == roadmapId && a.SkillAreaId == skillAreaId, cancellationToken);
        }

        public async Task AddAreaAsync(PersonRoadmapArea area, CancellationToken cancellationToken = default)
        {
            await _context.PersonRoadmapAreas.AddAsync(area, cancellationToken);
        }

        public void RemoveArea(PersonRoadmapArea area)
        {
            _context.PersonRoadmapAreas.Remove(area);
        }

        public async Task<PersonRoadmapActivity?> GetActivityByIdAsync(int activityEntryId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRoadmapActivities
                .Include(a => a.Activity)
                .FirstOrDefaultAsync(a => a.Id == activityEntryId, cancellationToken);
        }

        public async Task<bool> ActivityExistsInAreaAsync(int roadmapAreaId, int activityId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRoadmapActivities
                .AnyAsync(a => a.PersonRoadmapAreaId == roadmapAreaId && a.ActivityId == activityId, cancellationToken);
        }

        public async Task<List<PersonRoadmapActivity>> GetActivitiesByAreaIdAsync(int areaId, CancellationToken cancellationToken = default)
        {
            return await _context.PersonRoadmapActivities
                .Where(a => a.PersonRoadmapAreaId == areaId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddActivityAsync(PersonRoadmapActivity activity, CancellationToken cancellationToken = default)
        {
            await _context.PersonRoadmapActivities.AddAsync(activity, cancellationToken);
        }

        public void RemoveActivity(PersonRoadmapActivity activity)
        {
            _context.PersonRoadmapActivities.Remove(activity);
        }

        public async Task<PersonRoadmapActivity?> GetByPersonAndActivityAsync(
            Guid personId, int activityId, CancellationToken ct = default)
        {
            return await _context.PersonRoadmapActivities
                .Include(a => a.PersonRoadmapArea)
                    .ThenInclude(area => area.PersonRoadmap)
                .FirstOrDefaultAsync(a =>
                    a.ActivityId == activityId &&
                    a.PersonRoadmapArea.PersonRoadmap.PersonId == personId, ct);
        }

        public async Task<PersonRoadmapActivity?> GetNextInAreaAsync(
            int areaId, int currentSequenceOrder, CancellationToken ct = default)
        {
            return await _context.PersonRoadmapActivities
                .Where(a => a.PersonRoadmapAreaId == areaId && a.SequenceOrder > currentSequenceOrder)
                .OrderBy(a => a.SequenceOrder)
                .FirstOrDefaultAsync(ct);
        }
    }
}
