using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
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

            // 3. Por cada persona: actividades recientes + resumen de reportes
            var summaries = new List<FamilyPersonSummaryResponse>();

            foreach (var person in persons)
            {
                var recentResponses = await _assignments.GetRecentCompletedResponsesAsync(
                    person.Id, limit: 3, cancellationToken);

                var (reportCount, latestReport) = await _reports.GetApprovedReportsSummaryAsync(
                    person.Id, cancellationToken);

                summaries.Add(new FamilyPersonSummaryResponse
                {
                    PersonId    = person.Id,
                    FullName    = $"{person.FirstName} {person.LastName}".Trim(),
                    AvatarColor = person.AvatarColor,

                    RecentActivities = recentResponses.Select(r => new RecentActivityResultResponse
                    {
                        AssignmentId      = r.AssignmentId,
                        ActivityTitle     = r.Assignment.Activity.Title,
                        Result            = r.Result?.ToString(),
                        SuccessPercentage = r.SuccessPercentage,
                        CompletedAt       = r.CompletedAt!.Value
                    }).ToList(),

                    ApprovedReportsCount = reportCount,
                    LatestReportTitle    = latestReport?.Title,
                    LatestReportDate     = latestReport?.ReportDate
                });
            }

            return ApiResponse<FamilyDashboardResponse>.SuccessResult(new FamilyDashboardResponse
            {
                Persons        = summaries,
                UnreadMessages = unreadMessages
            });
        }
    }
}
