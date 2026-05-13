using Microsoft.Extensions.DependencyInjection;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Agents.Cleanup;
using InclusiON.Agents.Workers;

namespace InclusiON.Agents;

public static class DependencyInjection
{
    public static IServiceCollection AddAgents(this IServiceCollection services)
    {
        // Job handlers (scoped — use IServiceScopeFactory in singleton workers)
        services.AddScoped<IJobHandler, EmailAgent>();
        services.AddScoped<IJobHandler, NotificationAgent>();
        services.AddScoped<IJobHandler, EmbeddingAgent>();

        // Executor (scoped — depends on IJobHandler and IBackgroundJobRepository)
        services.AddScoped<JobExecutor>();

        // Cleanup steps
        services.AddScoped<ICleanupStep, DeleteCompletedJobsStep>();
        services.AddScoped<ICleanupStep, SuspendInactiveProfessionalsStep>();
        services.AddScoped<ICleanupStep, GenerateTemplateCentroidsStep>();

        // Workers (singleton BackgroundService — create scope per cycle)
        services.AddHostedService<PendingJobsWorker>();
        services.AddHostedService<MidnightCleanupWorker>();

        return services;
    }
}
