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
            DateTime? latestReportDate,
            int currentRoadmapLevel = 1,
            string roadmapLevelName = "",
            string gasStatusLabel = "Iniciando camino",
            decimal averageSuccessRate = 0m,
            bool hasFrustrationAlert = false) => new()
        {
            PersonId             = person.Id,
            FullName             = $"{person.FirstName} {person.LastName}".Trim(),
            AvatarColor          = person.AvatarColor,
            RecentActivities     = recentActivities,
            ApprovedReportsCount = approvedReportsCount,
            LatestReportTitle    = latestReportTitle,
            LatestReportDate     = latestReportDate,
            CurrentRoadmapLevel  = currentRoadmapLevel,
            RoadmapLevelName     = roadmapLevelName,
            GasStatusLabel       = gasStatusLabel,
            AverageSuccessRate   = averageSuccessRate,
            HasFrustrationAlert  = hasFrustrationAlert,
        };
    }
}
