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
                options.UseNpgsql("Host=localhost;Port=5432;Database=inclusion_dev;Username=postgres;Password=Tu_Password_Segura123!"));

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<AppDbContext>();
        }
    }
}