using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using InclusiON.Api.ModelBinders;
using InclusiON.Data;

namespace InclusiON.Tests.Integration.TestSupport
{
    public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg17")
            .Build();

        public virtual async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public new virtual async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            return host;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTests");

            builder.UseSetting("ConnectionStrings:PostgreSqlConn", _postgres.GetConnectionString());
            builder.UseSetting("JwtSettings:Secret", "this-is-a-test-only-jwt-secret-key-with-enough-length-for-hmac-256");
            builder.UseSetting("JwtSettings:Issuer", "InclusiONTests");
            builder.UseSetting("JwtSettings:Audience", "InclusiONTests");
            builder.UseSetting("JwtSettings:ExpirationMinutes", "60");
            builder.UseSetting("JwtSettings:RefreshTokenExpirationDays", "7");

            builder.ConfigureServices(services =>
            {
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                        || d.ServiceType == typeof(DbContextOptions)
                        || d.ServiceType == typeof(AppDbContext)
                        || d.ServiceType == typeof(NpgsqlDataSource))
                    .ToList();

                foreach (var descriptor in toRemove)
                    services.Remove(descriptor);

                var dsBuilder = new NpgsqlDataSourceBuilder(_postgres.GetConnectionString());
                dsBuilder.UseVector();
                services.AddSingleton(dsBuilder.Build());

                services.AddDbContext<AppDbContext>((sp, options) =>
                {
                    options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>());
                });

                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opts =>
                {
                    var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("this-is-a-test-only-jwt-secret-key-with-enough-length-for-hmac-256"));
                    opts.TokenValidationParameters.IssuerSigningKey = key;
                    opts.TokenValidationParameters.ValidIssuer      = "InclusiONTests";
                    opts.TokenValidationParameters.ValidAudience     = "InclusiONTests";
                });

                services.PostConfigure<MvcOptions>(opts =>
                {
                    var provider = opts.ModelBinderProviders
                        .FirstOrDefault(p => p is EncryptedGuidModelBinderProvider);
                    if (provider != null)
                        opts.ModelBinderProviders.Remove(provider);
                });
            });
        }
    }
}
