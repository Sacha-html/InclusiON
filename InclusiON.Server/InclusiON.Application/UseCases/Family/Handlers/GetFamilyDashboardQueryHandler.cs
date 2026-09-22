using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetFamilyDashboardQueryHandler
        : IQueryHandler<GetFamilyDashboardQuery, ApiResponse<FamilyDashboardResponse>>
    {
        private readonly IFamilyRepository              _family;
        private readonly IActivityAssignmentRepository  _assignments;
        private readonly IReportsRepository             _reports;
        private readonly IMessagesRepository            _messages;

        public GetFamilyDashboardQueryHandler(
            IFamilyRepository             family,
            IActivityAssignmentRepository assignments,
            IReportsRepository            reports,
            IMessagesRepository           messages)
        {
            _family      = family;
            _assignments = assignments;
            _reports     = reports;
            _messages    = messages;
        }

        public async Task<ApiResponse<FamilyDashboardResponse>> HandleAsync(
            GetFamilyDashboardQuery query, CancellationToken cancellationToken)
        {
            // 1. Personas vinculadas activamente al familiar
            var persons = await _family.GetLinkedPersonsAsync(query.FamilyUserId, cancellationToken);

            // 2. Mensajes no leídos (por UserId del familiar)
            var unreadMessages = await _messages.GetUnreadCountAsync(query.FamilyUserId, cancellationToken);

            // 3. Bulk: actividades recientes + resumen de reportes para todas las personas en 2 queries
            var personIds = persons.Select(p => p.Id).ToList();

            var recentResponsesByPerson = await _assignments
                .GetRecentCompletedResponsesByPersonIdsAsync(personIds, limit: 3, cancellationToken);

            var reportSummaryByPerson = await _reports
                .GetApprovedReportsSummaryByPersonIdsAsync(personIds, cancellationToken);

            var roadmapProgressByPerson = await _assignments
                .GetRoadmapProgressByPersonIdsAsync(personIds, cancellationToken);

            var responsesMap = recentResponsesByPerson ?? new();
            var reportsMap = reportSummaryByPerson ?? new();
            var progressMap = roadmapProgressByPerson ?? new();

            var summaries = persons.Select(person =>
            {
                responsesMap.TryGetValue(person.Id, out var responses);
                reportsMap.TryGetValue(person.Id, out var reportSummary);
                progressMap.TryGetValue(person.Id, out var roadmapProgress);

                return FamilyMapper.ToPersonSummary(
                    person,
                    recentActivities:     (responses ?? []).Select(FamilyMapper.ToRecentActivityResult).ToList(),
                    approvedReportsCount: reportSummary.Count,
                    latestReportTitle:    reportSummary.Latest?.Title,
                    latestReportDate:     reportSummary.Latest?.ReportDate,
                    currentRoadmapLevel:  roadmapProgress?.CurrentLevel ?? 1,
                    roadmapLevelName:     roadmapProgress?.LevelName ?? "Nivel 1",
                    gasStatusLabel:       roadmapProgress?.GasStatusLabel ?? "Iniciando camino",
                    averageSuccessRate:   roadmapProgress?.AverageSuccessRate ?? 0m,
                    hasFrustrationAlert:  roadmapProgress?.HasFrustrationAlert ?? false,
                    roadmapLevels:        roadmapProgress?.Levels);
            }).ToList();

            return ApiResponse<FamilyDashboardResponse>.SuccessResult(new FamilyDashboardResponse
            {
                Persons        = summaries,
                UnreadMessages = unreadMessages
            });
        }
    }
}
