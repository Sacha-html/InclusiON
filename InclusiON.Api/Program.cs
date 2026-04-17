using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Scalar.AspNetCore;
using InclusiON.Application;
using InclusiON.Data;
using InclusiON.Data.Seeders;
using InclusiON.Api.Middleware;
using InclusiON.Api.Scalar;
using InclusiON.Infrastructure;
using InclusiON.Infrastructure.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/inclusion-.log",
                        rollingInterval: RollingInterval.Day,
                         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<InclusiON.Api.Filters.ValidationFilter>();
    options.Filters.Add<InclusiON.Api.Filters.InstitutionAccessFilter>();
})
.AddJsonOptions(options =>
{
    // Normalizar DateTime entrante a UTC — Npgsql rechaza Kind=Unspecified en timestamp with time zone
    options.JsonSerializerOptions.Converters.Add(new InclusiON.Api.Converters.UtcDateTimeConverter());
});

builder.Services.AddPersistence(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConn")
    ?? throw new InvalidOperationException("Connection string 'PostgreSqlConn' not found.");

builder.Services.AddInfrastructureTelemetry(builder.Configuration, connectionString);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddTransient<OpenApiExamplesTransformer>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "InclusiON API",
            Version = "v1",
            Description = "InclusiON - Sistema de gestión para instituciones de educación especial",
            Contact = new OpenApiContact
            {
                Name = "InclusiON",
                Email = "contacto@inclusion.edu.ar"
            }   
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization. Ingresá 'Bearer' seguido de tu token. Ejemplo: 'Bearer eyJhbGci...'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            }
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, ct) =>
    {
        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer")] = []
        });
        return Task.CompletedTask;
    });

    options.AddOperationTransformer<OpenApiExamplesTransformer>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("InclusiON API")
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .AddPreferredSecuritySchemes(["Bearer"])
               .AddHttpAuthentication("Bearer", scheme =>
               {
                   scheme.Token = string.Empty;
               });
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontendClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPrometheusScrapingEndpoint("/metrics");
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

// Seed inicial de datos
await DatabaseSeeder.SeedAsync(app.Services);

Log.Information("API running on: {Urls}", string.Join(", ", app.Urls));
Log.Information("API Docs: {Url}/scalar/v1", app.Urls.FirstOrDefault());

app.Run();
