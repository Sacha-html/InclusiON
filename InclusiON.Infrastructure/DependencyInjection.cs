using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.Infrastructure.Authentication;
using InclusiON.Infrastructure.Configuration;
using InclusiON.Infrastructure.Authorization;
using InclusiON.Infrastructure.Data;
using InclusiON.Infrastructure.Data.Factories;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Infrastructure.Services;
using System.Security.Claims;
using System.Text;
using IConnectionFactory = InclusiON.ApplicationBusiness.Interfaces.Infrastructure.IConnectionFactory;

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

            services.AddScoped<IConnectionFactory>(provider => new SqlConnectionFactory(connectionString));

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<InclusiON.ApplicationBusiness.Interfaces.Infrastructure.IPasswordHasher, PasswordHasher>();
            services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IVisualLoginRepository, VisualLoginRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();

            // Servicio de contexto HTTP (IP, User-Agent, Browser)
            services.AddScoped<IHttpContextService, HttpContextService>();

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

            // Politicas de autorizacion consolidadas
            // Todas las politicas requieren autenticacion + userId valido para seguridad consistente
            services.AddAuthorization(options =>
            {
                // Base: cualquier usuario autenticado con userId valido
                options.AddPolicy("ValidUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");
                });

                // Solo administradores
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");
                    policy.RequireClaim(ClaimTypes.Role, "Admin");
                });

                // Administradores o managers
                options.AddPolicy("ManagerOrAbove", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");
                    policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager");
                });

                // Staff (Admin, Manager, Employee) - excluye Person/Family
                options.AddPolicy("StaffOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");
                    policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager", "Employee");
                });

                // Profesionales o superiores (para gestion de personas)
                options.AddPolicy("ProfessionalOrAbove", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");
                    policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager", "Professional");
                });
            });


            return services;
        }
    }
}
