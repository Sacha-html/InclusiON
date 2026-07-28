using System.Collections.Generic;

namespace InclusiON.Infrastructure.Telemetry;

public class OpenTelemetrySettings
{
    public bool Enabled { get; set; } = true;
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; } = "http/protobuf";
    public Dictionary<string, string>? Headers { get; set; }
}