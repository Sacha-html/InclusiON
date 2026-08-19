using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Analytics;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger<AnalyticsController> _logger;

        private static readonly string[] HighContrastColors =
        [
            "#1A237E", // Azul Marino Intenso
            "#00C853", // Verde Brillante
            "#FF6D00", // Naranja Vivo
            "#FFD600", // Amarillo Oro
            "#6200EA", // Violeta Profundo
            "#00B0FF", // Celeste Eléctrico
            "#D50000"  // Rojo Escarlata
        ];

        public AnalyticsController(
            AppDbContext context,
            IHttpContextService httpContextService,
            ILogger<AnalyticsController> logger)
        {
            _context = context;
            _httpContextService = httpContextService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene métricas analíticas y KPIs para el dashboard del Profesional autenticado.
        /// Opcionalmente filtra por aula específica (aulaId / classroomId) y rango de fechas (desde / hasta).
        /// </summary>
        [HttpGet("professional")]
        [ProducesResponseType(typeof(ApiResponse<AnalyticsDashboardResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AnalyticsDashboardResponse>>> GetProfessionalAnalytics(
            [FromQuery] Guid? aulaId = null,
            [FromQuery] Guid? classroomId = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            CancellationToken cancellationToken = default)
        {
            var targetClassroomId = aulaId ?? classroomId;
            var from = NormalizeToUtc(desde ?? dateFrom, isEndOfDay: false);
            var to = NormalizeToUtc(hasta ?? dateTo, isEndOfDay: true);
            var currentEntityId = _httpContextService.GetCurrentEntityId();
            var currentUserId = _httpContextService.GetCurrentUserId();

            // Buscar profesional
            var professional = await _context.Professionals
                .FirstOrDefaultAsync(p => p.Id == currentEntityId || p.UserId == currentUserId, cancellationToken);

            if (professional == null)
            {
                // Fallback para pruebas si es administrador o no tiene profesional directo
                professional = await _context.Professionals.FirstOrDefaultAsync(p => p.IsActive, cancellationToken);
            }

            if (professional == null)
            {
                return Ok(ApiResponse<AnalyticsDashboardResponse>.SuccessResult(BuildEmptyAnalyticsResponse()));
            }

            // Obtener alumnos del profesional
            var query = _context.ProfessionalPersons
                .Where(pp => pp.ProfessionalId == professional.Id && pp.IsActive && pp.Person.IsActive);

            if (targetClassroomId.HasValue && targetClassroomId.Value != Guid.Empty)
            {
                query = query.Where(pp => pp.ClassroomId == targetClassroomId.Value);
            }

            var studentIds = await query.Select(pp => pp.PersonId).Distinct().ToListAsync(cancellationToken);

            var analytics = await CalculateAnalyticsAsync(studentIds, from, to, cancellationToken);

            // Si se consulta la vista global del profesional ("Todas mis aulas"), calcular el ranking comparativo entre sus aulas
            if (!targetClassroomId.HasValue || targetClassroomId.Value == Guid.Empty)
            {
                var professionalClassrooms = await _context.Classrooms
                    .Where(c => c.ProfessionalId == professional.Id && c.IsActive)
                    .ToListAsync(cancellationToken);

                var rankingList = new List<ClassroomRankingItem>();

                foreach (var classroom in professionalClassrooms)
                {
                    var classroomStudentIds = await _context.ProfessionalPersons
                        .Where(pp => pp.ClassroomId == classroom.Id && pp.IsActive && pp.Person.IsActive)
                        .Select(pp => pp.PersonId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    var classroomSessionsQuery = _context.ActivitySessions
                        .Where(s => classroomStudentIds.Contains(s.StudentId) && s.IsActive);

                    if (from.HasValue)
                        classroomSessionsQuery = classroomSessionsQuery.Where(s => s.DateCompleted >= from.Value);
                    if (to.HasValue)
                        classroomSessionsQuery = classroomSessionsQuery.Where(s => s.DateCompleted <= to.Value);

                    var classroomSessions = await classroomSessionsQuery.ToListAsync(cancellationToken);

                    var avgSuccess = classroomSessions.Count != 0
                        ? Math.Round(classroomSessions.Average(s => s.SuccessRate), 1)
                        : 0m;

                    rankingList.Add(new ClassroomRankingItem
                    {
                        ClassroomId = classroom.Id,
                        NombreAula = classroom.Name,
                        TotalAlumnos = classroomStudentIds.Count,
                        PromedioExitoAula = avgSuccess,
                        TotalSesiones = classroomSessions.Count
                    });
                }

                analytics.RankingMisAulas = rankingList
                    .OrderByDescending(r => r.PromedioExitoAula)
                    .ThenByDescending(r => r.TotalAlumnos)
                    .ToList();
            }

            return Ok(ApiResponse<AnalyticsDashboardResponse>.SuccessResult(analytics));
        }

        /// <summary>
        /// Obtiene el listado detallado de sesiones con alerta de frustración o bloqueo
        /// para el modal de drill-down del profesional con filtro opcional de fechas.
        /// </summary>
        [HttpGet("professional/frustration-details")]
        [ProducesResponseType(typeof(ApiResponse<List<FrustrationDetailResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FrustrationDetailResponse>>>> GetFrustrationDetails(
            [FromQuery] Guid? aulaId = null,
            [FromQuery] Guid? classroomId = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            CancellationToken cancellationToken = default)
        {
            var targetClassroomId = aulaId ?? classroomId;
            var from = NormalizeToUtc(desde ?? dateFrom, isEndOfDay: false);
            var to = NormalizeToUtc(hasta ?? dateTo, isEndOfDay: true);
            var currentEntityId = _httpContextService.GetCurrentEntityId();
            var currentUserId = _httpContextService.GetCurrentUserId();

            var professional = await _context.Professionals
                .FirstOrDefaultAsync(p => p.Id == currentEntityId || p.UserId == currentUserId, cancellationToken);

            if (professional == null)
            {
                professional = await _context.Professionals.FirstOrDefaultAsync(p => p.IsActive, cancellationToken);
            }

            if (professional == null)
            {
                return Ok(ApiResponse<List<FrustrationDetailResponse>>.SuccessResult(new List<FrustrationDetailResponse>()));
            }

            var query = _context.ProfessionalPersons
                .Where(pp => pp.ProfessionalId == professional.Id && pp.IsActive && pp.Person.IsActive);

            if (targetClassroomId.HasValue && targetClassroomId.Value != Guid.Empty)
            {
                query = query.Where(pp => pp.ClassroomId == targetClassroomId.Value);
            }

            var studentIds = await query.Select(pp => pp.PersonId).Distinct().ToListAsync(cancellationToken);

            var defaultSinceDate = DateTime.UtcNow.AddDays(-30);
            var minDate = from ?? defaultSinceDate;

            // Obtener sesiones con indicadores de frustración (éxito <= 60%, errores >= 4 o GAS <= -1)
            var frustrationQuery = _context.ActivitySessions
                .Include(s => s.Student)
                .Include(s => s.Activity)
                    .ThenInclude(a => a.Category)
                .Where(s => studentIds.Contains(s.StudentId) && s.IsActive && s.DateCompleted >= minDate &&
                           (s.SuccessRate <= 60 || s.ErrorCount >= 4 || s.GasScore <= -1));

            if (to.HasValue)
            {
                frustrationQuery = frustrationQuery.Where(s => s.DateCompleted <= to.Value);
            }

            var frustrationSessions = await frustrationQuery
                .OrderByDescending(s => s.DateCompleted)
                .ToListAsync(cancellationToken);

            var result = frustrationSessions.Select(s =>
            {
                var motivos = new List<string>();
                if (s.SuccessRate <= 60) motivos.Add($"Éxito bajo ({s.SuccessRate:F0}%)");
                if (s.ErrorCount >= 4) motivos.Add($"{s.ErrorCount} errores cometidos");
                if (s.GasScore <= -1) motivos.Add($"GAS {s.GasScore}");

                return new FrustrationDetailResponse
                {
                    StudentId = s.StudentId,
                    NombreAlumno = $"{s.Student.FirstName} {s.Student.LastName}".Trim(),
                    ActivityId = s.ActivityId,
                    NombreActividad = s.Activity.Title,
                    CategoriaPedagogica = s.Activity.Category?.Name ?? "General",
                    CantidadErrores = s.ErrorCount,
                    SuccessRate = s.SuccessRate,
                    TimeSpentSeconds = s.TimeSpentSeconds,
                    GasScore = s.GasScore,
                    Fecha = s.DateCompleted,
                    MotivoFrustracion = motivos.Count > 0 ? string.Join(" • ", motivos) : "Bloqueo en nivel"
                };
            }).ToList();

            return Ok(ApiResponse<List<FrustrationDetailResponse>>.SuccessResult(result));
        }

        /// <summary>
        /// Obtiene métricas analíticas globales de toda la institución para el dashboard del Administrador.
        /// Opcionalmente filtra por rango de fechas (desde / hasta).
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Policy = "users:read")]
        [ProducesResponseType(typeof(ApiResponse<AnalyticsDashboardResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AnalyticsDashboardResponse>>> GetAdminAnalytics(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            CancellationToken cancellationToken = default)
        {
            var from = NormalizeToUtc(desde ?? dateFrom, isEndOfDay: false);
            var to = NormalizeToUtc(hasta ?? dateTo, isEndOfDay: true);

            var studentIds = await _context.PersonsWithDisability
                .Where(p => p.IsActive)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var analytics = await CalculateAnalyticsAsync(studentIds, from, to, cancellationToken);
            return Ok(ApiResponse<AnalyticsDashboardResponse>.SuccessResult(analytics));
        }

        /// <summary>
        /// Obtiene analítica documental de reportes y estado del workflow para el Administrador con filtro opcional de fechas.
        /// </summary>
        [HttpGet("admin/reports")]
        [Authorize(Policy = "reports:read")]
        [ProducesResponseType(typeof(ApiResponse<AdminReportsAnalyticsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminReportsAnalyticsResponse>>> GetAdminReportsAnalytics(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            CancellationToken cancellationToken = default)
        {
            var from = NormalizeToUtc(desde ?? dateFrom, isEndOfDay: false);
            var to = NormalizeToUtc(hasta ?? dateTo, isEndOfDay: true);

            var reportsQuery = _context.Reports
                .Include(r => r.Professional)
                .Where(r => r.IsActive);

            if (from.HasValue)
                reportsQuery = reportsQuery.Where(r => r.ReportDate >= from.Value);
            if (to.HasValue)
                reportsQuery = reportsQuery.Where(r => r.ReportDate <= to.Value);

            var reports = await reportsQuery.ToListAsync(cancellationToken);

            var total = reports.Count;
            var pendientes = reports.Count(r => r.Status == ReportStatus.Submitted);
            var aprobados = reports.Count(r => r.Status == ReportStatus.Approved);
            var borradores = reports.Count(r => r.Status == ReportStatus.Draft);
            var rechazados = reports.Count(r => r.Status == ReportStatus.Rejected);

            var tasaRechazo = total > 0 ? Math.Round((decimal)rechazados / total * 100, 1) : 0m;
            var leidos = reports.Count(r => r.Status == ReportStatus.Approved && r.IsReadByFamily);
            var indiceLectura = aprobados > 0 ? Math.Round((decimal)leidos / aprobados * 100, 1) : 0m;

            // Obtener todos los profesionales activos para que se listen automáticamente
            var allProfessionals = await _context.Professionals
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);

            // Ranking de Productividad de Profesionales (por reportes aprobados)
            var rankingProfesionales = allProfessionals
                .Select(prof =>
                {
                    var profReports = reports.Where(r => r.ProfessionalId == prof.Id).ToList();
                    var profName = $"{prof.FirstName} {prof.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(profName)) profName = "Docente";

                    return new ProfessionalReportsProductivityItem
                    {
                        ProfessionalId = prof.Id,
                        NombreProfesional = profName,
                        ReportesAprobados = profReports.Count(r => r.Status == ReportStatus.Approved),
                        TotalReportes = profReports.Count
                    };
                })
                .OrderByDescending(p => p.ReportesAprobados)
                .ThenByDescending(p => p.TotalReportes)
                .ThenBy(p => p.NombreProfesional)
                .ToList();

            // Distribución de estados para Pie Chart con paleta de alto contraste
            var distribucion = new List<ReportStatusDistributionItem>
            {
                new()
                {
                    Estado = "Aprobados",
                    EstadoKey = "Approved",
                    Cantidad = aprobados,
                    Porcentaje = total > 0 ? Math.Round((decimal)aprobados / total * 100, 1) : 0m,
                    Color = "#00C853" // Verde
                },
                new()
                {
                    Estado = "Pendientes de revisión",
                    EstadoKey = "Submitted",
                    Cantidad = pendientes,
                    Porcentaje = total > 0 ? Math.Round((decimal)pendientes / total * 100, 1) : 0m,
                    Color = "#FF6D00" // Naranja alerta
                },
                new()
                {
                    Estado = "Borradores",
                    EstadoKey = "Draft",
                    Cantidad = borradores,
                    Porcentaje = total > 0 ? Math.Round((decimal)borradores / total * 100, 1) : 0m,
                    Color = "#1A237E" // Azul Marino
                },
                new()
                {
                    Estado = "Rechazados",
                    EstadoKey = "Rejected",
                    Cantidad = rechazados,
                    Porcentaje = total > 0 ? Math.Round((decimal)rechazados / total * 100, 1) : 0m,
                    Color = "#D50000" // Rojo
                }
            };

            var response = new AdminReportsAnalyticsResponse
            {
                Pendientes_Revision = pendientes,
                Tasa_Rechazo = tasaRechazo,
                Indice_Lectura_Familiar = indiceLectura,
                Total_Reportes = total,
                Aprobados_Total = aprobados,
                Borradores_Total = borradores,
                Rechazados_Total = rechazados,
                Ranking_Profesionales = rankingProfesionales,
                Distribucion_Estados = distribucion
            };

            return Ok(ApiResponse<AdminReportsAnalyticsResponse>.SuccessResult(response));
        }

        private static DateTime? NormalizeToUtc(DateTime? date, bool isEndOfDay = false)
        {
            if (!date.HasValue) return null;
            var d = date.Value;
            // Validar que el año sea razonable (evita errores cuando el usuario empieza a tipear "0002", etc.)
            if (d.Year < 1900 || d.Year > 3000) return null;

            if (isEndOfDay)
            {
                d = d.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                d = d.Date;
            }

            return DateTime.SpecifyKind(d, DateTimeKind.Utc);
        }

        private async Task<AnalyticsDashboardResponse> CalculateAnalyticsAsync(
            List<Guid> studentIds,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken)
        {
            var response = new AnalyticsDashboardResponse
            {
                PersonasActivas = studentIds.Count
            };

            if (studentIds.Count == 0)
            {
                return BuildEmptyAnalyticsResponse();
            }

            // Obtener sesiones de estos alumnos
            var sessionsQuery = _context.ActivitySessions
                .Include(s => s.Activity)
                    .ThenInclude(a => a.Category)
                .Where(s => studentIds.Contains(s.StudentId) && s.IsActive);

            if (from.HasValue)
            {
                sessionsQuery = sessionsQuery.Where(s => s.DateCompleted >= from.Value);
            }
            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                sessionsQuery = sessionsQuery.Where(s => s.DateCompleted <= toDate);
            }

            var sessions = await sessionsQuery.ToListAsync(cancellationToken);

            // Obtener las 10 actividades del Roadmap
            var roadmapActivities = await _context.Activities
                .Include(a => a.Category)
                .Where(a => a.IsActive && a.RoadmapOrder != null)
                .OrderBy(a => a.RoadmapOrder)
                .ToListAsync(cancellationToken);

            if (sessions.Count == 0)
            {
                response.Distribucion_Por_Nivel = roadmapActivities.Select(a => new LevelDistributionItem
                {
                    Nivel = a.RoadmapOrder ?? 0,
                    NombreActividad = a.Title,
                    AlumnosEstancados = 0,
                    AlumnosSuperaron = 0,
                    TotalAlumnos = 0
                }).ToList();

                response.Tasa_Abandono_Por_Nivel = roadmapActivities.Select(a => new LevelDropoutRateItem
                {
                    Nivel = a.RoadmapOrder ?? 0,
                    NombreActividad = a.Title,
                    AbandonoPct = 0,
                    AlumnosAbandono = 0,
                    TotalIntentos = 0
                }).ToList();

                return response;
            }

            // 1. KPIs Globales
            response.TotalActividadesCompletadas = sessions.Count;
            response.Promedio_GAS = Math.Round((decimal)sessions.Average(s => (decimal)s.GasScore), 2);
            response.Tiempo_Promedio_Nivel = Math.Round(sessions.Average(s => s.TimeSpentSeconds), 1);
            response.PromedioExito = Math.Round(sessions.Average(s => s.SuccessRate), 1);
            response.AlertasFrustracion = sessions.Count(s => s.SuccessRate < 40 || s.GasScore <= -1);

            // 2. Distribución actual por nivel
            // Para cada alumno, encontrar cuál es el nivel más alto alcanzado y su estado
            var studentLevelGroups = sessions
                .GroupBy(s => s.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    MaxLevelPlayed = g.Max(s => s.Activity.RoadmapOrder ?? 1),
                    LastSession = g.OrderByDescending(s => s.DateCompleted).First()
                })
                .ToList();

            var levelDistribution = new List<LevelDistributionItem>();
            var levelDropoutList = new List<LevelDropoutRateItem>();

            var fifteenDaysAgo = DateTime.UtcNow.AddDays(-15);

            foreach (var act in roadmapActivities)
            {
                var levelNum = act.RoadmapOrder ?? 0;
                var sessionsForLevel = sessions.Where(s => s.ActivityId == act.Id || s.Activity.RoadmapOrder == levelNum).ToList();

                var passedCount = sessionsForLevel.Count(s => s.SuccessRate > 60);
                var stuckCount = studentLevelGroups.Count(sl => sl.MaxLevelPlayed == levelNum && sl.LastSession.SuccessRate <= 60);
                var totalStudentsReached = sessionsForLevel.Select(s => s.StudentId).Distinct().Count();

                levelDistribution.Add(new LevelDistributionItem
                {
                    Nivel = levelNum,
                    NombreActividad = act.Title,
                    AlumnosEstancados = stuckCount,
                    AlumnosSuperaron = passedCount,
                    TotalAlumnos = totalStudentsReached
                });

                // Tasa de abandono / estancamiento
                // Alumnos que fallaron el nivel y no volvieron a registrar sesión reciente
                var failedStudentSessions = sessionsForLevel
                    .GroupBy(s => s.StudentId)
                    .Where(g => g.OrderByDescending(s => s.DateCompleted).First().SuccessRate <= 60)
                    .ToList();

                var abandonedCount = failedStudentSessions.Count(g => g.Max(s => s.DateCompleted) < fifteenDaysAgo || g.Count() == 1);
                var totalUniqueLevelStudents = sessionsForLevel.Select(s => s.StudentId).Distinct().Count();
                var dropoutPct = totalUniqueLevelStudents > 0
                    ? Math.Round((decimal)failedStudentSessions.Count * 100m / totalUniqueLevelStudents, 1)
                    : 0;

                levelDropoutList.Add(new LevelDropoutRateItem
                {
                    Nivel = levelNum,
                    NombreActividad = act.Title,
                    AbandonoPct = dropoutPct,
                    AlumnosAbandono = failedStudentSessions.Count,
                    TotalIntentos = sessionsForLevel.Count
                });
            }

            response.Distribucion_Por_Nivel = levelDistribution;
            response.Tasa_Abandono_Por_Nivel = levelDropoutList;

            // 3. Avance vs Tiempo Estimado
            var withinTimeCount = sessions.Count(s =>
            {
                var estimatedSeconds = (s.Activity.EstimatedDurationMinutes ?? 3) * 60;
                return s.TimeSpentSeconds <= estimatedSeconds;
            });
            var moreTimeCount = sessions.Count - withinTimeCount;

            response.Avance_Tiempo_Estimado = new TimeProgressItem
            {
                TotalSesionesAnalizadas = sessions.Count,
                DentroDeTiempoPct = Math.Round((decimal)withinTimeCount * 100m / sessions.Count, 1),
                MasTiempoPct = Math.Round((decimal)moreTimeCount * 100m / sessions.Count, 1)
            };

            // 4. Rendimiento por Categoría Pedagógica
            var categoryGroups = sessions
                .GroupBy(s => s.Activity.Category?.Name ?? "General")
                .Select((g, idx) => new CategoryPerformanceItem
                {
                    Categoria = g.Key,
                    PromedioExito = Math.Round(g.Average(s => s.SuccessRate), 1),
                    TotalSesiones = g.Count(),
                    Color = HighContrastColors[idx % HighContrastColors.Length]
                })
                .OrderByDescending(c => c.TotalSesiones)
                .ToList();

            response.Rendimiento_Por_Categoria = categoryGroups;

            return response;
        }

        private static AnalyticsDashboardResponse BuildEmptyAnalyticsResponse()
        {
            return new AnalyticsDashboardResponse
            {
                Promedio_GAS = 0,
                Tiempo_Promedio_Nivel = 0,
                PersonasActivas = 0,
                TotalActividadesCompletadas = 0,
                PromedioExito = 0,
                AlertasFrustracion = 0,
                Distribucion_Por_Nivel = Enumerable.Range(1, 10).Select(n => new LevelDistributionItem
                {
                    Nivel = n,
                    NombreActividad = $"Nivel {n}",
                    AlumnosEstancados = 0,
                    AlumnosSuperaron = 0,
                    TotalAlumnos = 0
                }).ToList(),
                Tasa_Abandono_Por_Nivel = Enumerable.Range(1, 10).Select(n => new LevelDropoutRateItem
                {
                    Nivel = n,
                    NombreActividad = $"Nivel {n}",
                    AbandonoPct = 0,
                    AlumnosAbandono = 0,
                    TotalIntentos = 0
                }).ToList(),
                Avance_Tiempo_Estimado = new TimeProgressItem
                {
                    DentroDeTiempoPct = 0,
                    MasTiempoPct = 0,
                    TotalSesionesAnalizadas = 0
                },
                Rendimiento_Por_Categoria = new List<CategoryPerformanceItem>()
            };
        }
    }
}
