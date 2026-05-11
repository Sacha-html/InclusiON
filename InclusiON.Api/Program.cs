using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Scalar.AspNetCore;
using InclusiON.Application;
using InclusiON.Data;
using InclusiON.Data.Seeders;
using InclusiON.Api.Extensions;
using InclusiON.Api.Middleware;
using InclusiON.Api.Scalar;
using InclusiON.Infrastructure;
using InclusiON.Infrastructure.Seeders;
using InclusiON.Infrastructure.Telemetry;
using InclusiON.Agents;

var builder = WebApplication.CreateBuilder(args);

// No revelar que el servidor es Kestrel ni su versión en el header "Server".
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddApiRateLimiter(builder.Configuration);
builder.Services.AddApiOutputCache();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Append("application/json");
});

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
    options.ModelBinderProviders.Insert(0, new InclusiON.Api.ModelBinders.EncryptedGuidModelBinderProvider());
    options.ModelBinderProviders.Insert(1, new InclusiON.Api.ModelBinders.EncryptedIntModelBinderProvider());
})
.AddJsonOptions(options =>
{
    // Normalizar DateTime entrante a UTC — Npgsql rechaza Kind=Unspecified en timestamp with time zone
    options.JsonSerializerOptions.Converters.Add(new InclusiON.Api.Converters.UtcDateTimeConverter());
    // Encriptar/desencriptar Guids automáticamente en todos los requests y responses
    options.JsonSerializerOptions.Converters.Add(new InclusiON.Api.Converters.EncryptedGuidConverter());
    options.JsonSerializerOptions.Converters.Add(new InclusiON.Api.Converters.EncryptedNullableGuidConverter());
});

builder.Services.AddPersistence(builder.Configuration, builder.Environment.IsDevelopment());

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConn")
    ?? throw new InvalidOperationException("Connection string 'PostgreSqlConn' not found.");

builder.Services.AddInfrastructureTelemetry(builder.Configuration, connectionString);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();
builder.Services.AddAgents();

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
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        // Si AllowedOrigins está vacío no se llama a ningún método — CORS queda bloqueado.
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

app.UseForwardedHeaders();
app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontendClient");

if (!builder.Configuration.GetValue<bool>("RateLimiter:Disabled"))
    app.UseRateLimiter();
app.UseOutputCache();

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

// Skip migration/seed en el entorno de integration tests — cada suite arma su propio DbContext
// y no comparte estado con el runtime normal.
if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
    }

    await SensitiveDataEncryptor.EncryptAsync(app.Services);

    // Seed inicial de datos
    await DatabaseSeeder.SeedAsync(app.Services);
}

Log.Information("API running on: {Urls}", string.Join(", ", app.Urls));
Log.Information("API Docs: {Url}/scalar/v1", app.Urls.FirstOrDefault());

app.Run();

// Expuesto para WebApplicationFactory<Program> en los integration tests (InclusiON.Tests.Integration).
public partial class Program;
