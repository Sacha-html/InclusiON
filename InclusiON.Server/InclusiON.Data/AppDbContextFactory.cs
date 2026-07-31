using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using InclusiON.Data;

namespace InclusiON.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            string? connectionString = null;
            try
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../InclusiON.Api");
                if (!Directory.Exists(basePath))
                {
                    basePath = Directory.GetCurrentDirectory();
                }

                var filePath = Path.Combine(basePath, "appsettings.Development.json");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(basePath, "appsettings.json");
                }

                if (File.Exists(filePath))
                {
                    var content = File.ReadAllText(filePath);
                    var marker = "\"PostgreSqlConn\":";
                    var index = content.IndexOf(marker);
                    if (index != -1)
                    {
                        var start = content.IndexOf("\"", index + marker.Length);
                        if (start != -1)
                        {
                            var end = content.IndexOf("\"", start + 1);
                            if (end != -1)
                            {
                                connectionString = content.Substring(start + 1, end - start - 1);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback on exception
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Host=localhost;Port=5432;Database=inclusion_dev;Username=postgres;Password=Tu_Password_Segura123!";
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}