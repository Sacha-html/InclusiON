using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using InclusiON.Domain.Models;

namespace InclusiON.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                configuration.GetConnectionString("PostgreSqlConn"));
            dataSourceBuilder.UseVector();
            var dataSource = dataSourceBuilder.Build();
            services.AddSingleton(dataSource);

            services.AddDbContext<AppDbContext>((sp, opt) =>
            {
                opt
                    .LogTo(Console.WriteLine,
                        new[] { DbLoggerCategory.Database.Command.Name },
                        LogLevel.Information)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

                // Solo en desarrollo: loguea valores de parámetros SQL (passwords, emails, etc.)
                if (isDevelopment)
                    opt.EnableSensitiveDataLogging();
                opt
                    .UseNpgsql(dataSource, npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(180);
                    });

                // Registra cualquier IInterceptor registrado en el contenedor (ej: TelemetryCommandInterceptor).
                // AddPersistence no necesita referenciar el ensamblado de telemetría directamente;
                // cada capa registra sus interceptores y EF los levanta acá.
                var interceptors = sp.GetServices<IInterceptor>().ToArray();
                if (interceptors.Length > 0)
                    opt.AddInterceptors(interceptors);
            });

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;

                // Bloqueo de cuenta tras intentos fallidos.
                // 5 intentos / 15 min aplica a todos los tipos de usuario (profesional, familia, admin, persona).
                // Cada handler visual llama AccessFailedAsync() manualmente al fallar credenciales;
                // el login estándar delega en CheckPasswordAsync(lockoutOnFailure: true).
                options.Lockout.AllowedForNewUsers    = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<SignInManager<User>>();

            // AddIdentityCore NO registra SignInManager automáticamente (a diferencia de AddIdentity).
            // Se registra explícitamente mediante .AddSignInManager() en la cadena del builder.
            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>();

            return services;
        }
    }
}
