using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using InclusiON.Data;

namespace InclusiON.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer("Server=localhost;Database=InclusiON;Trusted_Connection=true;TrustServerCertificate=true"));

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<AppDbContext>();
        }
    }
}