using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Seeders
{
    /// <summary>
    /// Seeder de métricas secuenciales del Roadmap ("Mi Camino").
    /// Simula que todos los alumnos registrados avanzan en los 10 niveles del Roadmap en estricto orden cronológico,
    /// respetando la regla pedagógica de que un alumno solo accede al nivel N si obtuvo > 60% de éxito en el nivel N-1.
    /// </summary>
    public static class MetricsDataSeeder
    {
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

            // 1. Recuperación de Entidades (Fetch)
            // A. Todos los alumnos registrados activos
            var students = await context.PersonsWithDisability
                .Where(p => p.IsActive)
                .ToListAsync();

            if (students.Count == 0)
            {
                logger?.LogWarning("MetricsDataSeeder: No se encontraron alumnos registrados en la base de datos.");
                return;
            }

            // B. Las 10 actividades globales del Roadmap ordenadas ascendentemente (1 al 10)
            var roadmapActivities = await context.Activities
                .Where(a => a.IsActive && a.IsTemplate && a.RoadmapOrder != null)
                .OrderBy(a => a.RoadmapOrder)
                .ToListAsync();

            if (roadmapActivities.Count == 0)
            {
                // Fallback: Si no están marcadas como IsTemplate, buscar por RoadmapOrder
                roadmapActivities = await context.Activities
                    .Where(a => a.IsActive && a.RoadmapOrder != null)
                    .OrderBy(a => a.RoadmapOrder)
                    .ToListAsync();
            }

            if (roadmapActivities.Count == 0)
            {
                logger?.LogWarning("MetricsDataSeeder: No se encontraron actividades del Roadmap para sembrar.");
                return;
            }

            // C. Mapa de alumno -> profesional a cargo (a través de ProfessionalPersons o fallback)
            var professionalPersons = await context.ProfessionalPersons
                .Where(pp => pp.IsActive && pp.Person.IsActive && pp.Professional.IsActive)
                .ToListAsync();

            var defaultProf = await context.Professionals.FirstOrDefaultAsync(p => p.IsActive);
            if (defaultProf == null)
            {
                logger?.LogWarning("MetricsDataSeeder: No se encontró ningún profesional activo.");
                return;
            }

            var studentProfMap = new Dictionary<Guid, Guid>();
            foreach (var student in students)
            {
                var pp = professionalPersons.FirstOrDefault(x => x.PersonId == student.Id);
                var profId = pp?.ProfessionalId ?? student.SupervisorUserId ?? defaultProf.Id;
                studentProfMap[student.Id] = profId;
            }

            // 2. Lógica de Simulación Secuencial (Simulation Loop)
            var random = new Random(42); // Seed para reproducibilidad
            var sessions = new List<ActivitySession>();
            var now = DateTime.UtcNow;

            foreach (var student in students)
            {
                var profId = studentProfMap[student.Id];

                // Fecha base inicial para el Nivel 1 (entre 25 y 29 días atrás)
                var currentDate = now.AddDays(-random.Next(25, 30))
                                     .AddHours(random.Next(8, 17))
                                     .AddMinutes(random.Next(0, 60));

                foreach (var activity in roadmapActivities)
                {
                    // Regla de Progresión Estricta:
                    // Ponderación: 80% de las veces supera el 60% (avance), 20% estancamiento (<= 60%)
                    var isSuccess = random.NextDouble() < 0.80;

                    decimal successRate;
                    int errorCount;
                    int gasScore;

                    if (isSuccess)
                    {
                        // Éxito: 61% a 100%
                        successRate = random.Next(61, 101);
                        errorCount = random.Next(0, 4); // 0 a 3 errores
                        gasScore = successRate >= 85 ? random.Next(1, 3) : random.Next(0, 2); // [0, +2]
                    }
                    else
                    {
                        // Estancamiento / Fallo: 30% a 60%
                        successRate = random.Next(30, 61);
                        errorCount = random.Next(4, 11); // 4 a 10 errores
                        gasScore = random.Next(-2, 0); // [-2, -1]
                    }

                    var timeSpent = random.Next(40, 301); // 40 a 300 segundos

                    // Garantizar que la fecha no supere el presente
                    if (currentDate > now)
                    {
                        currentDate = now.AddMinutes(-random.Next(5, 60));
                    }

                    sessions.Add(new ActivitySession
                    {
                        StudentId = student.Id,
                        ProfessionalId = profId,
                        ActivityId = activity.Id,
                        DateCompleted = currentDate,
                        SuccessRate = successRate,
                        ErrorCount = errorCount,
                        TimeSpentSeconds = timeSpent,
                        GasScore = gasScore,
                        CreatedAt = currentDate,
                        IsActive = true,
                    });

                    // Punto de quiebre (Break condition):
                    // Si el éxito es <= 60%, el alumno no desbloquea el siguiente nivel. Detener simulación.
                    if (successRate <= 60)
                    {
                        break;
                    }

                    // Fechas Coherentes (Time Flow):
                    // Avanzar la fecha para el siguiente nivel (+1 a +3 días con horas hábiles)
                    currentDate = currentDate.AddDays(random.Next(1, 4))
                                             .AddHours(random.Next(1, 5))
                                             .AddMinutes(random.Next(0, 60));
                }
            }

            // 4. Persistencia
            await context.ActivitySessions.AddRangeAsync(sessions);
            await context.SaveChangesAsync();

            logger?.LogInformation("MetricsDataSeeder: Se generaron {Count} sesiones secuenciales del Roadmap para {StudentCount} alumnos.", sessions.Count, students.Count);
        }
    }
}
