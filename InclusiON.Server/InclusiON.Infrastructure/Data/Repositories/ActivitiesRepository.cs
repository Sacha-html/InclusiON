using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Infrastructure.Extensions;
using InclusiON.Data;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class ActivitiesRepository : IActivitiesRepository
    {
        private readonly AppDbContext _context;

        public ActivitiesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Activity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Activities
                .Include(a => a.Category)
                .Include(a => a.SkillArea)
                .Include(a => a.Content)
                    .ThenInclude(c => c!.TemplateType)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<(List<Activity> Items, int Total)> GetPagedAsync(
            Guid professionalId,
            string? search,
            int? categoryId,
            int? skillAreaId,
            int? templateTypeId,
            bool? isActive,
            bool? isStandard,
            bool? isTemplate,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Activities
                .Include(a => a.Category)
                .Include(a => a.SkillArea)
                .Include(a => a.Professional)
                .Include(a => a.Content)
                    .ThenInclude(c => c!.TemplateType)
                .AsNoTracking();

            if (isTemplate.HasValue && isTemplate.Value)
            {
                query = query.Where(a => a.IsTemplate);
            }
            else
            {
                // Aislamiento estricto: solo actividades del profesional que no sean plantillas
                query = query.Where(a => a.ProfessionalId == professionalId && !a.IsTemplate);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a =>
                    a.Title.Contains(search) ||
                    (a.Description != null && a.Description.Contains(search)));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            if (skillAreaId.HasValue)
                query = query.Where(a => a.SkillAreaId == skillAreaId.Value);

            if (templateTypeId.HasValue)
                query = query.Where(a => a.Content != null && a.Content.TemplateTypeId == templateTypeId.Value);

            if (isActive.HasValue)
                query = query.Where(a => a.IsActive == isActive.Value);

            if (isStandard.HasValue)
                query = query.Where(a => a.IsStandardActivity == isStandard.Value);

            var paged = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToPagedAsync(page, pageSize, cancellationToken);

            return (paged.Data, paged.TotalRecords);
        }

        public async Task<Activity> CreateAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            _context.Activities.Add(activity);
            return activity;
        }

        public async Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            _context.Activities.Update(activity);
        }

        public async Task<bool> HasActiveAssignmentsAsync(int activityId, CancellationToken cancellationToken = default)
        {
            return await _context.ActivityAssignments
                .AnyAsync(a =>
                    a.ActivityId == activityId &&
                    a.StatusId != AssignmentStatuses.Completada &&
                    a.StatusId != AssignmentStatuses.Cancelada,
                    cancellationToken);
        }

        public async Task<List<Activity>> GetByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return [];

            var activities = await _context.Activities
                .Include(a => a.Category)
                .Include(a => a.SkillArea)
                .Include(a => a.Content)
                    .ThenInclude(c => c!.TemplateType)
                .Where(a => idList.Contains(a.Id))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Preservar el orden de similitud devuelto por pgvector
            return [.. idList
                .Select(id => activities.FirstOrDefault(a => a.Id == id))
                .Where(a => a is not null)
                .Select(a => a!)];
        }

        public async Task<List<ActivityEmbeddingProjection>> GetAllActiveForEmbeddingAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Activities
                .Where(a => a.IsActive)
                .Select(a => new ActivityEmbeddingProjection(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.Instructions,
                    a.Content != null ? a.Content.ContentJson : null))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ActivityEmbeddingProjection>> GetStandardActivitiesForEmbeddingAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Activities
                .Where(a => a.IsActive && a.IsStandardActivity)
                .Select(a => new ActivityEmbeddingProjection(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.Instructions,
                    a.Content != null ? a.Content.ContentJson : null))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Activity>> GetRoadmapTemplatesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Activities
                .Include(a => a.Category)
                .Include(a => a.SkillArea)
                .Include(a => a.Professional)
                .Include(a => a.Content)
                    .ThenInclude(c => c!.TemplateType)
                .Where(a => a.IsTemplate && a.RoadmapOrder != null)
                .OrderBy(a => a.RoadmapOrder)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
