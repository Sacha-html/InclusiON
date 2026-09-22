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

            // 1. PersonRoadmaps de los alumnos con sus áreas y actividades configuradas
            var personRoadmaps = await _context.PersonRoadmaps
                .Include(r => r.Areas)
                    .ThenInclude(a => a.Activities)
                        .ThenInclude(pa => pa.Activity)
                .AsNoTracking()
                .Where(r => idList.Contains(r.PersonId))
                .ToListAsync(ct);

            // 2. Catálogo oficial de actividades del Roadmap como fallback
            var officialRoadmapActivities = await _context.Activities
                .AsNoTracking()
                .Where(a => a.IsActive && a.RoadmapOrder != null)
                .OrderBy(a => a.RoadmapOrder)
                .ToListAsync(ct);

            var officialRoadmapMap = officialRoadmapActivities.ToDictionary(a => a.RoadmapOrder!.Value, a => a);

            // 3. Asignaciones de los alumnos con sus respuestas
            var assignments = await _context.ActivityAssignments
                .Include(a => a.Activity)
                .Include(a => a.Responses)
                .AsNoTracking()
                .Where(a => idList.Contains(a.PersonId))
                .ToListAsync(ct);

            // 4. Sesiones analíticas de los alumnos
            var sessions = await _context.ActivitySessions
                .AsNoTracking()
                .Where(s => idList.Contains(s.StudentId) && s.IsActive)
                .ToListAsync(ct);

            var result = new Dictionary<Guid, PersonRoadmapProgress>();

            foreach (var pid in idList)
            {
                var personAssignments = assignments.Where(a => a.PersonId == pid).ToList();
                var personSessions = sessions.Where(s => s.StudentId == pid).ToList();
                var roadmap = personRoadmaps.FirstOrDefault(r => r.PersonId == pid);

                // Actividades del roadmap personalizado de la persona ordenadas por área y secuencia
                var studentRoadmapActivities = roadmap?.Areas
                    .OrderBy(a => a.DisplayOrder)
                    .SelectMany(a => a.Activities.OrderBy(act => act.SequenceOrder))
                    .ToList() ?? [];

                var levelSteps = new List<RoadmapLevelStep>();
                bool hasFrustrationAlert = false;

                for (int lvl = 1; lvl <= 10; lvl++)
                {
                    var customActivity = studentRoadmapActivities.FirstOrDefault(a => a.SequenceOrder == lvl)
                        ?? (studentRoadmapActivities.Count >= lvl ? studentRoadmapActivities[lvl - 1] : null);
                    officialRoadmapMap.TryGetValue(lvl, out var officialActivity);

                    int? activityId = customActivity?.ActivityId ?? officialActivity?.Id;
                    string title = customActivity?.Activity?.Title ?? officialActivity?.Title ?? $"Nivel {lvl}";
                    bool isUnlocked = customActivity?.IsUnlocked ?? (lvl == 1);

                    // Buscar asignaciones asociadas a este nivel / actividad
                    var matchedAssignments = activityId.HasValue
                        ? personAssignments.Where(a => a.ActivityId == activityId.Value).ToList()
                        : [];

                    // Verificar si fue aprobada / completada (>= 60%) en asignaciones o sesiones
                    bool isCompleted = matchedAssignments.Any(a =>
                        a.StatusId == AssignmentStatuses.Completada ||
                        (a.Responses != null && a.Responses.Any(r => r.CompletedAt.HasValue && (r.SuccessPercentage ?? 0) >= 60m)))
                        || personSessions.Any(s => activityId.HasValue && s.ActivityId == activityId.Value && s.SuccessRate >= 60m);

                    // Verificar si está trabado (alerta de frustración: >= 4 fallas consecutivas en la actividad)
                    bool isStruggling = false;
                    if (!isCompleted && matchedAssignments.Count > 0)
                    {
                        foreach (var asn in matchedAssignments)
                        {
                            var sortedAttempts = (asn.Responses ?? [])
                                .Where(r => r.CompletedAt.HasValue)
                                .OrderBy(r => r.StartedAt)
                                .ToList();

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
                                        isStruggling = true;
                                        hasFrustrationAlert = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    consecutiveFailures = 0;
                                }
                            }
                            if (isStruggling) break;
                        }
                    }

                    string status;
                    if (isCompleted)
                    {
                        status = "completed"; // Verde ✓
                    }
                    else if (isStruggling)
                    {
                        status = "struggling"; // Naranja (>= 4 fallas)
                    }
                    else if (isUnlocked || matchedAssignments.Any(a => a.StatusId == AssignmentStatuses.EnProgreso || a.StatusId == AssignmentStatuses.Pendiente))
                    {
                        status = "active"; // Azul
                    }
                    else
                    {
                        status = "locked"; // Gris 🔒
                    }

                    levelSteps.Add(new RoadmapLevelStep(lvl, title, status));
                }

                // Nivel actual: el primer nivel no completado que esté activo o trabado, o el siguiente tras el último completado
                int currentLevel = 1;
                var currentStep = levelSteps.FirstOrDefault(s => s.Status == "struggling" || s.Status == "active");
                if (currentStep != null)
                {
                    currentLevel = currentStep.Level;
                }
                else
                {
                    var lastCompleted = levelSteps.LastOrDefault(s => s.Status == "completed");
                    currentLevel = lastCompleted != null ? Math.Min(10, lastCompleted.Level + 1) : 1;
                }

                // Garantizar que el nivel actual (si no está completado) esté activo (azul) o trabado (naranja), nunca bloqueado
                var activeStepIndex = levelSteps.FindIndex(s => s.Level == currentLevel);
                if (activeStepIndex >= 0 && levelSteps[activeStepIndex].Status != "completed")
                {
                    string resolvedStatus = levelSteps[activeStepIndex].Status == "struggling" ? "struggling" : "active";
                    levelSteps[activeStepIndex] = new RoadmapLevelStep(
                        currentLevel,
                        levelSteps[activeStepIndex].Title,
                        resolvedStatus);
                }

                // Garantizar consistencia: niveles previos completados y niveles posteriores no completados bloqueados
                for (int i = 0; i < levelSteps.Count; i++)
                {
                    var s = levelSteps[i];
                    if (s.Level < currentLevel && s.Status != "completed")
                    {
                        levelSteps[i] = new RoadmapLevelStep(s.Level, s.Title, "completed");
                    }
                    else if (s.Level > currentLevel && s.Status != "completed")
                    {
                        levelSteps[i] = new RoadmapLevelStep(s.Level, s.Title, "locked");
                    }
                }

                string levelName = levelSteps.FirstOrDefault(s => s.Level == currentLevel)?.Title ?? $"Nivel {currentLevel}";

                decimal avgSuccess = 0m;
                string gasLabel;

                var completedResponses = personAssignments
                    .SelectMany(a => a.Responses ?? [])
                    .Where(r => r.CompletedAt.HasValue && r.SuccessPercentage.HasValue)
                    .ToList();

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
                else if (completedResponses.Count > 0)
                {
                    avgSuccess = Math.Round(completedResponses.Average(r => r.SuccessPercentage!.Value), 1);
                    if (avgSuccess >= 80m)
                        gasLabel = "Superando objetivos con autonomía";
                    else if (avgSuccess >= 60m)
                        gasLabel = "En ritmo de progreso esperado";
                    else
                        gasLabel = "En desarrollo, requiere apoyos en casa";
                }
                else
                {
                    gasLabel = "Iniciando camino";
                }

                result[pid] = new PersonRoadmapProgress(currentLevel, levelName, gasLabel, avgSuccess, hasFrustrationAlert, levelSteps);
            }

            return result;
        }
    }
}
