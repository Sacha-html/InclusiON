namespace InclusiON.Application.UseCases.Roadmap.Queries;

/// <summary>
/// Returns one data point per roadmap skill area for the given person,
/// with average success percentage computed from all completed activity responses (IN-90).
/// </summary>
public record GetSkillRadarQuery(Guid PersonId);
