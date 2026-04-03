using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;

namespace InclusiON.Infrastructure.Telemetry.HealthChecks;

public class GrafanaCloudHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _endpoint;
    private readonly string? _authHeader;

    public GrafanaCloudHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        
        var otlpSettings = configuration.GetSection("OpenTelemetry");
        _endpoint = otlpSettings["Endpoint"] ?? "";
        _authHeader = otlpSettings["Headers:Authorization"];
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_endpoint))
        {
            return HealthCheckResult.Degraded("Grafana Cloud endpoint not configured");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient("GrafanaHealthCheck");
            
            if (!string.IsNullOrEmpty(_authHeader))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authHeader.Replace("Basic ", ""));
            }

            var testPayload = new byte[] { 0x00 };
            
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Content = new ByteArrayContent(testPayload);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-protobuf");
            
            var response = await httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return HealthCheckResult.Healthy(
                    "Grafana Cloud OTLP endpoint is reachable",
                    data: new Dictionary<string, object?>
                    {
                        ["endpoint"] = _endpoint,
                        ["statusCode"] = (int)response.StatusCode
                    });
            }
            
            return HealthCheckResult.Unhealthy(
                $"Grafana Cloud returned: {response.StatusCode}",
                data: new Dictionary<string, object?>
                {
                    ["endpoint"] = _endpoint,
                    ["statusCode"] = (int)response.StatusCode
                });
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Cannot connect to Grafana Cloud: {ex.Message}",
                exception: ex);
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Unhealthy("Grafana Cloud request timed out");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Grafana Cloud health check failed",
                exception: ex);
        }
    }
}