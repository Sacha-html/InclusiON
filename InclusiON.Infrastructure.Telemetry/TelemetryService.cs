using System.Collections.Generic;
using System.Diagnostics.Metrics;
using InclusiON.Application.Interfaces.Telemetry;

namespace InclusiON.Infrastructure.Telemetry;

public class TelemetryService : ITelemetryService, IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _loginCounter;
    private readonly Counter<long> _tokenCounter;
    private readonly Histogram<double> _dbQueryHistogram;
    private readonly Counter<long> _errorCounter;
    private readonly bool _enabled;

    public TelemetryService(TelemetrySettings settings)
    {
        _enabled = settings.Enabled;
        
        _meter = new Meter($"{settings.ServiceName}.Telemetry", settings.ServiceVersion);
        
        _loginCounter = _meter.CreateCounter<long>(
            "auth_login_total",
            unit: "{login}",
            description: "Total number of login attempts");
        
        _tokenCounter = _meter.CreateCounter<long>(
            "auth_token_generated_total",
            unit: "{token}",
            description: "Total number of tokens generated");
        
        _dbQueryHistogram = _meter.CreateHistogram<double>(
            "db_query_duration_seconds",
            unit: "s",
            description: "Duration of database queries in seconds");
        
        _errorCounter = _meter.CreateCounter<long>(
            "error_total",
            unit: "{error}",
            description: "Total number of errors");
    }

    public void RecordLogin(string status, string? institutionId)
    {
        if (!_enabled) return;
        
        _loginCounter.Add(1, 
            new KeyValuePair<string, object?>("status", status),
            new KeyValuePair<string, object?>("institution_id", institutionId ?? "default"));
    }

    public void RecordTokenGenerated(string tokenType)
    {
        if (!_enabled) return;
        
        _tokenCounter.Add(1, 
            new KeyValuePair<string, object?>("token_type", tokenType));
    }

    public void RecordDatabaseQuery(string operation, string entity, double durationSeconds)
    {
        if (!_enabled) return;
        
        _dbQueryHistogram.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("entity", entity));
    }

    public void RecordError(string errorType, int statusCode, string endpoint)
    {
        if (!_enabled) return;
        
        _errorCounter.Add(1, 
            new KeyValuePair<string, object?>("error_type", errorType),
            new KeyValuePair<string, object?>("status_code", statusCode.ToString()),
            new KeyValuePair<string, object?>("endpoint", endpoint));
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
