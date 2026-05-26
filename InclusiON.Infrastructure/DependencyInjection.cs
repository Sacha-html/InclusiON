using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InclusiON.Application.Constants;
using Microsoft.IdentityModel.Tokens;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Domain.Models;
using InclusiON.Domain.Models.BaseEntities;
using InclusiON.Infrastructure.Authentication;
using InclusiON.Infrastructure.Configuration;
using InclusiON.Infrastructure.Authorization;
using InclusiON.Infrastructure.Data;
using InclusiON.Infrastructure.Data.Factories;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Data.Converters;
using InclusiON.Data.Seeders;
using InclusiON.Infrastructure.Services;
using System.Text;

namespace InclusiON.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var jwtSettings = configuration.GetSection("JwtSettings")
                .Get<JwtSettings>();

            if (jwtSettings is null)
            {
                throw new InvalidOperationException("Jwt Setting configuration is missing");
            }

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.Configure<BackgroundJobSettings>(configuration.GetSection("BackgroundJobs"));
            services.Configure<PasswordResetSettings>(configuration.GetSection("PasswordResetSettings"));
            services.AddScoped<IPasswordResetConfig, PasswordResetConfig>();

            // Python agent HTTP client
            var pythonUrl = configuration.GetSection("BackgroundJobs:PythonAgent:Url")?.Value
                ?? "http://localhost:5001";
            var pythonTimeout = configuration.GetValue("BackgroundJobs:PythonAgent:TimeoutSeconds", 60);
            services.AddHttpClient("PythonAgent", client =>
            {
                client.BaseAddress = new Uri(pythonUrl);
                client.Timeout = TimeSpan.FromSeconds(pythonTimeout);
            });

            // IEmbeddingService — reemplaza al ONNX local, llama al agente Python vía HTTP
            services.AddSingleton<IEmbeddingService, HttpEmbeddingService>();

            var encryptionService = new AesGcmEncryptionService(configuration);
            EncryptionAccessor.Initialize(encryptionService.Encrypt, encryptionService.Decrypt);
            services.AddSingleton<IEncryptionService>(encryptionService);

            var pinHasher = new Argon2idPinHasher(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<Argon2idPinHasher>.Instance);
            PinHashAccessor.Initialize(pinHasher.Hash);

            var connectionString = configuration.GetConnectionString("PostgreSqlConn");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is missing");
            }

            services.AddScoped<IConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<InclusiON.Application.Interfaces.Infrastructure.IPasswordHasher, PasswordHasher>();
            services.AddScoped<IPinHasher, Argon2idPinHasher>();
            services.AddScoped<TokenServices>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRawDbExecutor, RawDbExecutor>();

            // Repositorios — auto-registrado por convención: clase concreta *Repository → interfaz I*Repository
            var readOnlyRepoOpenType = typeof(IReadOnlyRepository<>);
            var infraAssembly = typeof(UsersRepository).Assembly;

            var repoImplementations = infraAssembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition
                            && t.Name.EndsWith("Repository"));

            foreach (var impl in repoImplementations)
            {
                var repoInterface = impl.GetInterfaces()
                    .FirstOrDefault(i => i.Name.StartsWith("I")
                                        && i.Name.EndsWith("Repository")
                                        && !(i.IsGenericType && i.GetGenericTypeDefinition() == readOnlyRepoOpenType));

                if (repoInterface != null)
                    services.AddScoped(repoInterface, impl);
            }

            // Email
            services.AddScoped<IEmailService, EmailService>();

            // Generación de PDF (QuestPDF)
            services.AddScoped<IReportPdfService, Services.ReportPdfService>();

            // Gestión de roles e Identity RoleClaims
            services.AddScoped<IRoleService, RoleService>();

            // Administración de catálogos (Create/Update/PatchStatus genérico)
            services.AddScoped<ICatalogAdminService, CatalogAdminService>();

            // Repositorios read-only — auto-registrado para todo tipo que implemente IActivatable e IHasIntId
            // (IReadOnlyRepository<TEntity> tiene constraint where TEntity : class, IActivatable, IHasIntId)
            var activatableType = typeof(IActivatable);
            var hasIntIdType    = typeof(IHasIntId);
            var readOnlyRepoOpen = typeof(IReadOnlyRepository<>);
            var readOnlyRepoImplOpen = typeof(ReadOnlyRepository<>);

            var domainTypes = typeof(DisabilityType).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                            && activatableType.IsAssignableFrom(t)
                            && hasIntIdType.IsAssignableFrom(t));

            foreach (var domainType in domainTypes)
            {
                var serviceType = readOnlyRepoOpen.MakeGenericType(domainType);
                var implType = readOnlyRepoImplOpen.MakeGenericType(domainType);
                services.AddScoped(serviceType, implType);
            }

            // Proveedor de fecha/hora (zona horaria Argentina)
            services.AddSingleton<IDateTimeProvider, ArgentinaDateTimeProvider>();

            // Servicio de contexto HTTP (IP, User-Agent, Browser)
            services.AddScoped<IHttpContextService, HttpContextService>();

            // Auditoria de accesos a datos sensibles (HU-IN-172)
            services.AddScoped<IAccessAuditLogger, AccessAuditLogger>();

            // Autorizacion por recurso / row-level (HU-IN-172)
            services.AddScoped<InclusiON.Application.Authorization.IResourceAuthorizationService,
                               Authorization.ResourceAuthorizationService>();

            // Servicio de sesion de login (genera tokens, revoca sesiones, actualiza metadata)
            services.AddScoped<ILoginSessionService, LoginSessionService>();

            // Servicio de roles con cache por request (evita N+1)
            services.AddScoped<IUserRoleService, UserRoleService>();

            // Servicio de permisos (obtiene permisos de AspNetRoleClaims)
            services.AddScoped<IPermissionService, PermissionService>();

            // Autorización basada en permisos
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, GlobalAdminAuthorizationHandler>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = !environment.IsDevelopment();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                    };

                    options.Events = new JwtBearerEvents
                    {
                        // SignalR no puede enviar Authorization header en WebSocket/SSE.
                        // El cliente pasa el token como query param ?access_token=...
                        OnMessageReceived = ctx =>
                        {
                            var accessToken = ctx.Request.Query["access_token"];
                            var path = ctx.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken)
                                && path.StartsWithSegments("/hubs"))
                            {
                                ctx.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },

                        OnTokenValidated = ctx =>
                        {
                            // Verifica el claim isActive embebido en el token.
                            // Si el usuario fue desactivado después de emitir el token, este claim
                            // sigue valiendo "True" (del momento del login) — esta check no cubre
                            // desactivaciones en tiempo real. Para eso habría que consultar la DB,
                            // lo que agrega latencia a cada request. Con ExpirationHours=1 el
                            // margen de exposición post-desactivación es aceptable.
                            // Lo que sí previene: tokens emitidos accidentalmente con isActive=false.
                            var isActiveClaim = ctx.Principal?
                                .FindFirst(Permissions.IsActiveClaimType)?.Value;

                            if (!string.Equals(isActiveClaim, "True", StringComparison.OrdinalIgnoreCase))
                            {
                                ctx.Fail("Cuenta inactiva.");
                            }

                            return Task.CompletedTask;
                        },

                        OnChallenge = ctx =>
                        {
                            // Reemplaza la respuesta 401 por defecto (vacía o HTML) con JSON uniforme.
                            if (ctx.AuthenticateFailure != null)
                            {
                                ctx.HandleResponse();
                                ctx.Response.StatusCode = 401;
                                ctx.Response.ContentType = "application/json";
                                var body = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    success = false,
                                    message = "Token inválido o sesión expirada."
                                });
                                return ctx.Response.Body.WriteAsync(
                                    System.Text.Encoding.UTF8.GetBytes(body)).AsTask();
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // La autorizacion se resuelve dinamicamente via PermissionPolicyProvider.
            // Cualquier [Authorize(Policy = "modulo:accion")] se evalua contra los claims
            // de tipo "permission" del JWT, sin necesidad de registrar politicas manuales.
            services.AddAuthorization();


            return services;
        }
    }
}
