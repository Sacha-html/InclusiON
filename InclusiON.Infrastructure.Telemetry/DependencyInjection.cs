using InclusiON.Infrastructure.Configuration;
using InclusiON.Infrastructure.Telemetry.HealthChecks;
using InclusiON.Application.Interfaces.Telemetry;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

namespace InclusiON.Infrastructure.Telemetry;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        var telemetrySettings = configuration.GetSection("Telemetry")
            .Get<TelemetrySettings>() ?? new TelemetrySettings();
        
        var otlpSettings = configuration.GetSection("OpenTelemetry")
            .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

        // [1] Métricas base (instrumentación automática + Prometheus endpoint)
        services.AddOpenTelemetryBaseMetrics(telemetrySettings.ServiceName ?? "InclusiON.Api");

        // [2] Métricas custom (servicio propio)
        services.AddCustomMetrics(telemetrySettings);

        // [3] OTLP Export (envío a Grafana)
        if (otlpSettings.Enabled && !string.IsNullOrEmpty(otlpSettings.Endpoint))
        {
            services.AddOtlpExport(otlpSettings);
        }

        // [4] Health Checks
        services.AddHealthChecks(configuration, connectionString);

        return services;
    }

    private static IServiceCollection AddOpenTelemetryBaseMetrics(
        this IServiceCollection services,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }

    private static IServiceCollection AddCustomMetrics(
        this IServiceCollection services,
        TelemetrySettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<ITelemetryService, TelemetryService>();

        // Interceptor de EF Core: registra duración y tipo de cada query como métrica.
        // AddPersistence lo recoge automáticamente via sp.GetServices<IInterceptor>().
        services.AddSingleton<IInterceptor, TelemetryCommandInterceptor>();

        return services;
    }

    private static IServiceCollection AddOtlpExport(
        this IServiceCollection services,
        OpenTelemetrySettings settings)
    {
        if (!settings.Enabled || string.IsNullOrEmpty(settings.Endpoint))
        {
            return services;
        }

        var otlpHeaders = settings.Headers != null
            ? string.Join(",", settings.Headers.Select(h => $"{h.Key}={h.Value}"))
            : null;

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(settings.Endpoint);
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    options.Headers = otlpHeaders;
                });
            });

        return services;
    }

    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        var smtpSettings = configuration.GetSection("SmtpSettings")
            .Get<InclusiON.Infrastructure.Configuration.SmtpSettings>() 
            ?? new InclusiON.Infrastructure.Configuration.SmtpSettings();

        services.AddSingleton(smtpSettings);
        
        // PostgresHealthCheck usa factory lambda porque connectionString viene como parámetro del método,
        // no es un servicio registrado en DI. GrafanaCloudHealthCheck resuelve sus deps directamente del contenedor.
        services.AddSingleton(sp => new PostgresHealthCheck(connectionString));

        var otlpSettings = configuration.GetSection("OpenTelemetry")
            .Get<OpenTelemetrySettings>();
        
        if (otlpSettings?.Enabled == true && !string.IsNullOrEmpty(otlpSettings.Endpoint))
        {
            services.AddSingleton<GrafanaCloudHealthCheck>();
        }
        
        var builder = services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready", "db"])
            .AddCheck<SmtpHealthCheck>("smtp", tags: ["ready", "email"]);
        
        if (otlpSettings?.Enabled == true && !string.IsNullOrEmpty(otlpSettings.Endpoint))
        {
            builder.AddCheck<GrafanaCloudHealthCheck>("grafana_cloud", tags: ["ready", "metrics"]);
        }

        return services;
    }
}