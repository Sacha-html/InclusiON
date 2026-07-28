namespace InclusiON.DTOs.Responses.Roadmap;

public class SkillRadarPointResponse
{
    public string AreaName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public double? AvgSuccessPercent { get; set; }
    public int TotalResponses { get; set; }
}
