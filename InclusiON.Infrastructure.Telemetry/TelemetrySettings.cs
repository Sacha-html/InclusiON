namespace InclusiON.Infrastructure.Telemetry;

public class TelemetrySettings
{
    public string ServiceName { get; set; } = "InclusiON-API";
    public string ServiceVersion { get; set; } = "1.0.0";
    public bool Enabled { get; set; } = true;
}
