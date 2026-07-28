using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using InclusiON.Infrastructure.Configuration;

namespace InclusiON.Infrastructure.Telemetry.HealthChecks;

public class SmtpHealthCheck : IHealthCheck
{
    private readonly SmtpSettings _smtpSettings;

    public SmtpHealthCheck(SmtpSettings smtpSettings)
    {
        _smtpSettings = smtpSettings;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_smtpSettings.Enabled)
        {
            return HealthCheckResult.Healthy("SMTP is disabled in configuration");
        }

        try
        {
            var host = _smtpSettings.Host;
            var port = _smtpSettings.Port;

            using var client = new TcpClient();
            
            await client.ConnectAsync(host, port, cancellationToken);

            using var stream = client.GetStream();
            stream.ReadTimeout = 5000;

            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            var response = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

            if (!response.StartsWith("220"))
            {
                return HealthCheckResult.Unhealthy($"SMTP server responded with: {response.Trim()}");
            }

            var quitCommand = "QUIT\r\n";
            var quitBytes = System.Text.Encoding.ASCII.GetBytes(quitCommand);
            await stream.WriteAsync(quitBytes, 0, quitBytes.Length, cancellationToken);

            return HealthCheckResult.Healthy("SMTP server is reachable");
        }
        catch (SocketException ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Cannot connect to SMTP server: {ex.Message}",
                exception: ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "SMTP health check failed",
                exception: ex);
        }
    }
}
