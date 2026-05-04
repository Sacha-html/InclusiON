namespace InclusiON.DTOs.Responses.Family
{
    /// <summary>
    /// Resultado reciente de una actividad realizada por la persona, para el dashboard familiar.
    /// </summary>
    public class RecentActivityResultResponse
    {
        public int    AssignmentId      { get; set; }
        public string ActivityTitle     { get; set; } = string.Empty;
        public string? Result           { get; set; }
        public decimal? SuccessPercentage { get; set; }
        public DateTime CompletedAt     { get; set; }
    }
}
