using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Shared.Constants;
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
            await SeedRolePermissionsAsync(context);
            await SeedSkillAreasAsync(context);
            await SeedProfessionalsAsync(userManager, context);
            await SeedVisualLoginTestUsersAsync(userManager, context);
            await SeedFamilyAsync(userManager, context);
            await SeedFiveAdditionalStudentsAndTutorsAsync(userManager, context);

            // Inicializar Roadmap Estándar para todos los alumnos existentes
            // El RoadmapInitializer ya verifica si el alumno tiene roadmap y lo omite si existe
            var students = await context.PersonsWithDisability.ToListAsync();
            foreach (var student in students)
            {
                await RoadmapInitializerAccessor.InitializeStudentRoadmap(context, student.Id, student.SupervisorUserId, CancellationToken.None);
            }

            // Las plantillas del Roadmap ya no se siembran automáticamente.
            // Los profesionales crean sus propias plantillas desde la Biblioteca de Plantillas.
            // Script de limpieza manual: Scripts/cleanup_templates.sql

            await SeedCustomClassroomsAndStudentsAsync(userManager, context);
        }

        private static async Task SeedAdminUserAsync(UserManager<User> userManager)
        {
            const string adminEmail = "admin@test.com";
            const string adminPassword = "Admin123!";

            var existingById = await userManager.FindByIdAsync("00000000-0000-0000-0000-000000000001");
            if (existingById != null)
            {
                if (existingById.Email != adminEmail)
                {
                    existingById.Email = adminEmail;
                    existingById.NormalizedEmail = adminEmail.ToUpperInvariant();
                    existingById.UserName = adminEmail;
                    existingById.NormalizedUserName = adminEmail.ToUpperInvariant();
                    await userManager.UpdateAsync(existingById);
                }
                return;
            }

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

        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var professionalRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var familyRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var personRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            var claims = new List<IdentityRoleClaim<Guid>>
            {
                // === ADMIN ===
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:delete" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:delete" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:delete" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:delete" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:link" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:unlink" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:delete" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "roadmap:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "roadmap:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "roadmap:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "roadmap:delete" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "diagnoses:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "diagnoses:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "diagnoses:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:approve" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:reject" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:export" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "messages:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "invitations:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "invitations:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "institutions:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "institutions:create" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "institutions:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "settings:read" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "settings:update" },
                new() { RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "audit:read" },

                // === PROFESSIONAL ===
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "persons:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "persons:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "persons:update" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "activities:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "activities:update" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "roadmap:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "roadmap:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "roadmap:update" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "roadmap:delete" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "diagnoses:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "diagnoses:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "diagnoses:update" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "reports:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "reports:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "reports:submit" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "reports:export" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "messages:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "invitations:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "invitations:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "family:read" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "family:create" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "family:update" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "family:link" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "family:unlink" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "professionals:link-family" },
                new() { RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "professionals:unlink-family" },

                // === FAMILY REPRESENTATIVE ===
                new() { RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "persons:read" },
                new() { RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "diagnoses:read" },
                new() { RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "reports:read" },
                new() { RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "reports:export" },
                new() { RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
                new() { RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "messages:create" },

                // === PERSON WITH DISABILITY ===
                new() { RoleId = personRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { RoleId = personRoleId, ClaimType = "permission", ClaimValue = "activities:respond" },
                new() { RoleId = personRoleId, ClaimType = "permission", ClaimValue = "roadmap:read" },
                new() { RoleId = personRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
            };

            // Upsert: solo agregar los claims que no existen aun
            var existingClaims = await context.RoleClaims
                .Select(c => new { c.RoleId, c.ClaimType, c.ClaimValue })
                .ToListAsync();

            var newClaims = claims.Where(c => !existingClaims.Any(e =>
                e.RoleId == c.RoleId &&
                e.ClaimType == c.ClaimType &&
                e.ClaimValue == c.ClaimValue))
                .ToList();

            if (newClaims.Count > 0)
            {
                context.RoleClaims.AddRange(newClaims);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedVisualLoginTestUsersAsync(UserManager<User> userManager, AppDbContext context)
        {
            // Usuarios de prueba para visual login - solo metodos activos (STANDARD=1, PIN=2, ASSISTED=3)
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
                    AvatarColor = AvatarColors.DefaultProfessional,
                    SupervisorUserId = (Guid?)null,
                    PersonId = Guid.Parse("00000000-0000-0000-0000-000000000100")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                    Name = "Juan",
                    Surname = "Lopez",
                    Email = "juan@test.com",
                    Password = "Juan123!",
                    Pin = "1234",
                    LoginMethodId = 2, // PIN
                    AvatarColor = AvatarColors.DefaultPerson,
                    SupervisorUserId = (Guid?)null,
                    PersonId = Guid.Parse("00000000-0000-0000-0000-000000000101")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                    Name = "Ana",
                    Surname = "Martinez",
                    Email = "ana@test.com",
                    Password = (string?)null,
                    Pin = (string?)null,
                    LoginMethodId = 3, // ASSISTED (requiere supervisor)
                    AvatarColor = AvatarColors.DefaultFamily,
                    SupervisorUserId = (Guid?)Guid.Parse("00000000-0000-0000-0000-000000000020"), // Supervisor
                    PersonId = Guid.Parse("00000000-0000-0000-0000-000000000102")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                    Name = "Carlos",
                    Surname = "Rodriguez",
                    Email = "carlos@test.com",
                    Password = "Carlos123!",
                    Pin = "5678",
                    LoginMethodId = 2, // PIN
                    AvatarColor = "#FF9800", // Naranja del catalogo AvatarColors
                    SupervisorUserId = (Guid?)null,
                    PersonId = Guid.Parse("00000000-0000-0000-0000-000000000103")
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
                            existingPerson.PinCodeHash = PinHashAccessor.Hash(testUser.Pin);
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
                        Id = testUser.PersonId,
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
                        person.PinCodeHash = PinHashAccessor.Hash(testUser.Pin);
                    }

                    context.PersonsWithDisability.Add(person);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedSkillAreasAsync(AppDbContext context)
        {
            if (await context.Set<SkillArea>().AnyAsync())
                return;

            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var now = DateTime.UtcNow;

            var comunicacion = new SkillArea
            {
                Name = "Comunicación",
                Description = "Actividades orientadas al desarrollo de habilidades comunicativas mediante pictogramas, selección de opciones y expresión.",
                Icon = "chat",
                Color = "#2E5FA3",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = adminId
            };

            var alfabetizacion = new SkillArea
            {
                Name = "Alfabetización",
                Description = "Actividades de lectura global, reconocimiento de sonidos y construcción de palabras para el desarrollo de la lectoescritura.",
                Icon = "menu_book",
                Color = "#4CAF50",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = adminId
            };

            var logicoMatematico = new SkillArea
            {
                Name = "Lógico-matemático",
                Description = "Actividades de clasificación, ordenamiento y numeración para el desarrollo del pensamiento lógico-matemático.",
                Icon = "calculate",
                Color = "#FF9800",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = adminId
            };

            context.Set<SkillArea>().AddRange(comunicacion, alfabetizacion, logicoMatematico);
            await context.SaveChangesAsync();

            // ActivityTemplateTypes para Comunicación
            var templatesComunicacion = new[]
            {
                new ActivityTemplateType
                {
                    SkillAreaId = comunicacion.Id,
                    Name = "Seleccionar pictograma",
                    Code = "PICTOGRAM_SELECT",
                    Description = "El usuario debe seleccionar el pictograma correcto entre varias opciones.",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = true,
                    HasAudio = true,
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                },
                new ActivityTemplateType
                {
                    SkillAreaId = comunicacion.Id,
                    Name = "Selección de opciones",
                    Code = "OPTION_SELECT",
                    Description = "El usuario elige la respuesta correcta entre opciones de texto o imagen.",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = false,
                    HasAudio = true,
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                }
            };

            // ActivityTemplateTypes para Alfabetización
            var templatesAlfabetizacion = new[]
            {
                new ActivityTemplateType
                {
                    SkillAreaId = alfabetizacion.Id,
                    Name = "Lectura global",
                    Code = "GLOBAL_READING",
                    Description = "El usuario asocia una palabra completa con su imagen o significado.",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = false,
                    HasAudio = true,
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                },
                new ActivityTemplateType
                {
                    SkillAreaId = alfabetizacion.Id,
                    Name = "Reconocer sonidos",
                    Code = "SOUND_RECOGNITION",
                    Description = "El usuario identifica el sonido de una letra o sílaba y lo asocia con su representación.",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = false,
                    HasAudio = true,
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                },
                new ActivityTemplateType
                {
                    SkillAreaId = alfabetizacion.Id,
                    Name = "Armar palabras",
                    Code = "BUILD_WORD",
                    Description = "El usuario construye una palabra ordenando letras o sílabas.",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = false,
                    HasAudio = false,
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                }
            };

            // ActivityTemplateTypes para Lógico-matemático
            var templatesLogicoMatematico = new[]
            {
                new ActivityTemplateType
                {
                    SkillAreaId = logicoMatematico.Id,
                    Name = "Clasificación",
                    Code = "CLASSIFY",
                    Description = "El usuario agrupa elementos según un criterio dado (color, forma, categoría).",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = false,
                    HasAudio = false,
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                },
                new ActivityTemplateType
                {
                    SkillAreaId = logicoMatematico.Id,
                    Name = "Ordenamiento",
                    Code = "ORDER_SEQUENCE",
                    Description = "El usuario ordena elementos en una secuencia lógica (mayor a menor, cronológico).",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = false,
                    HasAudio = false,
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                },
                new ActivityTemplateType
                {
                    SkillAreaId = logicoMatematico.Id,
                    Name = "Numeración",
                    Code = "NUMERATION",
                    Description = "El usuario practica conteo, reconocimiento de números y asociación cantidad-número.",
                    ContentSchema = "",
                    ComponentName = "",
                    UsesPictograms = false,
                    HasAudio = false,
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = adminId
                }
            };

            context.Set<ActivityTemplateType>().AddRange(templatesComunicacion);
            context.Set<ActivityTemplateType>().AddRange(templatesAlfabetizacion);
            context.Set<ActivityTemplateType>().AddRange(templatesLogicoMatematico);
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
                    Specialty = "Terapia Ocupacional",
                    ProfessionalId = Guid.Parse("00000000-0000-0000-0000-000000000200")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                    Email = "docente@test.com",
                    Password = "Doc123!",
                    FirstName = "Laura",
                    LastName = "Gonzalez",
                    LicenseNumber = "PROF-002",
                    Specialty = "Educacion Especial",
                    ProfessionalId = Guid.Parse("00000000-0000-0000-0000-000000000201")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000022"),
                    Email = "profesional2@test.com",
                    Password = "Password123!",
                    FirstName = "Sofía",
                    LastName = "Gutiérrez",
                    LicenseNumber = "PROF-003",
                    Specialty = "Psicopedagogía",
                    ProfessionalId = Guid.Parse("00000000-0000-0000-0000-000000000202")
                }
            };

            foreach (var prof in professionals)
            {
                var existingUser = await userManager.FindByEmailAsync(prof.Email);
                if (existingUser == null)
                {
                    var existingById = await userManager.FindByIdAsync(prof.Id.ToString());
                    if (existingById == null)
                    {
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
                            existingUser = user;
                        }
                        else
                        {
                            existingUser = await userManager.FindByEmailAsync(prof.Email);
                        }
                    }
                    else
                    {
                        existingUser = existingById;
                    }
                }

                if (existingUser != null)
                {
                    var existingProf = await context.Professionals.FirstOrDefaultAsync(p => p.UserId == existingUser.Id);
                    if (existingProf == null)
                    {
                        var professional = new Professional
                        {
                            Id = prof.ProfessionalId,
                            UserId = existingUser.Id,
                            FirstName = prof.FirstName,
                            LastName = prof.LastName,
                            LicenseNumber = prof.LicenseNumber,
                            Specialty = prof.Specialty,
                            Status = ProfessionalStatusEnum.Approved,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        context.Professionals.Add(professional);
                    }
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
                    Password = "Familia123!",
                    FirstName = "Rosa",
                    LastName = "Sanchez",
                    Phone = "1155667788",
                    Relationship = "Madre",
                    LinkedPersonEmail = "maria@test.com",
                    FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000300")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                    Email = "tutor@test.com",
                    Password = "Tutor123!",
                    FirstName = "Miguel",
                    LastName = "Fernandez",
                    Phone = "1144556677",
                    Relationship = "Tutor Legal",
                    LinkedPersonEmail = "juan@test.com",
                    FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000301")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000032"),
                    Email = "anatu@test.com",
                    Password = "Tutor123!",
                    FirstName = "Patricia",
                    LastName = "Martínez",
                    Phone = "1133445566",
                    Relationship = "Madre",
                    LinkedPersonEmail = "ana@test.com",
                    FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000302")
                },
                new {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000033"),
                    Email = "carlostu@test.com",
                    Password = "Tutor123!",
                    FirstName = "Roberto",
                    LastName = "Rodríguez",
                    Phone = "1122334455",
                    Relationship = "Padre",
                    LinkedPersonEmail = "carlos@test.com",
                    FamilyId = Guid.Parse("00000000-0000-0000-0000-000000000303")
                }
            };

            foreach (var fam in families)
            {
                var existingUser = await userManager.FindByEmailAsync(fam.Email);
                if (existingUser == null)
                {
                    existingUser = new User
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

                    var result = await userManager.CreateAsync(existingUser, fam.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(existingUser, IdentityRoles.FamilyRepresentative.ToString());
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create user {fam.Email}: {errors}");
                    }
                }

                // Asegurar que exista el FamilyRepresentative
                var familyEntity = await context.FamilyRepresentatives
                    .FirstOrDefaultAsync(f => f.UserId == existingUser.Id);

                if (familyEntity == null)
                {
                    familyEntity = new FamilyRepresentative
                    {
                        Id = fam.FamilyId,
                        UserId = existingUser.Id,
                        FirstName = fam.FirstName,
                        LastName = fam.LastName,
                        Phone = fam.Phone,
                        Relationship = fam.Relationship,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.FamilyRepresentatives.Add(familyEntity);
                    await context.SaveChangesAsync();
                }

                // Vincular con persona
                var personToLink = await context.PersonsWithDisability
                    .FirstOrDefaultAsync(p => p.User.Email == fam.LinkedPersonEmail);

                if (personToLink != null)
                {
                    var alreadyLinked = await context.PersonRepresentatives
                        .AnyAsync(pr => pr.RepresentativeId == familyEntity.Id && pr.PersonId == personToLink.Id);

                    if (!alreadyLinked)
                    {
                        context.PersonRepresentatives.Add(new PersonRepresentative
                        {
                            PersonId = personToLink.Id,
                            RepresentativeId = familyEntity.Id,
                            Relationship = fam.Relationship,
                            IsPrimary = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedFiveAdditionalStudentsAndTutorsAsync(UserManager<User> userManager, AppDbContext context)
        {
            var professional = await context.Professionals.FirstOrDefaultAsync(p => p.User.Email == "profesional@test.com");
            if (professional == null) return;

            var data = new[]
            {
                new { StudentEmail = "tomas@test.com", StudentName = "Tomás", StudentSurname = "Pérez", StudentDni = "11111111", TutorEmail = "carlostutor@test.com", TutorName = "Carlos", TutorSurname = "Pérez", Relationship = "Padre" },
                new { StudentEmail = "sofia@test.com", StudentName = "Sofía", StudentSurname = "Rodríguez", StudentDni = "22222222", TutorEmail = "anatutor@test.com", TutorName = "Ana", TutorSurname = "Rodríguez", Relationship = "Madre" },
                new { StudentEmail = "mateo@test.com", StudentName = "Mateo", StudentSurname = "Díaz", StudentDni = "33333333", TutorEmail = "luistutor@test.com", TutorName = "Luis", TutorSurname = "Díaz", Relationship = "Padre" },
                new { StudentEmail = "valentina@test.com", StudentName = "Valentina", StudentSurname = "Silva", StudentDni = "44444444", TutorEmail = "elenatutor@test.com", TutorName = "Elena", TutorSurname = "Silva", Relationship = "Madre" },
                new { StudentEmail = "benjamin@test.com", StudentName = "Benjamín", StudentSurname = "Castro", StudentDni = "55555555", TutorEmail = "jorgetutor@test.com", TutorName = "Jorge", TutorSurname = "Castro", Relationship = "Tutor Legal" }
            };

            foreach (var item in data)
            {
                var studentUser = await userManager.FindByEmailAsync(item.StudentEmail);
                PersonWithDisability student = null!;
                if (studentUser == null)
                {
                    studentUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Name = item.StudentName,
                        Surname = item.StudentSurname,
                        Email = item.StudentEmail,
                        UserName = item.StudentEmail,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(studentUser, "Student123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(studentUser, IdentityRoles.PersonWithDisability.ToString());
                        student = new PersonWithDisability
                        {
                            Id = Guid.NewGuid(),
                            UserId = studentUser.Id,
                            FirstName = item.StudentName,
                            LastName = item.StudentSurname,
                            BirthDate = DateTime.UtcNow.AddYears(-15),
                            DocumentNumber = item.StudentDni,
                            LoginMethodId = item.StudentEmail == "benjamin@test.com" ? 3 : 1, // 3: Ingreso Asistido (requiere supervisor) para Benjamín Castro
                            SupervisorUserId = item.StudentEmail == "benjamin@test.com" ? professional.UserId : null,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.PersonsWithDisability.Add(student);
                    }
                }
                else
                {
                    student = await context.PersonsWithDisability.FirstOrDefaultAsync(p => p.UserId == studentUser.Id);
                    if (student != null && item.StudentEmail == "benjamin@test.com")
                    {
                        student.LoginMethodId = 3;
                        student.SupervisorUserId = professional.UserId;
                    }
                }

                var tutorUser = await userManager.FindByEmailAsync(item.TutorEmail);
                FamilyRepresentative tutor = null!;
                if (tutorUser == null)
                {
                    tutorUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Name = item.TutorName,
                        Surname = item.TutorSurname,
                        Email = item.TutorEmail,
                        UserName = item.TutorEmail,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(tutorUser, "Tutor123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(tutorUser, IdentityRoles.FamilyRepresentative.ToString());
                        tutor = new FamilyRepresentative
                        {
                            Id = Guid.NewGuid(),
                            UserId = tutorUser.Id,
                            FirstName = item.TutorName,
                            LastName = item.TutorSurname,
                            Phone = "12345678",
                            DocumentNumber = "T" + item.StudentDni,
                            Relationship = item.Relationship,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.FamilyRepresentatives.Add(tutor);
                    }
                }
                else
                {
                    tutor = await context.FamilyRepresentatives.FirstOrDefaultAsync(f => f.UserId == tutorUser.Id);
                }

                await context.SaveChangesAsync();

                if (student != null && tutor != null)
                {
                    var studentTutorLinked = await context.PersonRepresentatives
                        .AnyAsync(pr => pr.PersonId == student.Id && pr.RepresentativeId == tutor.Id);
                    if (!studentTutorLinked)
                    {
                        context.PersonRepresentatives.Add(new PersonRepresentative
                        {
                            PersonId = student.Id,
                            RepresentativeId = tutor.Id,
                            Relationship = item.Relationship,
                            IsPrimary = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    var studentProfLinked = await context.ProfessionalPersons
                        .AnyAsync(pp => pp.ProfessionalId == professional.Id && pp.PersonId == student.Id);
                    if (!studentProfLinked)
                    {
                        context.ProfessionalPersons.Add(new ProfessionalPerson
                        {
                            ProfessionalId = professional.Id,
                            PersonId = student.Id,
                            IsPrimaryProfessional = true,
                            CanSuperviseLogin = true,
                            IsActive = true,
                            AssignedAt = DateTime.UtcNow
                        });
                    }

                    var professional2 = await context.Professionals.FirstOrDefaultAsync(p => p.User.Email == "docente@test.com");
                    if (professional2 != null)
                    {
                        var studentProf2Linked = await context.ProfessionalPersons
                            .AnyAsync(pp => pp.ProfessionalId == professional2.Id && pp.PersonId == student.Id);
                        if (!studentProf2Linked)
                        {
                            context.ProfessionalPersons.Add(new ProfessionalPerson
                            {
                                ProfessionalId = professional2.Id,
                                PersonId = student.Id,
                                IsPrimaryProfessional = false,
                                CanSuperviseLogin = true,
                                IsActive = true,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                    }

                    var professional3 = await context.Professionals.FirstOrDefaultAsync(p => p.User.Email == "profesional2@test.com");
                    if (professional3 != null)
                    {
                        var studentProf3Linked = await context.ProfessionalPersons
                            .AnyAsync(pp => pp.ProfessionalId == professional3.Id && pp.PersonId == student.Id);
                        if (!studentProf3Linked)
                        {
                            context.ProfessionalPersons.Add(new ProfessionalPerson
                            {
                                ProfessionalId = professional3.Id,
                                PersonId = student.Id,
                                IsPrimaryProfessional = false,
                                CanSuperviseLogin = true,
                                IsActive = true,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        // ──────────────────────────────────────────────────────────────────────────────
        // MÉTODO REMOVIDO: SeedOfficialThesisRoadmapTemplatesAsync
        // Las plantillas del Roadmap se crean dinámicamente por los profesionales.
        // Este código se conserva comentado como referencia histórica.
        // Ejecutar Scripts/cleanup_templates.sql para limpiar registros existentes.
        // ──────────────────────────────────────────────────────────────────────────────
        /*
        public static async Task SeedOfficialThesisRoadmapTemplatesAsync(AppDbContext context)
        {
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var skillArea = await context.SkillAreas.FirstOrDefaultAsync(sa => sa.Name == "Trayectoria");
            if (skillArea == null)
            {
                skillArea = new SkillArea
                {
                    Name = "Trayectoria",
                    Description = "Camino de aprendizaje estándar anti-frustración.",
                    Icon = "map",
                    Color = "#673AB7",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminId
                };
                context.SkillAreas.Add(skillArea);
                await context.SaveChangesAsync();
            }

            var defaultProf = await context.Professionals.FirstOrDefaultAsync();
            var defaultProfId = defaultProf?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000000200");

            var officialTemplates = new[]
            {
                new
                {
                    Seq = 1,
                    Title = "Rompecabezas de 2 piezas",
                    OldTitles = new[] { "Rompecabezas de 2 piezas" },
                    Description = "Une la mitad de la imagen para completar el objeto cotidiano.",
                    Instructions = "Arrastra y une la mitad de la imagen para completar el objeto cotidiano.",
                    CatId = 8, // Estimulación Cognitiva
                    TemplateCode = "CLASSIFY",
                    ContentJson = """{"pairs":[{"id":1,"label":"Taza (Derecha)","pictogramId":"pic_taza_der"},{"id":2,"label":"Taza (Izquierda)","pictogramId":"pic_taza_izq"}]}"""
                },
                new
                {
                    Seq = 2,
                    Title = "Mi rutina visual",
                    OldTitles = new[] { "Mi rutina visual" },
                    Description = "Ordena los pasos de tu rutina diaria.",
                    Instructions = "Ordena los pasos de tu rutina diaria.",
                    CatId = 3, // Habilidades Socioemocionales
                    TemplateCode = "ORDER_SEQUENCE",
                    ContentJson = """{"items":[{"id":1,"label":"Despertar","pictogramId":"pic_despertar","correctPosition":1},{"id":2,"label":"Comer","pictogramId":"pic_comer","correctPosition":2},{"id":3,"label":"Jugar","pictogramId":"pic_jugar","correctPosition":3}]}"""
                },
                new
                {
                    Seq = 3,
                    Title = "Concepto Muchos / Pocos",
                    OldTitles = new[] { "Concepto 'Muchos / Pocos'", "Concepto Muchos / Pocos" },
                    Description = "¿Dónde hay muchas manzanas?",
                    Instructions = "¿Dónde hay muchas manzanas?",
                    CatId = 2, // Numeración y Matemática
                    TemplateCode = "PICTOGRAM_SELECT",
                    ContentJson = """{"correctItemId":2,"items":[{"id":1,"pictogramId":"pic_una_manzana","label":"Pocas (1)"},{"id":2,"pictogramId":"pic_muchas_manzanas","label":"Muchas (8)"}]}"""
                },
                new
                {
                    Seq = 4,
                    Title = "Secuencia de acción (Camino Visual)",
                    OldTitles = new[] { "Explotar burbujas", "Secuencia de acción (Camino Visual)" },
                    Description = "Toca las burbujas en orden para terminar el camino.",
                    Instructions = "Toca las burbujas en orden para terminar el camino.",
                    CatId = 5, // Motricidad y Coordinación
                    TemplateCode = "ORDER_SEQUENCE",
                    ContentJson = """{"items":[{"id":1,"label":"Burbuja 1","pictogramId":"pic_burbuja_1","correctPosition":1},{"id":2,"label":"Burbuja 2","pictogramId":"pic_burbuja_2","correctPosition":2},{"id":3,"label":"Burbuja 3","pictogramId":"pic_burbuja_3","correctPosition":3}]}"""
                },
                new
                {
                    Seq = 5,
                    Title = "Asociación Funcional Cotidiana",
                    OldTitles = new[] { "¿Dónde va cada cosa?", "¿Qué quieres hacer?", "Asociación Funcional Cotidiana" },
                    Description = "Contexto: Cama. ¿Qué objeto va en la cama?",
                    Instructions = "Contexto: Cama. ¿Qué objeto va en la cama?",
                    CatId = 8, // Estimulación Cognitiva
                    TemplateCode = "PICTOGRAM_SELECT",
                    ContentJson = """{"correctItemId":1,"items":[{"id":1,"pictogramId":"pic_almohada","label":"Almohada"},{"id":2,"pictogramId":"pic_pelota","label":"Pelota de fútbol"}]}"""
                },
                new
                {
                    Seq = 6,
                    Title = "Reconocimiento Fonológico",
                    OldTitles = new[] { "Conciencia fonológica", "Reconocimiento Fonológico" },
                    Description = "¿Qué animal empieza con la letra A?",
                    Instructions = "¿Qué animal empieza con la letra A?",
                    CatId = 1, // Lectoescritura
                    TemplateCode = "PICTOGRAM_SELECT",
                    ContentJson = """{"correctItemId":3,"items":[{"id":1,"pictogramId":"pic_perro","label":"Perro"},{"id":2,"pictogramId":"pic_gato","label":"Gato"},{"id":3,"pictogramId":"pic_arana","label":"Araña"}]}"""
                },
                new
                {
                    Seq = 7,
                    Title = "Identificación de Formas Básicas",
                    OldTitles = new[] { "Colorear libre", "Identificación de Formas Básicas" },
                    Description = "¿Cuál es el círculo?",
                    Instructions = "¿Cuál es el círculo?",
                    CatId = 2, // Numeración y Matemática
                    TemplateCode = "PICTOGRAM_SELECT",
                    ContentJson = """{"correctItemId":3,"items":[{"id":1,"pictogramId":"pic_cuadrado","label":"Cuadrado"},{"id":2,"pictogramId":"pic_triangulo","label":"Triángulo"},{"id":3,"pictogramId":"pic_circulo","label":"Círculo"}]}"""
                },
                new
                {
                    Seq = 8,
                    Title = "Vestirse para el frío",
                    OldTitles = new[] { "Vestirse para el frío" },
                    Description = "Guarda la ropa de invierno en el armario.",
                    Instructions = "Guarda la ropa de invierno en el armario.",
                    CatId = 7, // Autonomía y Vida Diaria
                    TemplateCode = "CLASSIFY",
                    ContentJson = """{"pairs":[{"id":1,"label":"Invierno","pictogramId":"pic_bufanda"},{"id":2,"label":"Invierno","pictogramId":"pic_gorro"}]}"""
                },
                new
                {
                    Seq = 9,
                    Title = "Seriación de Tamaños",
                    OldTitles = new[] { "Clasificación por tamaño", "Seriación de Tamaños" },
                    Description = "Ordena las pelotas de la más pequeña a la más grande.",
                    Instructions = "Ordena las pelotas de la más pequeña a la más grande.",
                    CatId = 2, // Numeración y Matemática
                    TemplateCode = "ORDER_SEQUENCE",
                    ContentJson = """{"items":[{"id":1,"label":"Pequeña","pictogramId":"pic_pelota_chica","correctPosition":1},{"id":2,"label":"Mediana","pictogramId":"pic_pelota_mediana","correctPosition":2},{"id":3,"label":"Grande","pictogramId":"pic_pelota_grande","correctPosition":3}]}"""
                },
                new
                {
                    Seq = 10,
                    Title = "Encuentra el intruso",
                    OldTitles = new[] { "Encuentra el intruso" },
                    Description = "¿Qué objeto no pertenece a este grupo de frutas?",
                    Instructions = "¿Qué objeto no pertenece a este grupo de frutas?",
                    CatId = 8, // Estimulación Cognitiva
                    TemplateCode = "PICTOGRAM_SELECT",
                    ContentJson = """{"correctItemId":4,"items":[{"id":1,"pictogramId":"pic_manzana","label":"Manzana"},{"id":2,"pictogramId":"pic_pera","label":"Pera"},{"id":3,"pictogramId":"pic_banana","label":"Banana"},{"id":4,"pictogramId":"pic_zapato","label":"Zapato"}]}"""
                }
            };

            foreach (var t in officialTemplates)
            {
                var templateType = await context.Set<ActivityTemplateType>().FirstOrDefaultAsync(tp => tp.Code == t.TemplateCode);
                var templateTypeId = templateType?.Id ?? 1;

                var act = await context.Activities
                    .Include(a => a.Content)
                    .FirstOrDefaultAsync(a => a.Title == t.Title || t.OldTitles.Contains(a.Title));

                if (act == null)
                {
                    act = new Activity
                    {
                        Title = t.Title,
                        Description = t.Description,
                        Instructions = t.Instructions,
                        CategoryId = t.CatId,
                        SkillAreaId = skillArea.Id,
                        ProfessionalId = defaultProfId,
                        HasVisualSupport = true,
                        HasAudioSupport = true,
                        UsesEasyReading = true,
                        UsesPictograms = true,
                        RequiresSupervision = false,
                        IsStandardActivity = true,
                        IsTemplate = true,
                        ComplexityLevel = 1,
                        EstimatedDurationMinutes = 2,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    };
                    context.Activities.Add(act);
                    await context.SaveChangesAsync();

                    var content = new ActivityContent
                    {
                        ActivityId = act.Id,
                        TemplateTypeId = templateTypeId,
                        ContentJson = t.ContentJson,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    };
                    context.Set<ActivityContent>().Add(content);
                    await context.SaveChangesAsync();
                }
                else
                {
                    act.Title = t.Title;
                    act.Description = t.Description;
                    act.Instructions = t.Instructions;
                    act.CategoryId = t.CatId;
                    act.SkillAreaId = skillArea.Id;
                    act.IsTemplate = true;
                    act.IsStandardActivity = true;
                    act.HasVisualSupport = true;
                    act.HasAudioSupport = true;
                    act.UsesEasyReading = true;
                    act.UsesPictograms = true;
                    act.ComplexityLevel = 1;
                    act.EstimatedDurationMinutes = 2;
                    act.UpdatedAt = DateTime.UtcNow;
                    act.UpdatedBy = adminId;

                    if (act.Content != null)
                    {
                        act.Content.TemplateTypeId = templateTypeId;
                        act.Content.ContentJson = t.ContentJson;
                        act.Content.UpdatedAt = DateTime.UtcNow;
                        act.Content.UpdatedBy = adminId;
                    }
                    else
                    {
                        var content = new ActivityContent
                        {
                            ActivityId = act.Id,
                            TemplateTypeId = templateTypeId,
                            ContentJson = t.ContentJson,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = adminId
                        };
                        context.Set<ActivityContent>().Add(content);
                    }

                    await context.SaveChangesAsync();
                }
            }
        }
        */ // FIN del método SeedOfficialThesisRoadmapTemplatesAsync (comentado)

        private static async Task SeedCustomClassroomsAndStudentsAsync(UserManager<User> userManager, AppDbContext context)
        {
            // 0. Limpiar alumnos y tutores previamente semillados (emails studentX@inclusion.com y tutorX@inclusion.com)
            var studentEmailsToDelete = Enumerable.Range(1, 55).Select(i => $"student{i}@inclusion.com").ToList();
            var tutorEmailsToDelete = Enumerable.Range(1, 55).Select(i => $"tutor{i}@inclusion.com").ToList();

            var usersToDelete = await context.Users
                .Where(u => u.Email != null && (studentEmailsToDelete.Contains(u.Email) || tutorEmailsToDelete.Contains(u.Email)))
                .ToListAsync();

            if (usersToDelete.Any())
            {
                var userIds = usersToDelete.Select(u => u.Id).ToList();

                // Obtener los IDs reales de PersonsWithDisability y FamilyRepresentatives
                var studentPersonIds = await context.PersonsWithDisability
                    .Where(p => userIds.Contains(p.UserId))
                    .Select(p => p.Id)
                    .ToListAsync();

                var tutorRepIds = await context.FamilyRepresentatives
                    .Where(f => userIds.Contains(f.UserId))
                    .Select(f => f.Id)
                    .ToListAsync();

                // 1. Eliminar de tablas dependientes del Roadmap (usando studentPersonIds)
                await context.AdaptiveAdjustmentLogs
                    .Where(x => studentPersonIds.Contains(x.PersonRoadmapActivity.PersonRoadmapArea.PersonRoadmap.PersonId))
                    .ExecuteDeleteAsync();

                await context.AdaptiveEngineConfigs
                    .Where(x => studentPersonIds.Contains(x.PersonRoadmapActivity.PersonRoadmapArea.PersonRoadmap.PersonId))
                    .ExecuteDeleteAsync();

                await context.ActivityResults
                    .Where(x => studentPersonIds.Contains(x.PersonRoadmapActivity.PersonRoadmapArea.PersonRoadmap.PersonId))
                    .ExecuteDeleteAsync();

                await context.PersonRoadmapActivities
                    .Where(x => studentPersonIds.Contains(x.PersonRoadmapArea.PersonRoadmap.PersonId))
                    .ExecuteDeleteAsync();

                await context.PersonRoadmapAreas
                    .Where(x => studentPersonIds.Contains(x.PersonRoadmap.PersonId))
                    .ExecuteDeleteAsync();

                await context.PersonRoadmaps
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                // 2. Eliminar de tablas dependientes de la Actividad (usando studentPersonIds)
                await context.ActivityResponses
                    .Where(x => studentPersonIds.Contains(x.Assignment.PersonId))
                    .ExecuteDeleteAsync();

                await context.ActivityAssignments
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                // 3. Eliminar de otras tablas relacionadas a alumnos (usando studentPersonIds)
                await context.PersonEmbeddings
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                await context.ProfessionalPersons
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                await context.Reports
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                await context.PersonSkillProfiles
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                await context.Diagnoses
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                await context.AccessAudits
                    .Where(x => x.AccessedPersonId.HasValue && studentPersonIds.Contains(x.AccessedPersonId.Value))
                    .ExecuteDeleteAsync();

                await context.CalendarEvents
                    .Where(x => x.StudentId.HasValue && studentPersonIds.Contains(x.StudentId.Value))
                    .ExecuteDeleteAsync();

                await context.Invitations
                    .Where(x => x.ForPersonId.HasValue && studentPersonIds.Contains(x.ForPersonId.Value))
                    .ExecuteDeleteAsync();

                // 4. Relaciones Alumno-Tutor (usando studentPersonIds y tutorRepIds)
                await context.PersonRepresentatives
                    .Where(x => studentPersonIds.Contains(x.PersonId) || tutorRepIds.Contains(x.RepresentativeId))
                    .ExecuteDeleteAsync();

                await context.PersonRepresentativeHistories
                    .Where(x => studentPersonIds.Contains(x.PersonId))
                    .ExecuteDeleteAsync();

                // 5. Mensajes (usando userIds y studentPersonIds)
                await context.Messages
                    .Where(x => userIds.Contains(x.SenderId) || userIds.Contains(x.ReceiverId) || (x.RelatedPersonId.HasValue && studentPersonIds.Contains(x.RelatedPersonId.Value)))
                    .ExecuteDeleteAsync();

                // 6. Perfiles base (FamilyRepresentatives y PersonsWithDisability)
                await context.FamilyRepresentatives
                    .Where(x => userIds.Contains(x.UserId))
                    .ExecuteDeleteAsync();

                await context.PersonsWithDisability
                    .Where(x => userIds.Contains(x.UserId))
                    .ExecuteDeleteAsync();

                await context.SaveChangesAsync();

                // 7. Identity Users (a través de userManager)
                foreach (var user in usersToDelete)
                {
                    await userManager.DeleteAsync(user);
                }
            }

            // 0. Eliminar profesionales o usuarios duplicados de "Sacha" (manteniendo únicamente el oficial)
            var sachaFixedProfId = Guid.Parse("00000000-0000-0000-0000-000000000203");
            var sachaFixedUserId = Guid.Parse("00000000-0000-0000-0000-000000000023");
            var duplicateSachaProfs = await context.Professionals
                .Include(p => p.User)
                .Where(p => p.Id != sachaFixedProfId &&
                            (p.FirstName.ToLower().Contains("sacha") ||
                             p.LastName.ToLower().Contains("del barrio") ||
                             (p.User != null && p.User.Email.ToLower().Contains("sacha"))))
                .ToListAsync();

            foreach (var dup in duplicateSachaProfs)
            {
                var dupUser = dup.User;

                var dupClassrooms = await context.Classrooms.Where(c => c.ProfessionalId == dup.Id).ToListAsync();
                context.Classrooms.RemoveRange(dupClassrooms);

                var dupPersons = await context.ProfessionalPersons.Where(pp => pp.ProfessionalId == dup.Id).ToListAsync();
                context.ProfessionalPersons.RemoveRange(dupPersons);

                var dupInsts = await context.ProfessionalInstitutions.Where(pi => pi.ProfessionalId == dup.Id).ToListAsync();
                context.ProfessionalInstitutions.RemoveRange(dupInsts);

                await context.SaveChangesAsync();

                context.Professionals.Remove(dup);
                await context.SaveChangesAsync();

                if (dupUser != null && dupUser.Id != sachaFixedUserId)
                {
                    await userManager.DeleteAsync(dupUser);
                }
            }

            var duplicateSachaUsers = await userManager.Users
                .Where(u => u.Id != sachaFixedUserId &&
                            u.Email != "sacha.delbarrio@test.com" &&
                            (u.Name.ToLower().Contains("sacha") ||
                             (u.Surname != null && u.Surname.ToLower().Contains("del barrio")) ||
                             (u.Email != null && u.Email.ToLower().Contains("sacha"))))
                .ToListAsync();

            foreach (var dupUser in duplicateSachaUsers)
            {
                await userManager.DeleteAsync(dupUser);
            }

            // 1. Asegurar profesionales y estado Approved
            // Sacha Del Barrio
            var sachaEmail = "sacha.delbarrio@test.com";
            var sachaUser = await userManager.FindByEmailAsync(sachaEmail);
            Professional sachaProf;
            if (sachaUser == null)
            {
                sachaUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000023"),
                    Name = "Sacha",
                    Surname = "Del Barrio",
                    Email = sachaEmail,
                    UserName = sachaEmail,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(sachaUser, "Sacha123!");
                await userManager.AddToRoleAsync(sachaUser, IdentityRoles.Professional.ToString());

                sachaProf = new Professional
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000203"),
                    UserId = sachaUser.Id,
                    FirstName = "Sacha",
                    LastName = "Del Barrio",
                    LicenseNumber = "31293",
                    Specialty = "Educacion",
                    Status = ProfessionalStatusEnum.Approved,
                    ValidatedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Professionals.Add(sachaProf);
            }
            else
            {
                sachaProf = await context.Professionals.FirstAsync(p => p.UserId == sachaUser.Id);
                sachaProf.Status = ProfessionalStatusEnum.Approved;
            }

            // Sofía Gutiérrez
            var sofiaEmail = "profesional2@test.com"; // De la semilla existente
            var sofiaUser = await userManager.FindByEmailAsync(sofiaEmail);
            var sofiaProf = await context.Professionals.FirstAsync(p => p.UserId == sofiaUser.Id);
            sofiaProf.Status = ProfessionalStatusEnum.Approved;

            // Pedro Martinez
            var pedroEmail = "profesional@test.com"; // De la semilla existente
            var pedroUser = await userManager.FindByEmailAsync(pedroEmail);
            var pedroProf = await context.Professionals.FirstAsync(p => p.UserId == pedroUser.Id);
            pedroProf.Status = ProfessionalStatusEnum.Approved;

            await context.SaveChangesAsync();

            // 2. Crear las Aulas si no existen
            if (!await context.Classrooms.AnyAsync(c => c.Name == "Aula Sacha Mañana"))
                context.Classrooms.Add(new Classroom { Id = Guid.NewGuid(), Name = "Aula Sacha Mañana", ProfessionalId = sachaProf.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            if (!await context.Classrooms.AnyAsync(c => c.Name == "Aula Sacha Tarde"))
                context.Classrooms.Add(new Classroom { Id = Guid.NewGuid(), Name = "Aula Sacha Tarde", ProfessionalId = sachaProf.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            if (!await context.Classrooms.AnyAsync(c => c.Name == "Aula Sofía A"))
                context.Classrooms.Add(new Classroom { Id = Guid.NewGuid(), Name = "Aula Sofía A", ProfessionalId = sofiaProf.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            if (!await context.Classrooms.AnyAsync(c => c.Name == "Aula Sofía B"))
                context.Classrooms.Add(new Classroom { Id = Guid.NewGuid(), Name = "Aula Sofía B", ProfessionalId = sofiaProf.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            if (!await context.Classrooms.AnyAsync(c => c.Name == "Aula Pedro Integradora"))
                context.Classrooms.Add(new Classroom { Id = Guid.NewGuid(), Name = "Aula Pedro Integradora", ProfessionalId = pedroProf.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
            if (!await context.Classrooms.AnyAsync(c => c.Name == "Aula Pedro Avanzada"))
                context.Classrooms.Add(new Classroom { Id = Guid.NewGuid(), Name = "Aula Pedro Avanzada", ProfessionalId = pedroProf.Id, IsActive = true, CreatedAt = DateTime.UtcNow });

            await context.SaveChangesAsync();
        }
    }
}
