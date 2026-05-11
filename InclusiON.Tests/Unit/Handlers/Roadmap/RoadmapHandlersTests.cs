using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Application.UseCases.Roadmap.Handlers;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Roadmap
{
    // ════════════════════════════════════════════════════════════════════════════
    // GetPersonRoadmapQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetPersonRoadmapQueryHandlerTests
    {
        private readonly IRoadmapRepository _roadmaps = Substitute.For<IRoadmapRepository>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
        private GetPersonRoadmapQueryHandler BuildSut() => new(_roadmaps, _encryption);

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid ProfId   = Guid.NewGuid();

        private static PersonRoadmap AFullRoadmap() => new()
        {
            Id                      = 1,
            PersonId                = PersonId,
            CreatedByProfessionalId = ProfId,
            Notes                   = "Plan inicial",
            CreatedAt               = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedByProfessional   = new Professional { FirstName = "Juan", LastName = "Pérez" },
            Areas                   = new List<PersonRoadmapArea>()
        };

        [Fact]
        public async Task HandleAsync_RoadmapNotFound_ReturnsNotFound()
        {
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmap?)null);

            var result = await BuildSut().HandleAsync(new GetPersonRoadmapQuery(PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_RoadmapExists_MapsAllFields()
        {
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                     .Returns(AFullRoadmap());

            var result = await BuildSut().HandleAsync(new GetPersonRoadmapQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(1);
            result.Data.PersonId.Should().Be(PersonId);
            result.Data.CreatedByProfessionalId.Should().Be(ProfId);
            result.Data.CreatedByProfessionalFullName.Should().Be("Juan Pérez");
            result.Data.Notes.Should().Be("Plan inicial");
            result.Data.Areas.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_RoadmapWithAreas_OrdersByDisplayOrder()
        {
            var roadmap = AFullRoadmap();
            roadmap.Areas = new List<PersonRoadmapArea>
            {
                new() { Id = 10, SkillAreaId = 2, DisplayOrder = 2, SkillArea = new SkillArea { Name = "Autonomía", Color = "#0F0", Icon = "star" }, Activities = new List<PersonRoadmapActivity>() },
                new() { Id = 11, SkillAreaId = 1, DisplayOrder = 1, SkillArea = new SkillArea { Name = "Comunicación", Color = "#F00", Icon = "chat" }, Activities = new List<PersonRoadmapActivity>() }
            };

            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                     .Returns(roadmap);

            var result = await BuildSut().HandleAsync(new GetPersonRoadmapQuery(PersonId), default);

            result.Data!.Areas.Should().HaveCount(2);
            result.Data.Areas[0].DisplayOrder.Should().Be(1);
            result.Data.Areas[1].DisplayOrder.Should().Be(2);
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // CreateRoadmapCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class CreateRoadmapCommandHandlerTests
    {
        private readonly IRoadmapRepository       _roadmaps      = Substitute.For<IRoadmapRepository>();
        private readonly IProfessionalsRepository _professionals = Substitute.For<IProfessionalsRepository>();
        private readonly IUnitOfWork              _uow           = Substitute.For<IUnitOfWork>();
        private readonly IEncryptionService       _encryption    = Substitute.For<IEncryptionService>();

        private CreateRoadmapCommandHandler BuildSut() => new(_roadmaps, _professionals, _uow, _encryption);

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid ProfId   = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_RoadmapAlreadyExists_ReturnsConflict()
        {
            _roadmaps.ExistsForPersonAsync(PersonId, Arg.Any<CancellationToken>()).Returns(true);

            var result = await BuildSut().HandleAsync(
                new CreateRoadmapCommand(PersonId, ProfId, null), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsNotFound()
        {
            _roadmaps.ExistsForPersonAsync(PersonId, Arg.Any<CancellationToken>()).Returns(false);
            _professionals.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                          .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(
                new CreateRoadmapCommand(PersonId, ProfId, null), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ValidCommand_CreatesRoadmapAndSaves()
        {
            var professional = new Professional { Id = ProfId, FirstName = "Ana", LastName = "García" };
            _roadmaps.ExistsForPersonAsync(PersonId, Arg.Any<CancellationToken>()).Returns(false);
            _professionals.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(professional);
            _roadmaps.CreateAsync(Arg.Any<PersonRoadmap>(), Arg.Any<CancellationToken>())
                     .Returns(ci => ci.Arg<PersonRoadmap>());

            var result = await BuildSut().HandleAsync(
                new CreateRoadmapCommand(PersonId, ProfId, "Plan de trabajo"), default);

            result.Success.Should().BeTrue();
            result.Data!.PersonId.Should().Be(PersonId);
            result.Data.CreatedByProfessionalId.Should().Be(ProfId);
            result.Data.Notes.Should().Be("Plan de trabajo");
            result.Data.CreatedByProfessionalFullName.Should().Be("Ana García");
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_ValidCommand_ReturnsAreasEmpty()
        {
            var professional = new Professional { Id = ProfId, FirstName = "Ana", LastName = "García" };
            _roadmaps.ExistsForPersonAsync(PersonId, Arg.Any<CancellationToken>()).Returns(false);
            _professionals.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(professional);
            _roadmaps.CreateAsync(Arg.Any<PersonRoadmap>(), Arg.Any<CancellationToken>())
                     .Returns(ci => ci.Arg<PersonRoadmap>());

            var result = await BuildSut().HandleAsync(
                new CreateRoadmapCommand(PersonId, ProfId, null), default);

            result.Data!.Areas.Should().BeEmpty();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // UpdateRoadmapNotesCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class UpdateRoadmapNotesCommandHandlerTests
    {
        private readonly IRoadmapRepository _roadmaps = Substitute.For<IRoadmapRepository>();
        private readonly IUnitOfWork        _uow      = Substitute.For<IUnitOfWork>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
        private UpdateRoadmapNotesCommandHandler BuildSut() => new(_roadmaps, _uow, _encryption);

        private static readonly Guid PersonId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_RoadmapNotFound_ReturnsNotFound()
        {
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmap?)null);

            var result = await BuildSut().HandleAsync(
                new UpdateRoadmapNotesCommand(PersonId, "Nuevas notas"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ValidCommand_UpdatesNotesAndSaves()
        {
            var roadmap = new PersonRoadmap
            {
                Id                    = 1,
                PersonId              = PersonId,
                Notes                 = "Notas viejas",
                CreatedByProfessional = new Professional { FirstName = "X", LastName = "Y" },
                Areas                 = new List<PersonRoadmapArea>()
            };
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(roadmap);

            var result = await BuildSut().HandleAsync(
                new UpdateRoadmapNotesCommand(PersonId, "Notas nuevas"), default);

            result.Success.Should().BeTrue();
            roadmap.Notes.Should().Be("Notas nuevas");
            result.Data!.Notes.Should().Be("Notas nuevas");
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // AddRoadmapAreaCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class AddRoadmapAreaCommandHandlerTests
    {
        private readonly IRoadmapRepository             _roadmaps   = Substitute.For<IRoadmapRepository>();
        private readonly IReadOnlyRepository<SkillArea> _skillAreas = Substitute.For<IReadOnlyRepository<SkillArea>>();
        private readonly IUnitOfWork                    _uow        = Substitute.For<IUnitOfWork>();
        private readonly IEncryptionService             _encryption = Substitute.For<IEncryptionService>();
        private AddRoadmapAreaCommandHandler BuildSut() => new(_roadmaps, _skillAreas, _uow, _encryption);

        private static readonly Guid PersonId   = Guid.NewGuid();
        private const int            SkillAreaId = 3;

        private static SkillArea AnArea() =>
            new() { Id = SkillAreaId, Name = "Comunicación", Color = "#F00", Icon = "chat", IsActive = true };

        private static PersonRoadmap ARoadmap() =>
            new()
            {
                Id       = 1,
                PersonId = PersonId,
                CreatedByProfessional = new Professional { FirstName = "X", LastName = "Y" },
                Areas    = new List<PersonRoadmapArea>()
            };

        [Fact]
        public async Task HandleAsync_RoadmapNotFound_ReturnsNotFound()
        {
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmap?)null);

            var result = await BuildSut().HandleAsync(
                new AddRoadmapAreaCommand(PersonId, SkillAreaId, 1), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_SkillAreaNotFound_ReturnsNotFound()
        {
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(ARoadmap());
            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>())
                       .Returns((SkillArea?)null);

            var result = await BuildSut().HandleAsync(
                new AddRoadmapAreaCommand(PersonId, SkillAreaId, 1), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_AreaAlreadyInRoadmap_ReturnsConflict()
        {
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(ARoadmap());
            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>()).Returns(AnArea());
            _roadmaps.AreaExistsInRoadmapAsync(1, SkillAreaId, Arg.Any<CancellationToken>()).Returns(true);

            var result = await BuildSut().HandleAsync(
                new AddRoadmapAreaCommand(PersonId, SkillAreaId, 1), default);

            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        [Fact]
        public async Task HandleAsync_ValidCommand_AddsAreaAndSaves()
        {
            _roadmaps.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(ARoadmap());
            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>()).Returns(AnArea());
            _roadmaps.AreaExistsInRoadmapAsync(1, SkillAreaId, Arg.Any<CancellationToken>()).Returns(false);

            var result = await BuildSut().HandleAsync(
                new AddRoadmapAreaCommand(PersonId, SkillAreaId, 2), default);

            result.Success.Should().BeTrue();
            result.Data!.SkillAreaId.Should().Be(SkillAreaId);
            result.Data.SkillAreaName.Should().Be("Comunicación");
            result.Data.DisplayOrder.Should().Be(2);
            result.Data.Activities.Should().BeEmpty();
            await _roadmaps.Received(1).AddAreaAsync(
                Arg.Is<PersonRoadmapArea>(a => a.SkillAreaId == SkillAreaId && a.DisplayOrder == 2),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // RemoveRoadmapAreaCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class RemoveRoadmapAreaCommandHandlerTests
    {
        private readonly IRoadmapRepository _roadmaps = Substitute.For<IRoadmapRepository>();
        private readonly IUnitOfWork        _uow      = Substitute.For<IUnitOfWork>();
        private RemoveRoadmapAreaCommandHandler BuildSut() => new(_roadmaps, _uow);

        [Fact]
        public async Task HandleAsync_AreaNotFound_ReturnsNotFound()
        {
            _roadmaps.GetAreaByIdAsync(99, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmapArea?)null);

            var result = await BuildSut().HandleAsync(new RemoveRoadmapAreaCommand(99), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_AreaFound_RemovesAndSaves()
        {
            var area = new PersonRoadmapArea { Id = 5, SkillAreaId = 1 };
            _roadmaps.GetAreaByIdAsync(5, Arg.Any<CancellationToken>()).Returns(area);

            var result = await BuildSut().HandleAsync(new RemoveRoadmapAreaCommand(5), default);

            result.Success.Should().BeTrue();
            _roadmaps.Received(1).RemoveArea(area);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_AreaNotFound_NeverSaves()
        {
            _roadmaps.GetAreaByIdAsync(99, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmapArea?)null);

            await BuildSut().HandleAsync(new RemoveRoadmapAreaCommand(99), default);

            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // AddRoadmapActivityCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class AddRoadmapActivityCommandHandlerTests
    {
        private readonly IRoadmapRepository    _roadmaps   = Substitute.For<IRoadmapRepository>();
        private readonly IActivitiesRepository _activities = Substitute.For<IActivitiesRepository>();
        private readonly IUnitOfWork           _uow        = Substitute.For<IUnitOfWork>();
        private readonly IEncryptionService    _encryption = Substitute.For<IEncryptionService>();
        private AddRoadmapActivityCommandHandler BuildSut() => new(_roadmaps, _activities, _uow, _encryption);

        private const int AreaId       = 10;
        private const int ActivityId   = 42;

        private static Activity AnActivity() =>
            new() { Id = ActivityId, Title = "Actividad Demo", IsStandardActivity = true, IsActive = true };

        private static readonly Guid ProfId = Guid.NewGuid();

        private static AddRoadmapActivityCommand ACmd(int sequenceOrder = 2) =>
            new(AreaId, ActivityId, ProfId, sequenceOrder, 60, null, null, true, 1);

        [Fact]
        public async Task HandleAsync_AreaNotFound_ReturnsNotFound()
        {
            _roadmaps.GetAreaByIdAsync(AreaId, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmapArea?)null);

            var result = await BuildSut().HandleAsync(ACmd(), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ActivityNotFound_ReturnsNotFound()
        {
            _roadmaps.GetAreaByIdAsync(AreaId, Arg.Any<CancellationToken>())
                     .Returns(new PersonRoadmapArea { Id = AreaId, SkillArea = new SkillArea() });
            _activities.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                       .Returns((Activity?)null);

            var result = await BuildSut().HandleAsync(ACmd(), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ActivityAlreadyInArea_ReturnsConflict()
        {
            _roadmaps.GetAreaByIdAsync(AreaId, Arg.Any<CancellationToken>())
                     .Returns(new PersonRoadmapArea { Id = AreaId, SkillArea = new SkillArea() });
            _activities.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>()).Returns(AnActivity());
            _roadmaps.ActivityExistsInAreaAsync(AreaId, ActivityId, Arg.Any<CancellationToken>()).Returns(true);

            var result = await BuildSut().HandleAsync(ACmd(), default);

            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        [Fact]
        public async Task HandleAsync_ValidCommand_AddsActivityAndSaves()
        {
            _roadmaps.GetAreaByIdAsync(AreaId, Arg.Any<CancellationToken>())
                     .Returns(new PersonRoadmapArea { Id = AreaId, SkillArea = new SkillArea() });
            _activities.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>()).Returns(AnActivity());
            _roadmaps.ActivityExistsInAreaAsync(AreaId, ActivityId, Arg.Any<CancellationToken>()).Returns(false);

            var result = await BuildSut().HandleAsync(ACmd(sequenceOrder: 2), default);

            result.Success.Should().BeTrue();
            result.Data!.ActivityId.Should().Be(ActivityId);
            result.Data.ActivityTitle.Should().Be("Actividad Demo");
            result.Data.SequenceOrder.Should().Be(2);
            result.Data.IsUnlocked.Should().BeFalse();  // sequenceOrder != 1 → locked
            await _roadmaps.Received(1).AddActivityAsync(
                Arg.Is<PersonRoadmapActivity>(a => a.ActivityId == ActivityId && !a.IsUnlocked),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_FirstActivity_StartsUnlocked()
        {
            _roadmaps.GetAreaByIdAsync(AreaId, Arg.Any<CancellationToken>())
                     .Returns(new PersonRoadmapArea { Id = AreaId, SkillArea = new SkillArea() });
            _activities.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>()).Returns(AnActivity());
            _roadmaps.ActivityExistsInAreaAsync(AreaId, ActivityId, Arg.Any<CancellationToken>()).Returns(false);

            var result = await BuildSut().HandleAsync(ACmd(sequenceOrder: 1), default);

            result.Success.Should().BeTrue();
            result.Data!.IsUnlocked.Should().BeTrue();
            result.Data.UnlockedAt.Should().NotBeNull();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // RemoveRoadmapActivityCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class RemoveRoadmapActivityCommandHandlerTests
    {
        private readonly IRoadmapRepository _roadmaps = Substitute.For<IRoadmapRepository>();
        private readonly IUnitOfWork        _uow      = Substitute.For<IUnitOfWork>();
        private RemoveRoadmapActivityCommandHandler BuildSut() => new(_roadmaps, _uow);

        [Fact]
        public async Task HandleAsync_ActivityNotFound_ReturnsNotFound()
        {
            _roadmaps.GetActivityByIdAsync(77, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmapActivity?)null);

            var result = await BuildSut().HandleAsync(new RemoveRoadmapActivityCommand(77), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ActivityFound_RemovesAndSaves()
        {
            var activity = new PersonRoadmapActivity { Id = 77, Activity = new Activity() };
            _roadmaps.GetActivityByIdAsync(77, Arg.Any<CancellationToken>()).Returns(activity);

            var result = await BuildSut().HandleAsync(new RemoveRoadmapActivityCommand(77), default);

            result.Success.Should().BeTrue();
            _roadmaps.Received(1).RemoveActivity(activity);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // UnlockRoadmapActivityCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class UnlockRoadmapActivityCommandHandlerTests
    {
        private readonly IRoadmapRepository _roadmaps = Substitute.For<IRoadmapRepository>();
        private readonly IUnitOfWork        _uow      = Substitute.For<IUnitOfWork>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
        private UnlockRoadmapActivityCommandHandler BuildSut() => new(_roadmaps, _uow, _encryption);

        [Fact]
        public async Task HandleAsync_ActivityNotFound_ReturnsNotFound()
        {
            _roadmaps.GetActivityByIdAsync(55, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmapActivity?)null);

            var result = await BuildSut().HandleAsync(new UnlockRoadmapActivityCommand(55), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_AlreadyUnlocked_ReturnsConflict()
        {
            var activity = new PersonRoadmapActivity
            {
                Id         = 55,
                IsUnlocked = true,
                Activity   = new Activity { Title = "Demo" }
            };
            _roadmaps.GetActivityByIdAsync(55, Arg.Any<CancellationToken>()).Returns(activity);

            var result = await BuildSut().HandleAsync(new UnlockRoadmapActivityCommand(55), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        [Fact]
        public async Task HandleAsync_Locked_SetsUnlockedAndSaves()
        {
            var activity = new PersonRoadmapActivity
            {
                Id         = 55,
                IsUnlocked = false,
                Activity   = new Activity { Title = "Demo" }
            };
            _roadmaps.GetActivityByIdAsync(55, Arg.Any<CancellationToken>()).Returns(activity);

            var result = await BuildSut().HandleAsync(new UnlockRoadmapActivityCommand(55), default);

            result.Success.Should().BeTrue();
            activity.IsUnlocked.Should().BeTrue();
            activity.UnlockedAt.Should().NotBeNull();
            result.Data!.IsUnlocked.Should().BeTrue();
            result.Data.UnlockedAt.Should().NotBeNull();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_Locked_NeverSavesOnNotFound()
        {
            _roadmaps.GetActivityByIdAsync(55, Arg.Any<CancellationToken>())
                     .Returns((PersonRoadmapActivity?)null);

            await BuildSut().HandleAsync(new UnlockRoadmapActivityCommand(55), default);

            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // ReorderRoadmapActivitiesCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class ReorderRoadmapActivitiesCommandHandlerTests
    {
        private readonly IRoadmapRepository _roadmaps = Substitute.For<IRoadmapRepository>();
        private readonly IUnitOfWork        _uow      = Substitute.For<IUnitOfWork>();

        private ReorderRoadmapActivitiesCommandHandler BuildSut() =>
            new(_roadmaps, _uow);

        private static PersonRoadmapActivity ARoadmapActivity(int id, int order = 1) => new()
        {
            Id            = id,
            SequenceOrder = order,
        };

        [Fact]
        public async Task NoActivitiesInArea_ReturnsNotFound()
        {
            _roadmaps.GetActivitiesByAreaIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(new List<PersonRoadmapActivity>());

            var cmd    = new ReorderRoadmapActivitiesCommand(1, [(1, 1)]);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task ActivityIdNotInArea_ReturnsValidationFailed()
        {
            _roadmaps.GetActivitiesByAreaIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(new List<PersonRoadmapActivity>
                     {
                         ARoadmapActivity(10),
                         ARoadmapActivity(20),
                     });

            var cmd    = new ReorderRoadmapActivitiesCommand(1, [(10, 1), (99, 2)]); // 99 not in area
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValidReorder_UpdatesSequenceOrdersAndSaves()
        {
            var act10 = ARoadmapActivity(10, order: 1);
            var act20 = ARoadmapActivity(20, order: 2);

            _roadmaps.GetActivitiesByAreaIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(new List<PersonRoadmapActivity> { act10, act20 });

            var cmd    = new ReorderRoadmapActivitiesCommand(1, [(10, 2), (20, 1)]);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeTrue();
            act10.SequenceOrder.Should().Be(2);
            act20.SequenceOrder.Should().Be(1);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
