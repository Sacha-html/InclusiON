namespace InclusiON.DTOs.Responses.Family
{
    /// <summary>
    /// Resumen de una persona vinculada para el dashboard familiar.
    /// </summary>
    public class FamilyPersonSummaryResponse
    {
        public Guid   PersonId       { get; set; }
        public string FullName       { get; set; } = string.Empty;
        public string? AvatarColor   { get; set; }

        /// <summary>Últimas 3 actividades completadas.</summary>
        public List<RecentActivityResultResponse> RecentActivities { get; set; } = new();

        public int    ApprovedReportsCount { get; set; }
        public string? LatestReportTitle   { get; set; }
        public DateTime? LatestReportDate  { get; set; }
    }
}
