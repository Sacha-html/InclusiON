using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.Infrastructure.Services;

namespace InclusiON.Tests.Unit.Services
{
    public class RoadmapInitializerTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static async Task SeedCatalogAsync(
            AppDbContext context,
            bool includeTemplateTypes = true,
            bool includePuzzle = true)
        {
            context.SkillAreas.Add(new SkillArea { Id = 1, Name = "Trayectoria" });
            context.ActivityCategories.AddRange(
                new ActivityCategory { Id = 1, Name = "Lectoescritura" },
                new ActivityCategory { Id = 2, Name = "Numeración y Matemática" },
                new ActivityCategory { Id = 3, Name = "Habilidades Socioemocionales" },
                new ActivityCategory { Id = 4, Name = "Comunicación y Lenguaje" },
                new ActivityCategory { Id = 5, Name = "Motricidad y Coordinación" },
                new ActivityCategory { Id = 7, Name = "Autonomía y Vida Diaria" });

            if (includeTemplateTypes)
            {
                var codes = (includePuzzle ? new[] { "PUZZLE" } : Array.Empty<string>())
                    .Concat(new[] { "PICTOGRAM_SELECT", "SOUND_RECOGNITION", "GLOBAL_READING", "NUMERATION", "ORDER_SEQUENCE", "OPTION_SELECT", "CLASSIFY", "BUILD_WORD" })
                    .ToArray();
                context.ActivityTemplateTypes.AddRange(codes.Select((code, index) => new ActivityTemplateType
                {
                    Id = index + 1,
                    SkillAreaId = 1,
                    Code = code,
                    Name = code,
                    ContentSchema = "{}",
                    ComponentName = code
                }));
            }

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_WithoutTrayectoria_DoesNotCreateSkillAreaOrActivities()
        {
            await using var context = CreateContext();

            await new RoadmapInitializer(context).InitializeStudentRoadmapAsync(Guid.NewGuid());

            context.SkillAreas.Should().BeEmpty();
            context.Activities.Should().BeEmpty();
            context.PersonRoadmaps.Should().BeEmpty();
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_WithoutProfessionals_CreatesGlobalActivitiesWithoutRoadmap()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);

            await new RoadmapInitializer(context).InitializeStudentRoadmapAsync(Guid.NewGuid());

            context.Activities.Should().HaveCount(10);
            context.Activities.Should().OnlyContain(activity => activity.ProfessionalId == null);
            context.Activities.OrderBy(a => a.RoadmapOrder).Select(a => a.Title).Should().ContainInOrder(
                "Armando el vaso", "Necesito ir al baño", "¿Quién se ríe?", "Buscando la MESA",
                "Contando galletas", "Me sirvo agua", "¿Dónde hacemos pis?", "A ordenar la cocina",
                "Las letras de MESA", "La rutina de la mañana");
            context.PersonRoadmaps.Should().BeEmpty();
            context.ActivityAssignments.Should().BeEmpty();
        }

