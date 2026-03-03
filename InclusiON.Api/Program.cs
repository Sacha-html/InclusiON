using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using InclusiON.Application;
using InclusiON.Data;
using InclusiON.Data.Seeders;
using InclusiON.Api.Middleware;
using InclusiON.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/inclusion-.log",
                        rollingInterval: RollingInterval.Day,
                         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddControllers();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(p =>
{
    p.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InclusiON API",
        Version = "v1",
        Description = "InclusiON - API Template",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your@email.com"
        }
    });

    p.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header. Enter 'Bearer' [space] and token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    p.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
            },
            Array.Empty<string>()
        }
    });
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

app.UseSwagger();
app.UseSwaggerUI(p =>
{
    p.SwaggerEndpoint("/swagger/v1/swagger.json", "InclusiON API V1");
    p.RoutePrefix = string.Empty;
});

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontendClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

// Seed inicial de datos
await DatabaseSeeder.SeedAsync(app.Services);

Log.Information("API running on: {Urls}", string.Join(", ", app.Urls));
Log.Information("Swagger UI: {SwaggerUrl}/swagger", app.Urls.FirstOrDefault());

app.Run();
