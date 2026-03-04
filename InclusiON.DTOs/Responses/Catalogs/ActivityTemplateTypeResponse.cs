namespace InclusiON.DTOs.Responses.Catalogs
{
    public class ActivityTemplateTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public int SkillAreaId { get; set; }
        public string ContentSchema { get; set; } = string.Empty;
        public string ComponentName { get; set; } = string.Empty;
        public bool UsesPictograms { get; set; }
        public bool HasAudio { get; set; }
        public int DisplayOrder { get; set; }
    }
}
