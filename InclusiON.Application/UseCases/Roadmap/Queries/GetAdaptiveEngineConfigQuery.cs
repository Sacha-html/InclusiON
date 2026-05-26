namespace InclusiON.Application.UseCases.Roadmap.Queries;

/// <summary>Returns the adaptive engine config for a roadmap activity, or 404 if not set (IN-116).</summary>
public record GetAdaptiveEngineConfigQuery(int PersonRoadmapActivityId);
