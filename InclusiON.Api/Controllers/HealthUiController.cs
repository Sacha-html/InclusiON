using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InclusiON.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthUiController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IConfiguration _configuration;

    public HealthUiController(HealthCheckService healthCheckService, IConfiguration configuration)
    {
        _healthCheckService = healthCheckService;
        _configuration = configuration;
    }

    [HttpGet("ui")]
    public async Task<IActionResult> GetHealthUi()
    {
        var result = await _healthCheckService.CheckHealthAsync();
        
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var serviceName = _configuration["Telemetry:ServiceName"] ?? "InclusiON.Api";
        var serviceVersion = _configuration["Telemetry:ServiceVersion"] ?? "1.0.0";
        
        var checks = result.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            duration = e.Value.Duration.TotalMilliseconds + "ms",
            exception = e.Value.Exception?.Message,
            data = e.Value.Data
        }).ToList();

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>InclusiON Health Checks</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 40px; background: #1a1a2e; color: #eee; }}
        h1 {{ color: #00d4ff; margin-bottom: 10px; }}
        .info-bar {{ display: flex; gap: 20px; margin-bottom: 20px; padding: 15px; background: #16213e; border-radius: 8px; }}
        .info-item {{ display: flex; flex-direction: column; }}
        .info-label {{ font-size: 12px; color: #888; text-transform: uppercase; }}
        .info-value {{ font-size: 16px; color: #00d4ff; font-weight: bold; }}
        .status {{ padding: 8px 16px; border-radius: 4px; display: inline-block; margin-bottom: 20px; }}
        .Healthy {{ background: #28a745; color: white; }}
        .Unhealthy {{ background: #dc3545; color: white; }}
        .Degraded {{ background: #ffc107; color: black; }}
        table {{ border-collapse: collapse; width: 100%; max-width: 800px; }}
        th, td {{ padding: 12px; text-align: left; border-bottom: 1px solid #333; }}
        th {{ background: #16213e; }}
        .Healthy-row {{ color: #28a745; }}
        .Unhealthy-row {{ color: #dc3545; }}
        .Degraded-row {{ color: #ffc107; }}
        .data-info {{ font-size: 11px; color: #888; margin-top: 4px; }}
    </style>
</head>
<body>
    <h1>InclusiON Health Checks</h1>
    
    <div class='info-bar'>
        <div class='info-item'>
            <span class='info-label'>Environment</span>
            <span class='info-value'>{env}</span>
        </div>
        <div class='info-item'>
            <span class='info-label'>Service</span>
            <span class='info-value'>{serviceName}</span>
        </div>
        <div class='info-item'>
            <span class='info-label'>Version</span>
            <span class='info-value'>{serviceVersion}</span>
        </div>
        <div class='info-item'>
            <span class='info-label'>Timestamp</span>
            <span class='info-value'>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</span>
        </div>
    </div>
    
    <div class='status {result.Status}'>{result.Status} ({result.Entries.Count} checks)</div>
    <table>
        <thead>
            <tr>
                <th>Name</th>
                <th>Status</th>
                <th>Duration</th>
                <th>Details</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", checks.Select(c => $@"
            <tr class='{c.status}-row'>
                <td>{c.name}</td>
                <td>{c.status}</td>
                <td>{c.duration}</td>
                <td>
                    {c.exception ?? c.description ?? "-"}
                    {GetDataInfo(c.data)}
                </td>
            </tr>"))}
        </tbody>
    </table>
    <p style='margin-top: 20px; color: #666;'>Refresh the page to update</p>
</body>
</html>";

        return Content(html, "text/html");
    }

    private static string GetDataInfo(IReadOnlyDictionary<string, object?> data)
    {
        if (data == null || data.Count == 0) return "";
        
        var items = data.Where(d => d.Value != null)
            .Select(d => $"{d.Key}: {d.Value}");
        
        return items.Any() 
            ? $"<div class='data-info'>{string.Join(", ", items)}</div>" 
            : "";
    }
}