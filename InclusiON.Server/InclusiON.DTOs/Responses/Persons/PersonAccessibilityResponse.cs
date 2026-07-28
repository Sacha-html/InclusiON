namespace InclusiON.DTOs.Responses.Persons
{
    public class PersonAccessibilityResponse
    {
        public bool RequiresLargeFont      { get; set; }
        public bool RequiresHighContrast   { get; set; }
        public bool VisualNoiseSensitivity { get; set; }
        public bool SoundSensitivity       { get; set; }
        public string? ColorBlindnessType  { get; set; }
    }
}
