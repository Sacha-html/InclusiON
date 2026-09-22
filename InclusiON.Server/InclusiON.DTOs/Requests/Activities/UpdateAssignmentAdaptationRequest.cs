namespace InclusiON.DTOs.Requests.Activities
{
    public class UpdateAssignmentAdaptationRequest
    {
        public DateTime? DueDate { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public bool? HasVisualSupport { get; set; }
        public bool? HasAudioSupport { get; set; }
        public bool? UsesEasyReading { get; set; }
        public bool? UsesPictograms { get; set; }
        public bool? RequiresSupervision { get; set; }
        public string? CustomAdaptationNotes { get; set; }
        public bool AcknowledgeAlert { get; set; } = true;
    }
}
