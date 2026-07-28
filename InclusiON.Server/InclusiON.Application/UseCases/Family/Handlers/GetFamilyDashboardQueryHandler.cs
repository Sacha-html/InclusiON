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

            var summaries = persons.Select(person =>
            {
                recentResponsesByPerson.TryGetValue(person.Id, out var responses);
                reportSummaryByPerson.TryGetValue(person.Id, out var reportSummary);

                return FamilyMapper.ToPersonSummary(
                    person,
                    recentActivities:     (responses ?? []).Select(FamilyMapper.ToRecentActivityResult).ToList(),
                    approvedReportsCount: reportSummary.Count,
                    latestReportTitle:    reportSummary.Latest?.Title,
                    latestReportDate:     reportSummary.Latest?.ReportDate);
            }).ToList();

            return ApiResponse<FamilyDashboardResponse>.SuccessResult(new FamilyDashboardResponse
            {
                Persons        = summaries,
                UnreadMessages = unreadMessages
            });
        }
    }
}