        [Fact]
        public async Task EnsureStandardActivitiesAsync_UsesPuzzleTemplateFromCompleteCatalog()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);

            await new RoadmapInitializer(context).EnsureStandardActivitiesAsync();

            context.ActivityTemplateTypes.Should().ContainSingle(t => t.Code == "PUZZLE");
            var puzzleActivity = context.Activities.Single(a => a.StandardKey == "ROADMAP_STANDARD_01_ARMANDO_EL_VASO");
            context.ActivityContents.Single(c => c.ActivityId == puzzleActivity.Id)
                .TemplateTypeId.Should().Be(context.ActivityTemplateTypes.Single(t => t.Code == "PUZZLE").Id);
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_IsIdempotentAndCreatesRoadmapOnlyOnce()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);
            var studentId = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            var professionalUserId = Guid.NewGuid();
            context.Professionals.Add(new Professional { Id = professionalId, UserId = professionalUserId });
            await context.SaveChangesAsync();
            var initializer = new RoadmapInitializer(context);

            await initializer.InitializeStudentRoadmapAsync(studentId, professionalUserId);
            var firstActivityIds = context.Activities.OrderBy(a => a.Id).Select(a => a.Id).ToArray();
            await initializer.InitializeStudentRoadmapAsync(studentId, professionalUserId);

            context.Activities.Should().HaveCount(10);
            context.Activities.Should().OnlyContain(activity => activity.ProfessionalId == null);
            context.Activities.OrderBy(a => a.Id).Select(a => a.Id).Should().Equal(firstActivityIds);
            context.PersonRoadmaps.Should().ContainSingle();
            context.PersonRoadmapActivities.Should().HaveCount(10);
            context.ActivityAssignments.Should().ContainSingle();
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_WithMultipleProfessionals_UsesOnlyTheProvidedProfessional()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);
            var studentId = Guid.NewGuid();
            var firstProfessional = new Professional { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
            var assignedProfessional = new Professional { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
            context.Professionals.AddRange(firstProfessional, assignedProfessional);
            await context.SaveChangesAsync();

            await new RoadmapInitializer(context).InitializeStudentRoadmapAsync(
                studentId,
                assignedProfessional.UserId);

            context.PersonRoadmaps.Should().ContainSingle()
                .Which.CreatedByProfessionalId.Should().Be(assignedProfessional.Id);
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_WithoutValidProfessional_DoesNotChooseAnotherProfessional()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);
            context.Professionals.Add(new Professional { UserId = Guid.NewGuid() });
            await context.SaveChangesAsync();

            await new RoadmapInitializer(context).InitializeStudentRoadmapAsync(
                Guid.NewGuid(),
                Guid.NewGuid());

            context.PersonRoadmaps.Should().BeEmpty();
            context.Activities.Should().HaveCount(10);
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_LeavesCustomActivitiesUntouched()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);
            var custom = new Activity
            {
                Title = "Actividad personalizada",
                Description = "No modificar",
                CategoryId = 1,
                StandardKey = null,
                RoadmapOrder = 1,
                ProfessionalId = Guid.NewGuid()
            };
            context.Activities.Add(custom);
            await context.SaveChangesAsync();

            await new RoadmapInitializer(context).EnsureStandardActivitiesAsync();

            context.Activities.Single(a => a.Id == custom.Id).Should().BeEquivalentTo(custom);
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_WithExistingRoadmap_DoesNotModifyCatalog()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);
            var studentId = Guid.NewGuid();
            context.PersonRoadmaps.Add(new PersonRoadmap
            {
                PersonId = studentId,
                CreatedByProfessionalId = Guid.NewGuid()
            });
            await context.SaveChangesAsync();

            await new RoadmapInitializer(context).InitializeStudentRoadmapAsync(studentId);

            context.Activities.Should().BeEmpty();
        }

        [Fact]
        public async Task RepairAssignedStudentRoadmapsAsync_CreatesOnlyMissingRoadmapsUsingAssignedProfessional()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);
            var assignedStudent = Guid.NewGuid();
            var unassignedStudent = Guid.NewGuid();
            var professional = new Professional { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
            context.Professionals.Add(professional);
            context.ProfessionalPersons.Add(new ProfessionalPerson
            {
                PersonId = assignedStudent,
                ProfessionalId = professional.Id,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            });
            await context.SaveChangesAsync();

            var repaired = await new RoadmapInitializer(context).RepairAssignedStudentRoadmapsAsync();
            var repairedAgain = await new RoadmapInitializer(context).RepairAssignedStudentRoadmapsAsync();

            repaired.Should().Be(1);
            repairedAgain.Should().Be(0);
            context.PersonRoadmaps.Should().ContainSingle(r =>
                r.PersonId == assignedStudent && r.CreatedByProfessionalId == professional.Id);
            context.PersonRoadmaps.Should().NotContain(r => r.PersonId == unassignedStudent);
        }

        [Fact]
        public async Task RepairAssignedStudentRoadmapsAsync_PreservesExistingRoadmapAndProgress()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context);
            var studentId = Guid.NewGuid();
            var professional = new Professional { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
            var existingRoadmap = new PersonRoadmap
            {
                PersonId = studentId,
                CreatedByProfessionalId = professional.Id,
                Notes = "Progreso existente"
            };
            context.Professionals.Add(professional);
            context.PersonRoadmaps.Add(existingRoadmap);
            context.ProfessionalPersons.Add(new ProfessionalPerson
            {
                PersonId = studentId,
                ProfessionalId = professional.Id,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            });
            await context.SaveChangesAsync();

            var repaired = await new RoadmapInitializer(context).RepairAssignedStudentRoadmapsAsync();

            repaired.Should().Be(0);
            var preserved = context.PersonRoadmaps.Single();
            preserved.Id.Should().Be(existingRoadmap.Id);
            preserved.PersonId.Should().Be(studentId);
            preserved.CreatedByProfessionalId.Should().Be(professional.Id);
            preserved.Notes.Should().Be("Progreso existente");
            context.PersonRoadmapActivities.Should().BeEmpty();
        }

        [Fact]
        public async Task EnsureStandardActivitiesAsync_ThrowsWhenTemplateCodeIsMissingInsteadOfUsingFallback()
        {
            await using var context = CreateContext();
            await SeedCatalogAsync(context, includePuzzle: false);

            var action = () => new RoadmapInitializer(context).EnsureStandardActivitiesAsync();

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*activity template type with code*");
            context.Activities.Should().BeEmpty();
        }
    }
}
