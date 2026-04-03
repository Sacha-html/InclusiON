namespace InclusiON.Application.Interfaces.Telemetry;

public interface ITelemetryService
{
    void RecordLogin(string status, string? institutionId);
    void RecordTokenGenerated(string tokenType);
    void RecordDatabaseQuery(string operation, string entity, double durationSeconds);
    void RecordError(string errorType, int statusCode, string endpoint);
}
