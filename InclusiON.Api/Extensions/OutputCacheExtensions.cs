namespace InclusiON.Api.Extensions
{
    public static class OutputCacheExtensions
    {
        public static IServiceCollection AddApiOutputCache(this IServiceCollection services)
        {
            services.AddOutputCache(options =>
            {
                // "catalogs": cachea CatalogsController 10 min por usuario.
                // Se invalida por tag desde CatalogAdminController al persistir cambios.
                // TODO: reemplazar por Redis en despliegues multi-réplica.
                options.AddPolicy("catalogs", b => b
                    .Expire(TimeSpan.FromMinutes(10))
                    .Tag("catalogs")
                    .SetVaryByHeader("Authorization"));
            });

            return services;
        }
    }
}
