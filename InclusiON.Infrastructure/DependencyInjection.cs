using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Infrastructure.Authentication;
using InclusiON.Infrastructure.Configuration;
using InclusiON.Infrastructure.Authorization;
using InclusiON.Infrastructure.Data;
using InclusiON.Infrastructure.Data.Factories;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Infrastructure.Services;
using System.Text;

namespace InclusiON.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings")
                .Get<JwtSettings>();

            if (jwtSettings is null)
            {
                throw new InvalidOperationException("Jwt Setting configuration is missing");
            }

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            var connectionString = configuration.GetConnectionString("SqlServerConn");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is missing");
            }

            services.AddScoped<IConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<InclusiON.Application.Interfaces.Infrastructure.IPasswordHasher, PasswordHasher>();
            services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRawDbExecutor, RawDbExecutor>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IVisualLoginRepository, VisualLoginRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();

            // Servicio de contexto HTTP (IP, User-Agent, Browser)
            services.AddScoped<IHttpContextService, HttpContextService>();

            // Servicio de sesion de login (genera tokens, revoca sesiones, actualiza metadata)
            services.AddScoped<ILoginSessionService, LoginSessionService>();

            // Servicio de roles con cache por request (evita N+1)
            services.AddScoped<IUserRoleService, UserRoleService>();

            // Servicio de permisos (obtiene permisos de AspNetRoleClaims)
            services.AddScoped<IPermissionService, PermissionService>();

            // Autorización basada en permisos
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // Set to true in production
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true
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
