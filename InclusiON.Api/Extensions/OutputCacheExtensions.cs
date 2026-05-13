namespace InclusiON.Api.Extensions
{
    public static class OutputCacheExtensions
    {
        public static IServiceCollection AddApiOutputCache(this IServiceCollection services)
        {
            services.AddOutputCache(options =>
            {
                // "catalogs": cachea CatalogsController 1 hora (alineado con IMemoryCache).
                // Los datos de catálogo son iguales para todos los usuarios — sin variación por usuario.
                // Se invalida por tag desde CatalogAdminController al persistir cambios.
                // TODO: reemplazar por Redis en despliegues multi-réplica.
                options.AddPolicy("catalogs", b => b
                    .Expire(TimeSpan.FromHours(1))
                    .Tag("catalogs"));

                // "static": permisos disponibles — hardcodeados, nunca cambian en runtime.
                options.AddPolicy("static", b => b.Expire(TimeSpan.FromHours(24)));

                // "roles": lista y detalle de roles — cambian muy poco. Tag para invalidación.
                options.AddPolicy("roles", b => b
                    .Expire(TimeSpan.FromMinutes(30))
                    .Tag("roles"));

                // "history": endpoints de historial/audit log — datos inmutables (append-only).
                options.AddPolicy("history", b => b.Expire(TimeSpan.FromHours(1)));

                // "institutions": lista pública de instituciones. Tag para invalidación.
                options.AddPolicy("institutions", b => b
                    .Expire(TimeSpan.FromMinutes(15))
                    .Tag("institutions"));

                // "admins": lista de admins institucionales. Tag para invalidación.
                options.AddPolicy("admins", b => b
                    .Expire(TimeSpan.FromMinutes(15))
                    .Tag("admins"));
            });

            return services;
        }
    }
}
