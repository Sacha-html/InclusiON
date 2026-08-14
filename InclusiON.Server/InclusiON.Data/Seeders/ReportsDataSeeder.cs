using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Seeders
{
    /// <summary>
    /// Generador de datos relacionales (Mock Data) para el módulo de Reportes.
    /// Distribuye 5 a 10 reportes por alumno simulando el último año y respetando
    /// el flujo de aprobación (Borrador, Pendiente, Aprobado y Rechazado).
    /// </summary>
    public static class ReportsDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("ReportsDataSeeder");

            // Si ya hay más de 50 reportes registrados, omitir para evitar duplicados
            if (await context.Reports.CountAsync() >= 50)
            {
                logger?.LogInformation("ReportsDataSeeder: La tabla Reports ya cuenta con {Count} registros. Seeding omitido.", await context.Reports.CountAsync());
                return;
            }

            // 1. Fetch de entidades reales existentes
            var students = await context.PersonsWithDisability
                .Where(p => p.IsActive)
                .ToListAsync();

            if (students.Count == 0)
            {
                logger?.LogWarning("ReportsDataSeeder: No se encontraron alumnos registrados en la base de datos.");
                return;
            }

            var professionals = await context.Professionals
                .Where(p => p.IsActive)
                .ToListAsync();

            if (professionals.Count == 0)
            {
                logger?.LogWarning("ReportsDataSeeder: No se encontraron profesionales activos.");
                return;
            }

            var reportTypes = await context.ReportTypes
                .Where(rt => rt.IsActive)
                .ToListAsync();

            if (reportTypes.Count == 0)
            {
                logger?.LogWarning("ReportsDataSeeder: No se encontraron tipos de reporte configurados.");
                return;
            }

            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@test.com");

            var adminId = adminUser?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Obtener asignaciones Alumno -> Profesional (agrupando por PersonId para soportar múltiples asignaciones)
            var professionalPersons = await context.ProfessionalPersons
                .Where(pp => pp.IsActive && pp.Person.IsActive)
                .ToListAsync();

            var studentProfessionalMap = professionalPersons
                .GroupBy(pp => pp.PersonId)
                .ToDictionary(g => g.Key, g => g.First().ProfessionalId);

            var random = new Random(42); // Semilla determinista para consistencia
            var now = DateTime.UtcNow;
            var mockReports = new List<Report>();

            var rejectionComments = new[]
            {
                "Falta detallar el apartado de Áreas a Reforzar con observaciones concretas del aula.",
                "Corregir fechas del período: el rango no coincide con el trimestre evaluado.",
                "Ampliar los objetivos alcanzados en el área de comunicación y uso de pictogramas / SAAC.",
                "Revisar la coherencia de las recomendaciones futuras con el plan pedagógico individual (PPI).",
                "Se solicita adjuntar observaciones sobre la autorregulación y adaptación sensorial."
            };

            var sampleContents = new[]
            {
                "Durante el presente período evaluado, el estudiante demostró una notable evolución en el seguimiento de rutinas escolares y en la interacción con el sistema digital de comunicación aumentativa. Logró completar las actividades asignadas con autonomía y buena disposición ante los desafíos propuestos.",
                "Se observa un progreso sostenido en la discriminación visual y categorización cognitiva. Responde favorablemente a los estímulos visuales y auditivos de alto contraste, logrando mantener la concentración durante lapsos de 15 a 20 minutos sin signos de fatiga o desregulación.",
                "El estudiante ha participado activamente en las dinámicas pedagógicas adaptadas. Presenta avances significativos en la expresión de necesidades básicas mediante pictogramas y en la coordinación visomotriz fina sobre la interfaz táctil.",
                "Evaluación integral del proceso de inclusión educativa. Se destaca su entusiasmo en la superación de los niveles del Roadmap pedagógico ('Mi Camino') y su integración social positiva con pares y equipo docente.",
                "Informe periódico de desempeño. Se han implementado estrategias de apoyo sensorial que disminuyeron los episodios de sobrecarga, permitiendo una mayor estabilidad emocional y rendimiento académico constante."
            };

            var sampleAchievedGoals = new[]
            {
                "• Reconocimiento consistente de pictogramas de necesidades básicas y estados de ánimo.\n• Mayor tiempo de atención sostenida en actividades de clasificación visual (15+ min).\n• Autonomía en la selección de opciones en la tablet sin requerir asistencia física.",
                "• Superación secuencial de los primeros niveles del Roadmap pedagógico con más del 80% de éxito.\n• Disminución de la latencia de respuesta ante estímulos auditivos de retroalimentación.",
                "• Identificación correcta de figuras geométricas y secuencias de colores primarios.\n• Expresión espontánea de preferencias y pausas durante las sesiones de aprendizaje."
            };

            var sampleAreasToReinforce = new[]
            {
                "• Tolerancia a la frustración ante secuencias complejas de más de 4 pasos.\n• Coordinación visomotriz en el arrastre táctil continuo (drag & drop).\n• Afianzamiento del vocabulario receptivo en situaciones grupales abiertas.",
                "• Transición entre actividades de alta estimulación hacia tareas de concentración tranquila.\n• Autogestión del tiempo ante ejercicios con límite temporal programado."
            };

            var sampleFutureRecommendations = new[]
            {
                "• Continuar el refuerzo en el hogar utilizando la agenda visual diaria con pictogramas de alto contraste.\n• Realizar pausas sensoriales breves cada 15 minutos de trabajo continuo.\n• Mantener una retroalimentación positiva verbal y visual inmediata ante cada acierto.",
                "• Promover el uso del comunicador SAAC en situaciones cotidianas de juego y alimentación.\n• Coordinar con la fonoaudióloga y terapeuta ocupacional los apoyos motrices recomendados."
            };

            var sampleNextObjectives = new[]
            {
                "• Introducir secuencias de 3 y 4 pasos en el Roadmap pedagógico.\n• Fortalecer el vocabulario receptivo en el entorno escolar y comunitario.\n• Desarrollar mayor autonomía en el inicio y cierre de actividades individuales.",
                "• Consolidar el reconocimiento de fonemas iniciales y conceptos espaciales básicos.\n• Incrementar la tolerancia a cambios imprevistos en la rutina del aula."
            };

            // 2. Bucle de generación por alumno
            foreach (var student in students)
            {
                // Determinar profesional a cargo o fallback
                var profId = studentProfessionalMap.TryGetValue(student.Id, out var mappedProfId)
                    ? mappedProfId
                    : professionals[random.Next(professionals.Count)].Id;

                // Generar entre 5 y 8 reportes por alumno a lo largo del último año
                int reportsCount = random.Next(5, 9);

                for (int i = 0; i < reportsCount; i++)
                {
                    // Distribución Ponderada de Estados:
                    // 60% Approved (0-59), 20% Submitted (60-79), 10% Draft (80-89), 10% Rejected (90-99)
                    int roll = random.Next(100);
                    ReportStatus status;
                    DateTime reportDate;
                    DateTime? approvedAt = null;
                    Guid? approvedBy = null;
                    string? adminComment = null;
                    bool isReadByFamily = false;

                    if (roll < 60)
                    {
                        // Approved: histórico (entre 30 y 360 días atrás)
                        status = ReportStatus.Approved;
                        reportDate = now.AddDays(-random.Next(30, 360)).AddHours(-random.Next(1, 10));
                        approvedAt = reportDate.AddDays(random.Next(1, 5));
                        approvedBy = adminId;
                        isReadByFamily = random.Next(100) < 80; // 80% leídos por la familia
                    }
                    else if (roll < 80)
                    {
                        // Submitted: reciente esperando revisión (entre 4 y 25 días atrás)
                        status = ReportStatus.Submitted;
                        reportDate = now.AddDays(-random.Next(4, 25)).AddHours(-random.Next(1, 8));
                    }
                    else if (roll < 90)
                    {
                        // Draft: muy reciente en construcción (últimos 1 a 3 días)
                        status = ReportStatus.Draft;
                        reportDate = now.AddDays(-random.Next(0, 3)).AddHours(-random.Next(1, 6));
                    }
                    else
                    {
                        // Rejected: devuelto con comentario obligatorio (entre 5 y 60 días atrás)
                        status = ReportStatus.Rejected;
                        reportDate = now.AddDays(-random.Next(5, 60)).AddHours(-random.Next(1, 8));
                        adminComment = rejectionComments[random.Next(rejectionComments.Length)];
                    }

                    // Tipo de reporte
                    var reportType = reportTypes[random.Next(reportTypes.Count)];

                    // Fechas de período
                    DateTime? periodEndDate = reportDate.Date;
                    DateTime? periodStartDate;

                    if (reportType.Name.Contains("Trimestral", StringComparison.OrdinalIgnoreCase))
                    {
                        periodStartDate = periodEndDate.Value.AddMonths(-3);
                    }
                    else if (reportType.Name.Contains("Mensual", StringComparison.OrdinalIgnoreCase))
                    {
                        periodStartDate = periodEndDate.Value.AddMonths(-1);
                    }
                    else if (reportType.Name.Contains("Anual", StringComparison.OrdinalIgnoreCase))
                    {
                        periodStartDate = periodEndDate.Value.AddYears(-1);
                    }
                    else
                    {
                        periodStartDate = periodEndDate.Value.AddMonths(-2);
                    }

                    // Título autogenerado
                    var monthName = reportDate.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
                    var title = $"{reportType.Name} - {char.ToUpper(monthName[0]) + monthName.Substring(1)}";

                    // Contenido
                    var content = sampleContents[random.Next(sampleContents.Length)];

                    // Campos cualitativos opcionales (70% de probabilidad de estar completos)
                    bool hasOptionalFields = random.Next(100) < 70;
                    string? achievedGoals = hasOptionalFields ? sampleAchievedGoals[random.Next(sampleAchievedGoals.Length)] : null;
                    string? areasToReinforce = hasOptionalFields ? sampleAreasToReinforce[random.Next(sampleAreasToReinforce.Length)] : null;
                    string? futureRecommendations = hasOptionalFields ? sampleFutureRecommendations[random.Next(sampleFutureRecommendations.Length)] : null;
                    string? nextObjectives = hasOptionalFields ? sampleNextObjectives[random.Next(sampleNextObjectives.Length)] : null;

                    var report = new Report
                    {
                        PersonId = student.Id,
                        ProfessionalId = profId,
                        ReportTypeId = reportType.Id,
                        Title = title,
                        Content = content,
                        ReportDate = reportDate,
                        PeriodStartDate = periodStartDate,
                        PeriodEndDate = periodEndDate,
                        AchievedGoals = achievedGoals,
                        AreasToReinforce = areasToReinforce,
                        FutureRecommendations = futureRecommendations,
                        NextObjectives = nextObjectives,
                        Status = status,
                        AdminComment = adminComment,
                        ApprovedAt = approvedAt,
                        ApprovedBy = approvedBy,
                        IsReadByFamily = isReadByFamily,
                        IsActive = true,
                        CreatedAt = reportDate,
                        UpdatedAt = approvedAt ?? reportDate
                    };

                    mockReports.Add(report);
                }
            }

            // 3. Persistencia en Base de Datos
            context.Reports.AddRange(mockReports);
            await context.SaveChangesAsync();

            logger?.LogInformation(
                "ReportsDataSeeder: Se generaron exitosamente {TotalReports} reportes para {StudentCount} alumnos con distribución de estados (60% Aprobados, 20% Enviados, 10% Borradores, 10% Rechazados).",
                mockReports.Count,
                students.Count);
        }
    }
}
