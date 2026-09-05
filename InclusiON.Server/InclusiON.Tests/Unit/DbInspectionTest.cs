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
        public async Task InspectDatabaseUsers()
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

            var roles = await context.Roles.ToListAsync();
            var users = await context.Users.ToListAsync();
            var userRoles = await context.UserRoles.ToListAsync();
            var persons = await context.PersonsWithDisability.Include(p => p.LoginMethod).ToListAsync();
            var professionals = await context.Professionals.ToListAsync();
            var families = await context.FamilyRepresentatives.ToListAsync();
            var loginMethods = await context.LoginMethods.ToListAsync();

            _output.WriteLine("=== ROLES EN DB ===");
            foreach (var r in roles)
            {
                _output.WriteLine($"Role: {r.Name} (Id: {r.Id})");
            }

            _output.WriteLine("\n=== METODOS DE LOGIN EN DB ===");
            foreach (var lm in loginMethods)
            {
                _output.WriteLine($"Id: {lm.Id}, Name: {lm.Name}, Code: {lm.Code}, IsActive: {lm.IsActive}");
            }

            _output.WriteLine("\n=== USUARIOS EN DB (AspNetUsers) ===");
            foreach (var u in users)
            {
                var roleIds = userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleId).ToList();
                var userRoleNames = roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();
                var roleStr = string.Join(", ", userRoleNames);

                var person = persons.FirstOrDefault(p => p.UserId == u.Id);
                var prof = professionals.FirstOrDefault(p => p.UserId == u.Id);
                var fam = families.FirstOrDefault(f => f.UserId == u.Id);

                string detail = "";
                if (prof != null)
                {
                    detail = $"[PROFESIONAL] Especialidad: {prof.Specialty}, Matrícula: {prof.LicenseNumber}, Estado: {prof.Status}, Activo: {prof.IsActive}";
                }
                else if (fam != null)
                {
                    detail = $"[FAMILIAR] Parentesco: {fam.Relationship}, Teléfono: {fam.Phone}, DNI: {fam.DocumentNumber}";
                }
                else if (person != null)
                {
                    detail = $"[PERSONA/ALUMNO] DNI: {person.DocumentNumber}, Método Login: {person.LoginMethod?.Name} (Id: {person.LoginMethodId}), Tiene PIN: {person.PinCodeHash != null}, SupervisorId: {person.SupervisorUserId}";
                }

                _output.WriteLine($"USER: {u.Email} | Nombre: {u.Name} {u.Surname} | Activo: {u.IsActive} | Roles: [{roleStr}] | Detalle: {detail}");
            }
        }
    }
}
