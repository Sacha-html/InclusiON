using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class GetWeeklyProgressQueryHandler
        : IQueryHandler<GetWeeklyProgressQuery, ApiResponse<WeeklyProgressResponse>>
    {
        private readonly IAssignmentsRepository         _assignments;
        private readonly IActivityAssignmentRepository  _activityAssignments;
        private readonly IDateTimeProvider              _dateTime;

        public GetWeeklyProgressQueryHandler(
            IAssignmentsRepository assignmentsRepository,
            IActivityAssignmentRepository activityAssignmentRepository,
            IDateTimeProvider dateTime)
        {
            _assignments         = assignmentsRepository;
            _activityAssignments = activityAssignmentRepository;
            _dateTime            = dateTime;
        }

        public async Task<ApiResponse<WeeklyProgressResponse>> HandleAsync(
            GetWeeklyProgressQuery query,
            CancellationToken cancellationToken)
        {
            var periodEnd   = _dateTime.UtcNow;
            var periodStart = periodEnd.AddDays(-7);

            // Personas activas a cargo del profesional
            var persons        = await _assignments.GetPersonsByProfessionalIdAsync(query.ProfessionalId, cancellationToken);
            var activePersonIds = persons.Select(p => p.PersonId).ToList();

            if (activePersonIds.Count == 0)
            {
                return ApiResponse<WeeklyProgressResponse>.SuccessResult(new WeeklyProgressResponse
                {
                    PeriodStart       = periodStart,
                    PeriodEnd         = periodEnd,
                    PersonCount       = 0,
                    TotalCompleted    = 0,
                    AvgSuccess        = 0m,
                    FrustrationAlerts = 0,
                });
            }

            // Respuestas recientes (máx 50 por persona), filtradas al período
            var responsesByPerson = await _activityAssignments
                .GetRecentCompletedResponsesByPersonIdsAsync(activePersonIds, 50, cancellationToken);

            var weeklyByPerson = responsesByPerson
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Where(r => r.CompletedAt >= periodStart).ToList());

            var totalCompleted = weeklyByPerson.Values.Sum(r => r.Count);

            var avgSuccess = totalCompleted > 0
                ? weeklyByPerson.Values.SelectMany(r => r)
                    .Where(r => r.SuccessPercentage.HasValue)
                    .Select(r => r.SuccessPercentage!.Value)
                    .DefaultIfEmpty(0m)
                    .Average()
                : 0m;

            var frustrationAlerts = weeklyByPerson.Values.SelectMany(r => r)
                .Count(r => r.FrustrationLevel.HasValue && r.FrustrationLevel.Value >= 4);

            return ApiResponse<WeeklyProgressResponse>.SuccessResult(new WeeklyProgressResponse
            {
                PeriodStart       = periodStart,
                PeriodEnd         = periodEnd,
                PersonCount       = activePersonIds.Count,
                TotalCompleted    = totalCompleted,
                AvgSuccess        = Math.Round(avgSuccess, 0),
                FrustrationAlerts = frustrationAlerts,
            });
        }
    }
}
