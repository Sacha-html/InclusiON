namespace InclusiON.DTOs.Requests.Persons
{
    public class UpdatePersonAccessibilityRequest
    {
        public bool    RequiresLargeFont      { get; set; }
        public bool    RequiresHighContrast   { get; set; }
        public bool    VisualNoiseSensitivity { get; set; }
        public bool    SoundSensitivity       { get; set; }
        public string? ColorBlindnessType     { get; set; }
    }
}
