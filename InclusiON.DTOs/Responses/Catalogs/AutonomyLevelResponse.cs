namespace InclusiON.DTOs.Responses.Catalogs
{
    public class AutonomyLevelResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresSupervision { get; set; }
        public int DisplayOrder { get; set; }
    }
}
