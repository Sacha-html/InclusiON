using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.Domain.Enums;

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
                .Include(a => a.Person)
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

        public async Task<Dictionary<Guid, List<ActivityResponse>>> GetRecentCompletedResponsesByPersonIdsAsync(
            IEnumerable<Guid> personIds, int limit, CancellationToken ct = default)
        {
            var idList = personIds.ToList();
            if (idList.Count == 0) return new();

            var responses = await _context.ActivityResponses
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Activity)
                .AsNoTracking()
                .Where(r => idList.Contains(r.Assignment.PersonId) && r.CompletedAt != null)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync(ct);

            return responses
                .GroupBy(r => r.Assignment.PersonId)
                .ToDictionary(g => g.Key, g => g.Take(limit).ToList());
        }

        public async Task<bool> HasActiveAssignmentAsync(Guid personId, int activityId, CancellationToken ct = default)
        {
            return await _context.ActivityAssignments
                .AnyAsync(aa => aa.PersonId == personId &&
                                aa.ActivityId == activityId &&
                                aa.StatusId != AssignmentStatuses.Completada &&
                                aa.StatusId != AssignmentStatuses.Cancelada,
                          ct);
        }

        public async Task CreateSessionAsync(ActivitySession session, CancellationToken ct = default)
        {
            await _context.ActivitySessions.AddAsync(session, ct);
        }

        public async Task<Dictionary<Guid, PersonRoadmapProgress>> GetRoadmapProgressByPersonIdsAsync(
            IEnumerable<Guid> personIds, CancellationToken ct = default)
        {
            var idList = personIds.ToList();
            if (idList.Count == 0) return new();

            // 1. Catálogo oficial de las 10 actividades del Roadmap ordenadas por nivel
            var roadmapActivities = await _context.Activities
                .AsNoTracking()
                .Where(a => a.IsActive && a.RoadmapOrder != null)
                .OrderBy(a => a.RoadmapOrder)
                .ToListAsync(ct);

            var roadmapMap = roadmapActivities.ToDictionary(a => a.RoadmapOrder!.Value, a => a.Title);

            // 2. Asignaciones de los alumnos que pertenezcan al Roadmap
            var assignments = await _context.ActivityAssignments
                .Include(a => a.Activity)
                .AsNoTracking()
                .Where(a => idList.Contains(a.PersonId) && a.Activity.RoadmapOrder != null)
                .ToListAsync(ct);

            // 3. Sesiones analíticas de los alumnos
            var sessions = await _context.ActivitySessions
                .AsNoTracking()
                .Where(s => idList.Contains(s.StudentId) && s.IsActive)
                .ToListAsync(ct);

            // 4. Respuestas registradas para verificar las 4 fallas consecutivas
            var responses = await _context.ActivityResponses
                .Include(r => r.Assignment)
                .AsNoTracking()
                .Where(r => idList.Contains(r.Assignment.PersonId))
                .OrderByDescending(r => r.StartedAt)
                .ToListAsync(ct);

            var result = new Dictionary<Guid, PersonRoadmapProgress>();

            foreach (var pid in idList)
            {
                var personAssignments = assignments.Where(a => a.PersonId == pid).ToList();
                var personSessions = sessions.Where(s => s.StudentId == pid).ToList();
                var personResponses = responses.Where(r => r.Assignment.PersonId == pid).ToList();

                // Nivel actual: El nivel más alto que el alumno haya iniciado
                int currentLevel = 1;
                if (personAssignments.Count > 0)
                {
                    currentLevel = personAssignments.Max(a => a.Activity.RoadmapOrder ?? 1);
                }
                else if (personSessions.Count > 0)
                {
                    var maxSessionLevel = personSessions
                        .Select(s => roadmapActivities.FirstOrDefault(ra => ra.Id == s.ActivityId)?.RoadmapOrder ?? 1)
                        .DefaultIfEmpty(1)
                        .Max();
                    currentLevel = Math.Max(1, maxSessionLevel);
                }

                if (currentLevel < 1) currentLevel = 1;
                if (currentLevel > 10) currentLevel = 10;

                string levelName = roadmapMap.TryGetValue(currentLevel, out var title)
                    ? title
                    : $"Nivel {currentLevel}";

                decimal avgSuccess = 0m;
                string gasLabel;

                if (personSessions.Count > 0)
                {
                    avgSuccess = Math.Round(personSessions.Average(s => s.SuccessRate), 1);
                    var avgGas = (int)Math.Round(personSessions.Average(s => (decimal)s.GasScore), MidpointRounding.AwayFromZero);

                    if (avgGas >= 1)
                        gasLabel = "Superando objetivos con autonomía";
                    else if (avgGas == 0)
                        gasLabel = "En ritmo de progreso esperado";
                    else
                        gasLabel = "En desarrollo, requiere apoyos en casa";
                }
                else
                {
                    gasLabel = "Iniciando camino";
                }

                // Alerta de frustración (HU-21): Se activa exclusivamente si registra 4 fallas consecutivas en la misma actividad
                bool hasFrustrationAlert = false;
                var groupedByAssignment = personResponses.GroupBy(r => r.AssignmentId);
                foreach (var group in groupedByAssignment)
                {
                    var sortedAttempts = group.OrderBy(r => r.StartedAt).ToList();
                    int consecutiveFailures = 0;
                    foreach (var attempt in sortedAttempts)
                    {
                        bool isFailure = (attempt.SuccessPercentage.HasValue && attempt.SuccessPercentage.Value < 60m)
                                         || attempt.Result == ActivityResponseResult.Fallido;
                        if (isFailure)
                        {
                            consecutiveFailures++;
                            if (consecutiveFailures >= 4)
                            {
                                hasFrustrationAlert = true;
                                break;
                            }
                        }
                        else
                        {
                            consecutiveFailures = 0;
                        }
                    }
                    if (hasFrustrationAlert) break;
                }

                result[pid] = new PersonRoadmapProgress(currentLevel, levelName, gasLabel, avgSuccess, hasFrustrationAlert);
            }

            return result;
        }
    }
}
