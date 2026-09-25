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
        private static Func<AppDbContext, CancellationToken, Task>? _ensureActivities;
        private static Func<AppDbContext, CancellationToken, Task<int>>? _repairAssigned;

        public static Func<AppDbContext, Guid, Guid?, CancellationToken, Task> InitializeStudentRoadmap =>
            _initialize ?? throw new InvalidOperationException("RoadmapInitializerAccessor not initialized.");

        public static Func<AppDbContext, CancellationToken, Task> EnsureStandardActivities =>
            _ensureActivities ?? ((_, _) => Task.CompletedTask);

        public static Func<AppDbContext, CancellationToken, Task<int>> RepairAssignedStudentRoadmaps =>
            _repairAssigned ?? ((_, _) => Task.FromResult(0));

        public static void Initialize(
            Func<AppDbContext, Guid, Guid?, CancellationToken, Task> initialize,
            Func<AppDbContext, CancellationToken, Task>? ensureActivities = null,
            Func<AppDbContext, CancellationToken, Task<int>>? repairAssigned = null)
        {
            _initialize = initialize;
            _ensureActivities = ensureActivities;
            _repairAssigned = repairAssigned;
        }
    }
}
