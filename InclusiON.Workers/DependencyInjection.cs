using Microsoft.Extensions.DependencyInjection;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Workers.Cleanup;
using InclusiON.Workers.Hosted;

namespace InclusiON.Workers;

public static class DependencyInjection
{
    public static IServiceCollection AddAgents(this IServiceCollection services)
    {
        // Job handlers (scoped — use IServiceScopeFactory in singleton workers)
        services.AddScoped<IJobHandler, EmailAgent>();
        services.AddScoped<IJobHandler, NotificationAgent>();
        services.AddScoped<IJobHandler, EmbeddingAgent>();
        services.AddScoped<IJobHandler, AdaptiveAdjustmentAgent>();
        services.AddScoped<IJobHandler, TemplateGenerationAgent>();
        services.AddScoped<IJobHandler, WeeklyProgressReportAgent>();

        // Executor (scoped — depends on IJobHandler and IBackgroundJobRepository)
        services.AddScoped<JobExecutor>();

        // Cleanup steps
        services.AddScoped<ICleanupStep, DeleteCompletedJobsStep>();
        services.AddScoped<ICleanupStep, SuspendInactiveProfessionalsStep>();
        services.AddScoped<ICleanupStep, GenerateTemplateCentroidsStep>();
        services.AddScoped<ICleanupStep, WeeklyReportCleanupStep>();

        // Workers (singleton BackgroundService — create scope per cycle)
        services.AddHostedService<PendingJobsWorker>();
        services.AddHostedService<MidnightCleanupWorker>();

        return services;
    }
}
