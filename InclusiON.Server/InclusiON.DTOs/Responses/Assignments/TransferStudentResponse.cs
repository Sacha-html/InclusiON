namespace InclusiON.DTOs.Responses.Assignments
{
    public class TransferStudentResponse
    {
        public int ReassignedActivitiesCount { get; set; }
        public int ReassignedReportsCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
