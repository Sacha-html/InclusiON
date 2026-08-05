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

            // Inicializar Roadmap Estándar para todos los alumnos existentes y resetear todo su progreso a Nivel 0
            var students = await context.PersonsWithDisability.Include(p => p.User).ToListAsync();
            foreach (var student in students)
            {
                // 1. Borrar todas las respuestas de actividades (ActivityResponse) y asignaciones de la persona
                var studentAssignments = await context.Set<ActivityAssignment>()
                    .Where(a => a.PersonId == student.Id)
                    .ToListAsync();
                if (studentAssignments.Count > 0)
                {
                    var assignmentIds = studentAssignments.Select(a => a.Id).ToList();
                    var responses = await context.Set<ActivityResponse>()
                        .Where(r => assignmentIds.Contains(r.AssignmentId))
                        .ToListAsync();
                    if (responses.Count > 0)
                    {
                        context.Set<ActivityResponse>().RemoveRange(responses);
                        await context.SaveChangesAsync();
                    }

                    context.Set<ActivityAssignment>().RemoveRange(studentAssignments);
                    await context.SaveChangesAsync();
                }

                // 2. Borrar cualquier roadmap previo (PersonRoadmap, PersonRoadmapArea, PersonRoadmapActivity)
                var existingRoadmaps = await context.PersonRoadmaps
                    .Where(r => r.PersonId == student.Id)
                    .ToListAsync();
                if (existingRoadmaps.Count > 0)
                {
                    context.PersonRoadmaps.RemoveRange(existingRoadmaps);
                    await context.SaveChangesAsync();
                }

                // 3. Inicializar roadmap estándar a Nivel 0 (Nivel 1 desbloqueado y pendiente, niveles 2-10 bloqueados)
                await RoadmapInitializerAccessor.InitializeStudentRoadmap(context, student.Id, student.SupervisorUserId, CancellationToken.None);
            }
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
                    Pin = (string?)null,
                    LoginMethodId = 1, // STANDARD (password)
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
                    Password = "Fam123!",
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
                }
            };

            foreach (var fam in families)
            {
                // Crear usuario si no existe
                var existingUser = await userManager.FindByEmailAsync(fam.Email);
                if (existingUser == null)
                {
                    var existingById = await userManager.FindByIdAsync(fam.Id.ToString());
                    if (existingById == null)
                    {
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

                            context.FamilyRepresentatives.Add(new FamilyRepresentative
                            {
                                Id = fam.FamilyId,
                                UserId = fam.Id,
                                FirstName = fam.FirstName,
                                LastName = fam.LastName,
                                Phone = fam.Phone,
                                Relationship = fam.Relationship,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // Vincular con persona — siempre, aunque el familiar ya exista
                var familyEntity = await context.FamilyRepresentatives
                    .FirstOrDefaultAsync(f => f.User.Email == fam.Email);
                var personToLink = await context.PersonsWithDisability
                    .FirstOrDefaultAsync(p => p.User.Email == fam.LinkedPersonEmail);

                if (familyEntity != null && personToLink != null)
                {
                    var alreadyLinked = await context.PersonRepresentatives
                        .AnyAsync(pr => pr.RepresentativeId == familyEntity.Id && pr.PersonId == personToLink.Id);

                    if (!alreadyLinked)
                    {
                        context.PersonRepresentatives.Add(new PersonRepresentative
                        {
                            PersonId = personToLink.Id,
                            RepresentativeId = familyEntity.Id,
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
    }
}
