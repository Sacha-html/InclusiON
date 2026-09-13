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
            var studentId = Guid.NewGuid();
            context.SkillAreas.Add(new SkillArea { Id = 1, Name = "Trayectoria" });
            await context.SaveChangesAsync();

            await new RoadmapInitializer(context).InitializeStudentRoadmapAsync(studentId);

            context.Activities.Should().HaveCount(10);
            context.Activities.Should().OnlyContain(activity => activity.ProfessionalId == null);
            context.PersonRoadmaps.Should().BeEmpty();
            context.ActivityAssignments.Should().BeEmpty();
        }

        [Fact]
        public async Task InitializeStudentRoadmapAsync_WithProfessional_CreatesRoadmapAndInitialAssignment()
        {
            await using var context = CreateContext();
            var studentId = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            context.SkillAreas.Add(new SkillArea { Id = 1, Name = "Trayectoria" });
            context.Professionals.Add(new Professional { Id = professionalId, UserId = Guid.NewGuid() });
            await context.SaveChangesAsync();

            await new RoadmapInitializer(context).InitializeStudentRoadmapAsync(studentId);

            context.PersonRoadmaps.Should().ContainSingle(roadmap =>
                roadmap.PersonId == studentId && roadmap.CreatedByProfessionalId == professionalId);
            context.Activities.Should().HaveCount(10);
            context.Activities.Should().OnlyContain(activity => activity.ProfessionalId == professionalId);
            context.ActivityAssignments.Should().ContainSingle(assignment =>
                assignment.PersonId == studentId && assignment.AssignedByProfessionalId == professionalId);
        }
    }
}
