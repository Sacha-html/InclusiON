using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using System.Text.Json;

namespace InclusiON.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedAdminUserAsync(userManager);
            await SeedProfessionalsAsync(userManager, context);
            await SeedFamilyAsync(userManager, context);
            await SeedVisualLoginTestUsersAsync(userManager, context);
        }

        private static async Task SeedAdminUserAsync(UserManager<User> userManager)
        {
            const string adminEmail = "admin@inclusion.com";
            const string adminPassword = "Admin123!";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin != null)
                return;

            var adminUser = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Admin",
                Surname = "Sistema",
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, IdentityRoles.Admin.ToString());
            }
        }

        private static async Task SeedVisualLoginTestUsersAsync(UserManager<User> userManager, AppDbContext context)
        {
            // Usuarios de prueba para visual login - solo metodos activos (STANDARD=1, PIN=2, ASSISTED=5)
            var testUsers = new[]
            {
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Name = "Maria",
                    Surname = "Garcia",
                    Email = "maria@test.com",
                    Password = "Maria123!",
                    Pin = "1234",
                    LoginMethodId = 2, // PIN
                    AvatarColor = "#4CAF50",
                    SupervisorUserId = (Guid?)null
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "Juan",
                    Surname = "Lopez",
                    Email = "juan@test.com",
                    Password = "Juan123!",
                    Pin = (string?)null,
                    LoginMethodId = 1, // STANDARD (password)
                    AvatarColor = "#2196F3",
                    SupervisorUserId = (Guid?)null
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                    Name = "Ana",
                    Surname = "Martinez",
                    Email = "ana@test.com",
                    Password = (string?)null,
                    Pin = (string?)null,
                    LoginMethodId = 5, // ASSISTED (requiere supervisor)
                    AvatarColor = "#9C27B0",
                    SupervisorUserId = (Guid?)Guid.Parse("00000000-0000-0000-0000-000000000020") // Supervisor
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                    Name = "Carlos",
                    Surname = "Rodriguez",
                    Email = "carlos@test.com",
                    Password = "Carlos123!",
                    Pin = "5678",
                    LoginMethodId = 2, // PIN
                    AvatarColor = "#FF9800",
                    SupervisorUserId = (Guid?)null
                }
            };

            foreach (var testUser in testUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(testUser.Email);

                if (existingUser != null)
                {
                    // Usuario existe - actualizar PersonWithDisability con método de login correcto
                    var existingPerson = await context.PersonsWithDisability
                        .FirstOrDefaultAsync(p => p.UserId == existingUser.Id);

                    if (existingPerson != null)
                    {
                        existingPerson.LoginMethodId = testUser.LoginMethodId;
                        existingPerson.AvatarColor = testUser.AvatarColor;
                        existingPerson.SupervisorUserId = testUser.SupervisorUserId;

                        // Actualizar PIN si tiene
                        if (testUser.Pin != null)
                        {
                            existingPerson.PinCodeHash = BCrypt.Net.BCrypt.HashPassword(testUser.Pin);
                        }
                        else
                        {
                            existingPerson.PinCodeHash = null;
                        }
                    }
                    continue;
                }

                var user = new User
                {
                    Id = testUser.Id,
                    Name = testUser.Name,
                    Surname = testUser.Surname,
                    Email = testUser.Email,
                    UserName = testUser.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    LockoutEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Password por defecto si no tiene uno especifico
                var password = testUser.Password ?? "Test123!";
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, IdentityRoles.PersonWithDisability.ToString());

                    // Crear PersonWithDisability
                    var person = new PersonWithDisability
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        FirstName = testUser.Name,
                        LastName = testUser.Surname,
                        BirthDate = DateTime.UtcNow.AddYears(-25),
                        LoginMethodId = testUser.LoginMethodId,
                        AvatarColor = testUser.AvatarColor,
                        SupervisorUserId = testUser.SupervisorUserId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Configurar PIN si tiene
                    if (testUser.Pin != null)
                    {
                        person.PinCodeHash = BCrypt.Net.BCrypt.HashPassword(testUser.Pin);
                    }

                    context.PersonsWithDisability.Add(person);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedProfessionalsAsync(UserManager<User> userManager, AppDbContext context)
        {
            var professionals = new[]
            {
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                    Email = "profesional@test.com",
                    Password = "Prof123!",
                    FirstName = "Pedro",
                    LastName = "Martinez",
                    LicenseNumber = "PROF-001",
                    Specialty = "Terapia Ocupacional"
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Email = "docente@test.com",
                    Password = "Doc123!",
                    FirstName = "Laura",
                    LastName = "Gonzalez",
                    LicenseNumber = "PROF-002",
                    Specialty = "Educacion Especial"
                }
            };

            foreach (var prof in professionals)
            {
                // Verificar si ya existe por email o por ID
                var existingUser = await userManager.FindByEmailAsync(prof.Email);
                if (existingUser != null)
                    continue;

                var existingById = await userManager.FindByIdAsync(prof.Id.ToString());
                if (existingById != null)
                    continue;

                var user = new User
                {
                    Id = prof.Id,
                    Name = prof.FirstName,
                    Surname = prof.LastName,
                    Email = prof.Email,
                    UserName = prof.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    LockoutEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, prof.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, IdentityRoles.Professional.ToString());

                    var professional = new Professional
                    {
                        Id = Guid.NewGuid(),
                        UserId = prof.Id,
                        FirstName = prof.FirstName,
                        LastName = prof.LastName,
                        LicenseNumber = prof.LicenseNumber,
                        Specialty = prof.Specialty,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Professionals.Add(professional);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedFamilyAsync(UserManager<User> userManager, AppDbContext context)
        {
            var families = new[]
            {
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                    Email = "familia@test.com",
                    Password = "Fam123!",
                    FirstName = "Rosa",
                    LastName = "Sanchez",
                    Phone = "1155667788",
                    Relationship = "Madre"
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    Email = "tutor@test.com",
                    Password = "Tutor123!",
                    FirstName = "Miguel",
                    LastName = "Fernandez",
                    Phone = "1144556677",
                    Relationship = "Tutor Legal"
                }
            };

            foreach (var fam in families)
            {
                // Verificar si ya existe por email o por ID
                var existingUser = await userManager.FindByEmailAsync(fam.Email);
                if (existingUser != null)
                    continue;

                var existingById = await userManager.FindByIdAsync(fam.Id.ToString());
                if (existingById != null)
                    continue;

                var user = new User
                {
                    Id = fam.Id,
                    Name = fam.FirstName,
                    Surname = fam.LastName,
                    Email = fam.Email,
                    UserName = fam.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    LockoutEnabled = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, fam.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, IdentityRoles.FamilyRepresentative.ToString());

                    var family = new FamilyRepresentative
                    {
                        Id = Guid.NewGuid(),
                        UserId = fam.Id,
                        FirstName = fam.FirstName,
                        LastName = fam.LastName,
                        Phone = fam.Phone,
                        Relationship = fam.Relationship,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.FamilyRepresentatives.Add(family);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
