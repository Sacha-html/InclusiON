using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Seeders
{
    /// <summary>
    /// Seeder de datos analíticos y métricas simuladas sobre las actividades existentes.
    /// Puebla la tabla ActivitySessions con 100 a 200 registros distribuidos en los últimos 30 días
    /// para alimentar dashboards de KPIs de Profesionales y Administradores.
    /// </summary>
    public static class MetricsDataSeeder
    {
        private static readonly int[] GasScores = [-2, -1, 0, 1, 2];

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("MetricsDataSeeder");

            // Solo sembrar si la tabla de sesiones de métricas está vacía
            if (await context.ActivitySessions.AnyAsync())
            {
                logger?.LogInformation("MetricsDataSeeder: La tabla ActivitySessions ya contiene datos. Seeding omitido.");
                return;
            }

            // Paso A (Fetch): Obtener Actividades, Alumnos y Profesionales
            // 1. Actividades reales de profesionales (excluyendo plantillas de roadmap) o todas las actividades activas
            var activities = await context.Activities
                .Where(a => a.IsActive && !a.IsTemplate)
                .ToListAsync();

            if (activities.Count == 0)
            {
                // Fallback si no hay actividades de profesionales: usar cualquier actividad activa
                activities = await context.Activities.Where(a => a.IsActive).ToListAsync();
            }

            if (activities.Count == 0)
            {
                logger?.LogWarning("MetricsDataSeeder: No se encontraron actividades activas en la base de datos.");
                return;
            }

            // 2. Alumnos con sus relaciones de profesionales o aulas
            var studentProfessionalPairs = await context.ProfessionalPersons
                .Where(pp => pp.IsActive && pp.Person.IsActive && pp.Professional.IsActive)
                .Select(pp => new { StudentId = pp.PersonId, ProfessionalId = pp.ProfessionalId })
                .Distinct()
                .ToListAsync();

            // Fallback: Si no hay pares en ProfessionalPersons, emparejar alumnos con su supervisor o el primer profesional
            if (studentProfessionalPairs.Count == 0)
            {
                var students = await context.PersonsWithDisability.Where(p => p.IsActive).ToListAsync();
                var defaultProf = await context.Professionals.FirstOrDefaultAsync(p => p.IsActive);

                if (students.Count == 0 || defaultProf == null)
                {
                    logger?.LogWarning("MetricsDataSeeder: No se encontraron alumnos o profesionales activos para sembrar métricas.");
                    return;
                }

                studentProfessionalPairs = students
                    .Select(s => new { StudentId = s.Id, ProfessionalId = s.SupervisorUserId ?? defaultProf.Id })
                    .ToList();
            }

            // Paso B (Loop): Generar entre 100 y 200 registros de sesiones ficticias
            var random = new Random(42); // Seed fija para reproducibilidad uniforme
            var totalSessions = random.Next(120, 180); // Entre 120 y 180 registros
            var sessions = new List<ActivitySession>(totalSessions);
            var now = DateTime.UtcNow;

            for (int i = 0; i < totalSessions; i++)
            {
                // Paso C (Randomization)
                var pair = studentProfessionalPairs[random.Next(studentProfessionalPairs.Count)];
                var activity = activities[random.Next(activities.Count)];

                // Fechas distribuidas uniformemente en los últimos 30 días
                var daysAgo = random.Next(0, 30);
                var minutesAgo = random.Next(0, 1440);
                var dateCompleted = now.AddDays(-daysAgo).AddMinutes(-minutesAgo);

                // SuccessRate: entre 40% y 100%
                var successRate = (decimal)random.Next(40, 101);

                // ErrorCount: entre 0 y 6
                var errorCount = random.Next(0, 7);

                // TimeSpentSeconds: entre 30 y 300 segundos
                var timeSpent = random.Next(30, 301);

                // GasScore: valor cualitativo (-2, -1, 0, 1, 2)
                var gasScore = GasScores[random.Next(GasScores.Length)];

                sessions.Add(new ActivitySession
                {
                    StudentId = pair.StudentId,
                    ProfessionalId = pair.ProfessionalId,
                    ActivityId = activity.Id,
                    DateCompleted = dateCompleted,
                    SuccessRate = successRate,
                    ErrorCount = errorCount,
                    TimeSpentSeconds = timeSpent,
                    GasScore = gasScore,
                    CreatedAt = dateCompleted,
                    IsActive = true,
                });
            }

            await context.ActivitySessions.AddRangeAsync(sessions);
            await context.SaveChangesAsync();

            logger?.LogInformation("MetricsDataSeeder: Se generaron e insertaron exitosamente {Count} sesiones de métricas en ActivitySessions.", sessions.Count);
        }
    }
}
