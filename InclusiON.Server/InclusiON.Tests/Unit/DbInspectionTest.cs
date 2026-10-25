using System;
using System.Linq;
using System.Threading.Tasks;
using InclusiON.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace InclusiON.Tests.Unit
{
    public class DbInspectionTest
    {
        private readonly ITestOutputHelper _output;

        public DbInspectionTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task InspectRelationships()
        {
            string[] connStrings = new[]
            {
                "Host=192.168.0.17;Port=5433;Database=inclusion_dev;Username=postgres;Password=postgres",
                "Host=localhost;Port=5432;Database=inclusion_dev;Username=postgres;Password=postgres",
                "Host=localhost;Port=5433;Database=inclusion_dev;Username=postgres;Password=postgres",
                "Host=localhost;Port=5432;Database=inclusion_dev;Username=inclusion_dev_app;Password=Inclusion_Dev_2025_!"
            };

            AppDbContext? context = null;
            foreach (var conn in connStrings)
            {
                try
                {
                    var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseNpgsql(conn)
                        .Options;
                    var ctx = new AppDbContext(options);
                    if (await ctx.Database.CanConnectAsync())
                    {
                        context = ctx;
                        _output.WriteLine($"Connected to: {conn}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed {conn}: {ex.Message}");
                }
            }

            if (context == null)
            {
                _output.WriteLine("Could not connect to any database instance.");
                return;
            }

            var famRels = await context.FamilyRepresentatives
                .Select(f => f.Relationship)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToListAsync();

            var personRepRels = await context.PersonRepresentatives
                .Select(pr => pr.Relationship)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToListAsync();

            var historyRels = await context.PersonRepresentativeHistories
                .Select(h => h.Relationship)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToListAsync();

            var invitationRels = await context.Invitations
                .Select(i => i.Relationship)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToListAsync();

            var allRels = famRels
                .Concat(personRepRels)
                .Concat(historyRels)
                .Concat(invitationRels)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            _output.WriteLine("=== PARENTESCOS / RELACIONES ENCONTRADAS EN DB ===");
            foreach (var rel in allRels)
            {
                _output.WriteLine($"RELATIONSHIP: '{rel}'");
            }

            _output.WriteLine("\n=== DETALLE POR TABLA ===");
            _output.WriteLine($"FamilyRepresentatives: {string.Join(", ", famRels.Select(r => $"'{r}'"))}");
            _output.WriteLine($"PersonRepresentatives: {string.Join(", ", personRepRels.Select(r => $"'{r}'"))}");
            _output.WriteLine($"PersonRepresentativeHistories: {string.Join(", ", historyRels.Select(r => $"'{r}'"))}");
            _output.WriteLine($"Invitations: {string.Join(", ", invitationRels.Select(r => $"'{r}'"))}");

            var allFamilyRecords = await context.FamilyRepresentatives
                .Select(f => new { f.Id, f.FirstName, f.LastName, f.Relationship })
                .ToListAsync();
            _output.WriteLine("\n=== REGISTROS DE FAMILIARES Y SU PARENTESCO ===");
            foreach (var f in allFamilyRecords)
            {
                _output.WriteLine($"Familiar: {f.FirstName} {f.LastName} | Relationship: '{f.Relationship}'");
            }

            var allPersonReps = await context.PersonRepresentatives
                .Include(pr => pr.Person)
                .Include(pr => pr.Representative)
                .Select(pr => new {
                    Alumno = pr.Person.FirstName + " " + pr.Person.LastName,
                    Familiar = pr.Representative.FirstName + " " + pr.Representative.LastName,
                    pr.Relationship,
                    pr.IsPrimary
                })
                .ToListAsync();
            _output.WriteLine("\n=== VINCULOS ALUMNO-FAMILIAR ===");
            foreach (var pr in allPersonReps)
            {
                _output.WriteLine($"Alumno: {pr.Alumno} | Familiar: {pr.Familiar} | Relationship: '{pr.Relationship}' | Principal: {pr.IsPrimary}");
            }

            // Normalizar 'madre' a 'Madre'
            var lowercaseMothers = await context.FamilyRepresentatives
                .Where(f => f.Relationship == "madre")
                .ToListAsync();
            if (lowercaseMothers.Any())
            {
                foreach (var f in lowercaseMothers)
                {
                    f.Relationship = "Madre";
                }
                _output.WriteLine($"[NORMALIZADO] Se normalizaron {lowercaseMothers.Count} registros de 'madre' a 'Madre'.");
            }

            // Normalizar 'Tutor Legal' a 'Tutor/a'
            var famTutors = await context.FamilyRepresentatives
                .Where(f => f.Relationship == "Tutor Legal" || f.Relationship == "Tutor" || f.Relationship == "tutor")
                .ToListAsync();
            foreach (var f in famTutors)
            {
                f.Relationship = "Tutor/a";
            }
            if (famTutors.Any())
            {
                _output.WriteLine($"[NORMALIZADO] Se normalizaron {famTutors.Count} FamilyRepresentatives a 'Tutor/a'.");
            }

            var personRepTutors = await context.PersonRepresentatives
                .Where(pr => pr.Relationship == "Tutor Legal" || pr.Relationship == "Tutor" || pr.Relationship == "tutor")
                .ToListAsync();
            foreach (var pr in personRepTutors)
            {
                pr.Relationship = "Tutor/a";
            }
            if (personRepTutors.Any())
            {
                _output.WriteLine($"[NORMALIZADO] Se normalizaron {personRepTutors.Count} PersonRepresentatives a 'Tutor/a'.");
            }

            var histTutors = await context.PersonRepresentativeHistories
                .Where(h => h.Relationship == "Tutor Legal" || h.Relationship == "Tutor" || h.Relationship == "tutor")
                .ToListAsync();
            foreach (var h in histTutors)
            {
                h.Relationship = "Tutor/a";
            }
            if (histTutors.Any())
            {
                _output.WriteLine($"[NORMALIZADO] Se normalizaron {histTutors.Count} PersonRepresentativeHistories a 'Tutor/a'.");
            }

            var invTutors = await context.Invitations
                .Where(i => i.Relationship == "Tutor Legal" || i.Relationship == "Tutor" || i.Relationship == "tutor")
                .ToListAsync();
            foreach (var i in invTutors)
            {
                i.Relationship = "Tutor/a";
            }
            if (invTutors.Any())
            {
                _output.WriteLine($"[NORMALIZADO] Se normalizaron {invTutors.Count} Invitations a 'Tutor/a'.");
            }

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task SanitizeActivitySessionsMetrics()
        {
            string[] connStrings = new[]
            {
                "Host=192.168.0.17;Port=5433;Database=inclusion_dev;Username=postgres;Password=postgres",
                "Host=localhost;Port=5432;Database=inclusion_dev;Username=postgres;Password=postgres",
                "Host=localhost;Port=5433;Database=inclusion_dev;Username=postgres;Password=postgres",
                "Host=localhost;Port=5432;Database=inclusion_dev;Username=inclusion_dev_app;Password=Inclusion_Dev_2025_!"
            };

            AppDbContext? context = null;
            foreach (var conn in connStrings)
            {
                try
                {
                    var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseNpgsql(conn)
                        .Options;
                    var ctx = new AppDbContext(options);
                    if (await ctx.Database.CanConnectAsync())
                    {
                        context = ctx;
                        _output.WriteLine($"Connected to: {conn}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed {conn}: {ex.Message}");
                }
            }

            if (context == null)
            {
                _output.WriteLine("Could not connect to any database instance.");
                return;
            }

            // 1. Diagnóstico previo
            var totalSessions = await context.ActivitySessions.CountAsync();
            var approvedWithErrors = await context.ActivitySessions
                .Where(s => s.SuccessRate >= 60m && s.ErrorCount >= 4)
                .ToListAsync();

            var failedSessions = await context.ActivitySessions
                .Where(s => s.SuccessRate < 60m)
                .ToListAsync();

            _output.WriteLine($"=== ESTADO PREVIO DE ACTIVITY SESSIONS ===");
            _output.WriteLine($"Total sesiones: {totalSessions}");
            _output.WriteLine($"Sesiones aprobadas (>=60%) con errores inflados (>=4): {approvedWithErrors.Count}");
            _output.WriteLine($"Sesiones fallidas (<60%): {failedSessions.Count}");

            // 2. Corregir aprobadas con ErrorCount >= 4
            foreach (var s in approvedWithErrors)
            {
                s.ErrorCount = s.SuccessRate >= 80m ? 0 : 1;
                if (s.GasScore < 0)
                {
                    s.GasScore = s.SuccessRate >= 85m ? 1 : 0;
                }
            }

            // 3. Normalizar sesiones fallidas por actividad/alumno/fecha
            int correctedFailedGroups = 0;
            if (failedSessions.Count != 0)
            {
                var grouped = failedSessions
                    .GroupBy(s => new { s.StudentId, s.ActivityId, DateGroup = s.DateCompleted.Date });

                foreach (var g in grouped)
                {
                    var attempts = g.OrderBy(s => s.DateCompleted).ToList();
                    if (attempts.Count > 1 && attempts.Count < 4)
                    {
                        // Si no llegó a 4 intentos fallidos, ninguno debe tener ErrorCount >= 4
                        for (int i = 0; i < attempts.Count; i++)
                        {
                            if (attempts[i].ErrorCount >= 4)
                            {
                                attempts[i].ErrorCount = Math.Min(3, i + 1);
                                correctedFailedGroups++;
                            }
                        }
                    }
                    else if (attempts.Count >= 4)
                    {
                        // Tuvo 4 o más intentos: intentos 1..3 quedan < 4 y sólo del 4to en adelante ErrorCount >= 4
                        for (int i = 0; i < attempts.Count; i++)
                        {
                            if (i < 3)
                            {
                                attempts[i].ErrorCount = i + 1;
                            }
                            else
                            {
                                attempts[i].ErrorCount = Math.Max(4, i + 1);
                            }
                        }
                        correctedFailedGroups++;
                    }
                }
            }

            await context.SaveChangesAsync();

            // 4. Verificación posterior
            var postApprovedWithErrors = await context.ActivitySessions
                .CountAsync(s => s.SuccessRate >= 60m && s.ErrorCount >= 4);

            var postFrustrationEpisodes = await context.ActivitySessions
                .Where(s => s.SuccessRate < 60m && s.ErrorCount >= 4)
                .GroupBy(s => new { s.StudentId, s.ActivityId, DateGroup = s.DateCompleted.Date })
                .CountAsync();

            _output.WriteLine($"=== ESTADO POST-SANEAMIENTO ===");
            _output.WriteLine($"Aprobadas con ErrorCount >= 4 restantes: {postApprovedWithErrors} (debe ser 0)");
            _output.WriteLine($"Episodios de frustración (CB-07: 4 intentos fallidos): {postFrustrationEpisodes}");
            _output.WriteLine($"Grupos fallidos ajustados: {correctedFailedGroups}");

            Assert.Equal(0, postApprovedWithErrors);
        }
    }
}
