using System;
using System.Threading;
using System.Threading.Tasks;

namespace InclusiON.Data.Seeders
{
    // Puente estático para que DatabaseSeeder use la inicialización de roadmaps sin referenciar Application o Infrastructure.
    // Se inicializa desde Infrastructure.DependencyInjection al arrancar.
    public static class RoadmapInitializerAccessor
    {
        private static Func<AppDbContext, Guid, Guid?, CancellationToken, Task>? _initialize;

        public static Func<AppDbContext, Guid, Guid?, CancellationToken, Task> InitializeStudentRoadmap =>
            _initialize ?? throw new InvalidOperationException("RoadmapInitializerAccessor not initialized.");

        public static void Initialize(Func<AppDbContext, Guid, Guid?, CancellationToken, Task> initialize) => _initialize = initialize;
    }
}
