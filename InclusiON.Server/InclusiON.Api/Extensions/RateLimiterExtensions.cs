using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading.RateLimiting;

namespace InclusiON.Api.Extensions
{
    public static class RateLimiterExtensions
    {
        public static IServiceCollection AddApiRateLimiter(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("RateLimiter:Disabled"))
                return services;

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();

                // Proxies/LBs confiables configurados por ambiente.
                // Dev: vacío → conexión directa, RemoteIpAddress ya es el cliente real.
                // Prod: incluir la IP del nginx/ALB para que X-Forwarded-For sea procesado
                //       y el rate limiter parta por el IP real del cliente, no por la IP del LB.
                var trustedProxies = configuration
                    .GetSection("ForwardedHeaders:TrustedProxies")
                    .Get<string[]>() ?? [];

                foreach (var proxy in trustedProxies)
                    if (IPAddress.TryParse(proxy, out var ip))
                        options.KnownProxies.Add(ip);
            });

            services.AddRateLimiter(options =>
            {
                // Baseline global: 100 req/min por IP. Los endpoints con [EnableRateLimiting("...")] aplican
                // sus propios límites más estrictos encima de este.
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window      = TimeSpan.FromMinutes(1),
                            PermitLimit = 100,
                            QueueLimit  = 0
                        }));

                options.OnRejected = async (ctx, token) =>
                {
                    ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await ctx.HttpContext.Response.WriteAsJsonAsync(
                        new { success = false, message = "Demasiados intentos. Esperá unos minutos antes de reintentar." },
                        token);
                };

                // PIN: ventana deslizante de 5 min, máx 5 intentos por IP
                options.AddPolicy("auth-pin", ctx =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            Window            = TimeSpan.FromMinutes(5),
                            SegmentsPerWindow = 5,
                            PermitLimit       = 5,
                            QueueLimit        = 0
                        }));

                // Login estándar / visual / familiar / asistido: 10 intentos / 1 min por IP
                options.AddPolicy("auth-login", ctx =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            Window            = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            PermitLimit       = 10,
                            QueueLimit        = 0
                        }));

                // Refresh token: 20 / 1 min por IP (flujo frecuente en SPAs)
                options.AddPolicy("auth-refresh", ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window      = TimeSpan.FromMinutes(1),
                            PermitLimit = 20,
                            QueueLimit  = 0
                        }));

                // Endpoints sensibles (register, change-password): 5 / 1 min por IP.
                // Registro: evita spam de cuentas. Change-password: evita brute force de contraseña actual.
                options.AddPolicy("auth-sensitive", ctx =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            Window            = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 3,
                            PermitLimit       = 5,
                            QueueLimit        = 0
                        }));
            });

            return services;
        }
    }
}
