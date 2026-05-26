namespace InclusiON.DTOs.Responses.Professionals
{
    public class WeeklyProgressResponse
    {
        public DateTime PeriodStart       { get; init; }
        public DateTime PeriodEnd         { get; init; }
        public int      PersonCount       { get; init; }
        public int      TotalCompleted    { get; init; }
        public decimal  AvgSuccess        { get; init; }
        public int      FrustrationAlerts { get; init; }
    }
}
