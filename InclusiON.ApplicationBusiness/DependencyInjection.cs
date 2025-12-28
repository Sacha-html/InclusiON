using Microsoft.Extensions.DependencyInjection;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using System.Reflection;

namespace InclusiON.ApplicationBusiness
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            Register(services, typeof(ICommandHandler<,>));
            Register(services, typeof(IQueryHandler<,>));

            return services;
        }

        private static void Register(IServiceCollection services, Type openGenericHandlerType)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var handlers = assembly
                .GetTypes()
                .Where(p => !p.IsAbstract && !p.IsInterface)
                .SelectMany(q => q.GetInterfaces()
                                .Where(r => r.IsGenericType && r.GetGenericTypeDefinition() == openGenericHandlerType)
                                .Select(s => new { Interface = s, Implementation = q }))
                .ToList();

            foreach (var handler in handlers)
            {
                services.AddScoped(handler.Interface, handler.Implementation);
            }
        }
    }
}
