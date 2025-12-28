using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.Infrastructure.Authentication;
using InclusiON.Infrastructure.Configuration;
using InclusiON.Infrastructure.Data;
using InclusiON.Infrastructure.Data.Factories;
using InclusiON.Infrastructure.Data.Repositories;
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

            //TODO: review nested policies 
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequiereValidUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");
                });

                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ClaimTypes.Role, "Admin");
                });

                options.AddPolicy("AdminOrManager", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager");
                });

                options.AddPolicy("NotCustomer", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager", "Employee");
                });

                options.AddPolicy("CanManageCategories", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager");
                });

                options.AddPolicy("CanViewCategories", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId"); // Cualquier usuario con userId
                });

                options.AddPolicy("ValidAdminUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");                    // ✅ Usuario válido
                    policy.RequireClaim(ClaimTypes.Role, "Admin");    // ✅ + Role Admin
                });

                options.AddPolicy("ValidManagerOrAbove", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("userId");                                // ✅ Usuario válido
                    policy.RequireClaim(ClaimTypes.Role, "Admin", "Manager");     // ✅ + Admin o Manager
                });
            });


            return services;
        }
    }
}
