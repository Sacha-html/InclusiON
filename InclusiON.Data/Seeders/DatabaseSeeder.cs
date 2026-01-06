using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InclusiON.Entities.Enums;
using InclusiON.Entities.Models;
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
            // Usuarios de prueba para visual login
            var testUsers = new[]
            {
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                    Name = "Maria",
                    Surname = "Garcia",
                    Email = "maria@test.com",
                    Pin = "1234",
                    LoginMethodId = 2, // PIN
                    AvatarColor = "#4CAF50"
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "Juan",
                    Surname = "Lopez",
                    Email = "juan@test.com",
                    Pin = (string?)null,
                    LoginMethodId = 3, // EMOJI_SEQUENCE
                    AvatarColor = "#2196F3"
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                    Name = "Ana",
                    Surname = "Martinez",
                    Email = "ana@test.com",
                    Pin = (string?)null,
                    LoginMethodId = 4, // COLOR_SHAPE
                    AvatarColor = "#9C27B0"
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                    Name = "Carlos",
                    Surname = "Rodriguez",
                    Email = "carlos@test.com",
                    Pin = "5678",
                    LoginMethodId = 7, // PROFILE_SELECT
                    AvatarColor = "#FF9800"
                }
            };

            foreach (var testUser in testUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(testUser.Email);
                if (existingUser != null)
                    continue;

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

                var result = await userManager.CreateAsync(user, "Test123!");

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
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Configurar credenciales segun metodo de login
                    if (testUser.Pin != null)
                    {
                        person.PinCodeHash = BCrypt.Net.BCrypt.HashPassword(testUser.Pin);
                    }

                    if (testUser.LoginMethodId == 3) // EMOJI_SEQUENCE
                    {
                        // Secuencia: perro, casa, girasol (3 emojis)
                        person.EmojiSequence = JsonSerializer.Serialize(new[] { "🐶", "🏠", "🌻" });
                    }

                    if (testUser.LoginMethodId == 4) // COLOR_SHAPE
                    {
                        // Circulo rojo (ID 1 en la combinacion de colores/formas)
                        person.ColorShapeId = 1;
                    }

                    context.PersonsWithDisability.Add(person);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
