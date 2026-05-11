namespace InclusiON.Agents;

public readonly record struct JobResult(bool IsSuccess, string? ErrorMessage = null, int? SkippedJobTypeId = null)
{
    public static JobResult Succeeded() => new(true);

    public static JobResult Failed(string error) => new(false, error);

    public static JobResult Unhandled(int jobTypeId) => new(false, $"No handler registered for JobTypeId={jobTypeId}", jobTypeId);
}
