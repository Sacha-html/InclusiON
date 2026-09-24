using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.Mappers
{
    public static class FamilyMapper
    {
        public static RecentActivityResultResponse ToRecentActivityResult(ActivityResponse r) => new()
        {
            AssignmentId      = r.AssignmentId,
            ActivityTitle     = r.Assignment.Activity.Title,
            Result            = r.Result?.ToString(),
            SuccessPercentage = r.SuccessPercentage,
            CompletedAt       = r.CompletedAt!.Value,
        };

        public static FamilyPersonSummaryResponse ToPersonSummary(
            PersonWithDisability person,
            List<RecentActivityResultResponse> recentActivities,
            int approvedReportsCount,
            string? latestReportTitle,
            DateTime? latestReportDate) => new()
        {
            PersonId             = person.Id,
            FullName             = $"{person.FirstName} {person.LastName}".Trim(),
            AvatarColor          = person.AvatarColor,
            RecentActivities     = recentActivities,
            ApprovedReportsCount = approvedReportsCount,
            LatestReportTitle    = latestReportTitle,
            LatestReportDate     = latestReportDate,
        };

        public static void ApplyProgress(FamilyPersonSummaryResponse summary,
            IEnumerable<ActivitySession> sessions, IEnumerable<ActivityResponse> responses,
            IEnumerable<ActivityResponse>? startedResponses = null)
        {
            var sessionList = sessions.ToList();
            var roadmapActivities = sessionList.Where(s => s.Activity.RoadmapOrder.HasValue)
                .Select(s => (Order: s.Activity.RoadmapOrder!.Value, s.Activity.Title))
                .Concat((startedResponses ?? []).Where(r => r.Assignment.Activity.RoadmapOrder.HasValue)
                    .Select(r => (Order: r.Assignment.Activity.RoadmapOrder!.Value, r.Assignment.Activity.Title)));
            var currentRoadmapActivity = roadmapActivities.OrderByDescending(a => a.Order).FirstOrDefault();
            summary.CurrentRoadmapLevel = currentRoadmapActivity == default ? null : currentRoadmapActivity.Order;
            summary.CurrentRoadmapLevelName = currentRoadmapActivity == default ? null : currentRoadmapActivity.Title;
            summary.AverageSuccessRate = sessionList.Count == 0 ? 0 : Math.Round(sessionList.Average(s => s.SuccessRate), 1);
            var averageGas = sessionList.Count == 0 ? (decimal?)null : sessionList.Average(s => (decimal)s.GasScore);
            summary.GasStatusLabel = averageGas switch
            {
                null => "Sin datos",
                >= 1.5m => "Muy por encima de lo esperado",
                >= .5m => "Por encima de lo esperado",
                >= -.5m => "Nivel esperado de logro",
                >= -1.5m => "Por debajo de lo esperado",
                _ => "Muy por debajo de lo esperado"
            };

            // Historical rows do not identify a response, so reconstruct the alert from
            // ordered responses for the same activity. GAS alone never raises an alert.
            summary.HasFrustrationAlert = responses.GroupBy(r => r.AssignmentId)
                .Any(group => HasFourConsecutiveFailures(group.OrderBy(r => r.CompletedAt)));
        }

        private static bool HasFourConsecutiveFailures(IEnumerable<ActivityResponse> responses)
        {
            var failures = 0;
            foreach (var response in responses)
            {
                failures = response.SuccessPercentage.HasValue && response.SuccessPercentage.Value < 60m
                    ? failures + 1
                    : 0;
                if (failures >= 4) return true;
            }
            return false;
        }
    }
}
