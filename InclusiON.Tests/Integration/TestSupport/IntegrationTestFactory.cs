using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using InclusiON.Api.ModelBinders;
using InclusiON.Data;

namespace InclusiON.Tests.Integration.TestSupport
{
    /// <summary>
    /// Factory de <see cref="WebApplicationFactory{TEntryPoint}"/> para integration tests.
    /// Reemplaza la registracion de <see cref="AppDbContext"/> por una instancia en memoria
    /// para que los tests no requieran Postgres ni conexion externa.
    ///
    /// Uso tipico:
    /// <code>
    /// public class MyTests : IClassFixture&lt;IntegrationTestFactory&gt;
    /// {
    ///     private readonly HttpClient _client;
    ///     public MyTests(IntegrationTestFactory factory) =&gt; _client = factory.CreateClient();
    /// }
    /// </code>
    /// </summary>
    public class IntegrationTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTests");

            // Configuracion minima que satisface los requires de startup (AddInfrastructure
            // exige connection string y JwtSettings). UseSetting precede a appsettings.json
            // en la cadena de resolucion de IConfiguration.
            builder.UseSetting("ConnectionStrings:PostgreSqlConn", "Host=tests;Database=tests;Username=tests;Password=tests");
            builder.UseSetting("JwtSettings:Secret", "this-is-a-test-only-jwt-secret-key-with-enough-length-for-hmac-256");
            builder.UseSetting("JwtSettings:Issuer", "InclusiONTests");
            builder.UseSetting("JwtSettings:Audience", "InclusiONTests");
            builder.UseSetting("JwtSettings:ExpirationMinutes", "60");
            builder.UseSetting("JwtSettings:RefreshTokenExpirationDays", "7");

            builder.ConfigureServices(services =>
            {
                // Quitar registracion previa de AppDbContext (viene de AddPersistence con Npgsql).
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                        || d.ServiceType == typeof(DbContextOptions)
                        || d.ServiceType == typeof(AppDbContext))
                    .ToList();

                foreach (var descriptor in toRemove)
                {
                    services.Remove(descriptor);
                }

                // InMemory en un service provider aislado para evitar el clash con los servicios
                // de Npgsql ya registrados a nivel de IServiceCollection principal.
                var efServiceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                    options.UseInternalServiceProvider(efServiceProvider);
                });

                // PostConfigure garantiza que la clave de firma JWT usada para validar tokens
                // coincida con la que usa TokenHelper — independientemente del orden en que
                // AddInfrastructure leyó la IConfiguration en Program.cs.
                // UseSetting afecta la configuración del host, no la app configuration en
                // el modelo de hosting mínimo, por eso sobreescribimos directamente en options.
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opts =>
                {
                    var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("this-is-a-test-only-jwt-secret-key-with-enough-length-for-hmac-256"));
                    opts.TokenValidationParameters.IssuerSigningKey = key;
                    opts.TokenValidationParameters.ValidIssuer      = "InclusiONTests";
                    opts.TokenValidationParameters.ValidAudience     = "InclusiONTests";
                });

                // En integration tests los IDs de fixture se pasan como GUIDs en texto plano
                // (no encriptados), así que quitamos el model binder que intenta descifrarlos
                // y que devolvería 400 antes de que corran los filtros de autorización.
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
