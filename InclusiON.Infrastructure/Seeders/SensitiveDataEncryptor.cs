using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InclusiON.Data;
using InclusiON.Domain.Attributes;

namespace InclusiON.Infrastructure.Seeders
{
    // Cifra en-place todos los campos [Encrypted] de string que aún estén en texto plano.
    // Se llama una sola vez al arranque, después de MigrateAsync. Es idempotente:
    // los valores con prefijo "ENC:" ya están cifrados y se saltan.
    public static class SensitiveDataEncryptor
    {
        private const string EncryptedPrefix = "ENC:";

        public static async Task EncryptAsync(IServiceProvider services)
        {
            using var scope  = services.CreateScope();
            var context      = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger       = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            var encryptedProps = GetEncryptedStringProperties();
            var total = 0;

            foreach (var (clrType, properties) in encryptedProps)
            {
                total += await EncryptEntityTypeAsync(context, logger, clrType, properties);
            }

            if (total > 0)
                logger.LogInformation("SensitiveDataEncryptor: {Total} records encrypted.", total);
        }

        private static async Task<int> EncryptEntityTypeAsync(
            AppDbContext context,
            ILogger logger,
            Type clrType,
            PropertyInfo[] properties)
        {
            var dbSet = context.GetType()
                .GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericArguments()[0] == clrType)
                ?.GetValue(context);

            if (dbSet is not IQueryable<object> queryable) return 0;

            var records   = await queryable.ToListAsync();
            var pending   = records.Where(r => NeedsEncryption(r, properties)).ToList();

            if (pending.Count == 0) return 0;

            foreach (var record in pending)
            {
                var entry = context.Entry(record);
                foreach (var prop in properties)
                    entry.Property(prop.Name).IsModified = true;
            }

            await context.SaveChangesAsync();
            logger.LogInformation(
                "SensitiveDataEncryptor: {Count} {Entity} records encrypted.",
                pending.Count, clrType.Name);

            return pending.Count;
        }

        private static bool NeedsEncryption(object entity, PropertyInfo[] properties) =>
            properties.Any(p =>
            {
                var value = p.GetValue(entity) as string;
                return !string.IsNullOrEmpty(value) &&
                       !value.StartsWith(EncryptedPrefix, StringComparison.Ordinal);
            });

        private static Dictionary<Type, PropertyInfo[]> GetEncryptedStringProperties() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return []; } })
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => (Type: t, Props: t.GetProperties()
                    .Where(p => p.PropertyType == typeof(string)
                             && p.GetCustomAttribute<EncryptedAttribute>() != null)
                    .ToArray()))
                .Where(x => x.Props.Length > 0)
                .ToDictionary(x => x.Type, x => x.Props);
    }
}
