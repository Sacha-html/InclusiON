namespace InclusiON.DTOs.Responses.Roadmap;

public class AdaptiveAdjustmentLogResponse
{
    public int Id { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public string PreviousValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime AdjustedAt { get; set; }

    public static AdaptiveAdjustmentLogResponse From(InclusiON.Domain.Models.AdaptiveAdjustmentLog log) => new()
    {
        Id             = log.Id,
        AdjustmentType = log.AdjustmentType,
        PreviousValue  = log.PreviousValue,
        NewValue       = log.NewValue,
        Reason         = log.Reason,
        AdjustedAt     = log.AdjustedAt,
    };
}
