using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using Activity = InclusiON.Domain.Models.Activity;
using ActivityAssignment = InclusiON.Domain.Models.ActivityAssignment;
using DomainActivityResponse = InclusiON.Domain.Models.ActivityResponse;

namespace InclusiON.Tests.Unit.Handlers.Activities
{
    // ══════════════════════════════════════════════════════════════════════════════
    // CreateActivityCommandHandler
    // ══════════════════════════════════════════════════════════════════════════════

    public class CreateActivityCommandHandlerTests
    {
        private readonly IActivitiesRepository _repo = Substitute.For<IActivitiesRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
        private readonly IBackgroundJobRepository _bgJobRepo = Substitute.For<IBackgroundJobRepository>();

        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        public CreateActivityCommandHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
        }

        private CreateActivityCommandHandler BuildSut() =>
            new(_repo, _uow, _dateTime, _encryption, _bgJobRepo,
                NullLogger<CreateActivityCommandHandler>.Instance);

        private static CreateActivityCommand Cmd() => new(
            ProfId, "Colores primarios", "Descripción", "Instrucciones",
            CategoryId: 1, SkillAreaId: null, ComplexityLevel: 2,
            EstimatedDurationMinutes: 30, RequiresSupervision: false,
            HasVisualSupport: true, HasAudioSupport: false,
            UsesEasyReading: true, UsesPictograms: false,
            ResourcesUrl: null, TemplateTypeId: 1, ContentJson: "{\"items\":[{\"id\":1}]}");

        private static Activity ACreatedActivity(int id = 1) => new()
        {
            Id = id, Title = "Colores primarios", ProfessionalId = ProfId,
            IsActive = true, IsStandardActivity = false,
            CreatedAt = Now
        };

        [Fact]
        public async Task ValidCommand_CreatesActivityAndReturnsSuccess()
        {
            _dateTime.UtcNow.Returns(Now);
            var created = ACreatedActivity();
            _repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(created);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Title.Should().Be("Colores primarios");
            await _repo.Received(1).CreateAsync(Arg.Any<Activity>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ExceptionInCreate_ReturnsInternalError()
        {
            _dateTime.UtcNow.Returns(Now);
            _repo.CreateAsync(Arg.Any<Activity>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("DB error"));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InternalError);
        }

        [Fact]
        public async Task InvalidJson_ReturnsValidationFailed()
        {
            var cmd = Cmd() with { ContentJson = "esto no es json" };

            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
            await _repo.DidNotReceive().CreateAsync(Arg.Any<Activity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EmptyObjectJson_ReturnsValidationFailed()
        {
            var cmd = Cmd() with { ContentJson = "{}" };

            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
            await _repo.DidNotReceive().CreateAsync(Arg.Any<Activity>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("{\"items\":[{\"id\":1}]}")]
        [InlineData("{\"steps\":[\"A\",\"B\"]}")]
        [InlineData("{\"groups\":{\"a\":1}}")]
        public async Task NonEmptyValidJson_DoesNotFailOnContentValidation(string contentJson)
        {
            _dateTime.UtcNow.Returns(Now);
            var created = ACreatedActivity();
            _repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(created);
            var cmd = Cmd() with { ContentJson = contentJson };

            var result = await BuildSut().HandleAsync(cmd, default);

            result.ErrorCode.Should().NotBe(ErrorCode.ValidationFailed);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // UpdateActivityCommandHandler
    // ══════════════════════════════════════════════════════════════════════════════

    public class UpdateActivityCommandHandlerTests
    {
        private readonly IActivitiesRepository _repo = Substitute.For<IActivitiesRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
        private readonly IBackgroundJobRepository _bgJobRepo = Substitute.For<IBackgroundJobRepository>();

        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly Guid OtherProfId = Guid.NewGuid();
        private const int ActivityId = 5;
        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        public UpdateActivityCommandHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
        }

        private UpdateActivityCommandHandler BuildSut() =>
            new(_repo, _uow, _dateTime, _encryption, _bgJobRepo,
                NullLogger<UpdateActivityCommandHandler>.Instance);

        private static UpdateActivityCommand Cmd(Guid? profId = null) => new(
            ActivityId, profId ?? ProfId, "Nuevo título", null, null,
            CategoryId: 1, SkillAreaId: null, ComplexityLevel: 2,
            EstimatedDurationMinutes: 30, RequiresSupervision: false,
            HasVisualSupport: true, HasAudioSupport: false,
            UsesEasyReading: true, UsesPictograms: false,
            ResourcesUrl: null, ContentJson: "{\"items\":[{\"id\":1}]}");

        private static Activity AnActivity(bool isStandard = false, Guid? profId = null) => new()
        {
            Id = ActivityId, Title = "Título original",
            ProfessionalId = profId ?? ProfId,
            IsStandardActivity = isStandard, IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1)
        };

        [Fact]
        public async Task ActivityNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns((Activity?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task IsStandardActivity_ReturnsForbidden()
        {
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(isStandard: true));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task DifferentProfessional_ReturnsForbidden()
        {
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(profId: OtherProfId));

            var result = await BuildSut().HandleAsync(Cmd(profId: ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task ValidUpdate_UpdatesFieldsAndSaves()
        {
            var activity = AnActivity();
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(activity, AnActivity());  // second call is the reload
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            activity.Title.Should().Be("Nuevo título");
            activity.UpdatedAt.Should().Be(Now);
            await _repo.Received(1).UpdateAsync(activity, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // PatchActivityStatusCommandHandler
    // ══════════════════════════════════════════════════════════════════════════════

    public class PatchActivityStatusCommandHandlerTests
    {
        private readonly IActivitiesRepository _repo = Substitute.For<IActivitiesRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly Guid OtherProfId = Guid.NewGuid();
        private const int ActivityId = 7;
        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        public PatchActivityStatusCommandHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
        }

        private PatchActivityStatusCommandHandler BuildSut() =>
            new(_repo, _uow, _dateTime, _encryption);

        private static Activity AnActivity(bool isActive = true, Guid? profId = null) => new()
        {
            Id = ActivityId, Title = "Test", ProfessionalId = profId ?? ProfId,
            IsActive = isActive, IsStandardActivity = false,
            CreatedAt = new DateTime(2025, 1, 1)
        };

        [Fact]
        public async Task ActivityNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns((Activity?)null);

            var result = await BuildSut().HandleAsync(
                new PatchActivityStatusCommand(ActivityId, ProfId, false), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task DifferentProfessional_ReturnsForbidden()
        {
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(profId: OtherProfId));

            var result = await BuildSut().HandleAsync(
                new PatchActivityStatusCommand(ActivityId, ProfId, false), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task AlreadyInactive_ReturnsConflict()
        {
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(isActive: false));

            var result = await BuildSut().HandleAsync(
                new PatchActivityStatusCommand(ActivityId, ProfId, false), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task DeactivateWithActiveAssignments_ReturnsConflict()
        {
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(isActive: true));
            _repo.HasActiveAssignmentsAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await BuildSut().HandleAsync(
                new PatchActivityStatusCommand(ActivityId, ProfId, false), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task ValidDeactivate_SetsInactiveAndSaves()
        {
            var activity = AnActivity(isActive: true);
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(activity, AnActivity(isActive: false));
            _repo.HasActiveAssignmentsAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(false);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(
                new PatchActivityStatusCommand(ActivityId, ProfId, false), default);

            result.Success.Should().BeTrue();
            activity.IsActive.Should().BeFalse();
            activity.UpdatedAt.Should().Be(Now);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValidReactivate_SetsActiveAndSaves()
        {
            var activity = AnActivity(isActive: false);
            _repo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(activity, AnActivity(isActive: true));
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(
                new PatchActivityStatusCommand(ActivityId, ProfId, true), default);

            result.Success.Should().BeTrue();
            activity.IsActive.Should().BeTrue();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // CreateActivityAssignmentCommandHandler
    // ══════════════════════════════════════════════════════════════════════════════

    public class CreateActivityAssignmentCommandHandlerTests
    {
        private readonly IActivityAssignmentRepository _repo = Substitute.For<IActivityAssignmentRepository>();
        private readonly IActivitiesRepository _activitiesRepo = Substitute.For<IActivitiesRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly Guid OtherProfId = Guid.NewGuid();
        private static readonly Guid PersonId = Guid.NewGuid();
        private const int ActivityId = 10;
        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // The encrypted activity ID that decrypts to "10"
        private const string EncryptedActivityId = "ENCRYPTED_10";

        public CreateActivityAssignmentCommandHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
            // Decrypt: "ENCRYPTED_10" (url-safe) → standard base64 → "10"
            _encryption.Decrypt(Arg.Any<string>()).Returns(ActivityId.ToString());
        }

        private CreateActivityAssignmentCommandHandler BuildSut() =>
            new(_repo, _activitiesRepo, _uow, _dateTime, _encryption);

        private static CreateActivityAssignmentCommand Cmd(Guid? profId = null) => new(
            EncryptedActivityId, PersonId, profId ?? ProfId,
            DueDate: null, IsEvaluationActivity: false, SequenceOrder: null);

        private static Activity AnActivity(bool isActive = true, bool isStandard = false, Guid? profId = null) => new()
        {
            Id = ActivityId, Title = "Test Activity",
            ProfessionalId = profId ?? ProfId,
            IsActive = isActive, IsStandardActivity = isStandard,
            CreatedAt = new DateTime(2025, 1, 1)
        };

        private static ActivityAssignment ACreatedAssignment() => new()
        {
            Id = 1, ActivityId = ActivityId, PersonId = PersonId,
            AssignedByProfessionalId = ProfId, StatusId = AssignmentStatuses.Pendiente,
            Status = new InclusiON.Domain.Models.ActivityAssignmentStatus { Id = AssignmentStatuses.Pendiente, Name = AssignmentStatuses.Names.Pendiente },
            AssignedAt = Now
        };

        [Fact]
        public async Task ActivityNotFound_ReturnsNotFound()
        {
            _activitiesRepo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns((Activity?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task ActivityInactive_ReturnsNotFound()
        {
            _activitiesRepo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(isActive: false));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task NonStandardActivity_DifferentProfessional_ReturnsForbidden()
        {
            _activitiesRepo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(isStandard: false, profId: OtherProfId));

            var result = await BuildSut().HandleAsync(Cmd(profId: ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task StandardActivity_AnyProfessional_Allowed()
        {
            _activitiesRepo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(isStandard: true, profId: OtherProfId));
            _dateTime.UtcNow.Returns(Now);
            var created = ACreatedAssignment();
            _repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(created);

            var result = await BuildSut().HandleAsync(Cmd(profId: ProfId), default);

            result.Success.Should().BeTrue();
            await _repo.Received(1).CreateAsync(Arg.Any<ActivityAssignment>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task OwnActivity_CreatesAssignmentAndReturnsSuccess()
        {
            _activitiesRepo.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>())
                .Returns(AnActivity(isStandard: false, profId: ProfId));
            _dateTime.UtcNow.Returns(Now);
            var created = ACreatedAssignment();
            _repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(created);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Status.Should().Be(AssignmentStatuses.Names.Pendiente);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // StartActivityResponseCommandHandler
    // ══════════════════════════════════════════════════════════════════════════════

    public class StartActivityResponseCommandHandlerTests
    {
        private readonly IActivityAssignmentRepository _repo = Substitute.For<IActivityAssignmentRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid OtherPersonId = Guid.NewGuid();
        private const int AssignmentId = 3;
        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        public StartActivityResponseCommandHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
        }

        private StartActivityResponseCommandHandler BuildSut() =>
            new(_repo, _uow, _dateTime, _encryption);

        private static ActivityAssignment AnAssignment(int status = AssignmentStatuses.Pendiente, Guid? personId = null) => new()
        {
            Id = AssignmentId, ActivityId = 1, PersonId = personId ?? PersonId,
            StatusId = status, AssignedAt = new DateTime(2025, 1, 1)
        };

        [Fact]
        public async Task AssignmentNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns((ActivityAssignment?)null);

            var result = await BuildSut().HandleAsync(
                new StartActivityResponseCommand(AssignmentId, PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task DifferentPerson_ReturnsForbidden()
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(AnAssignment(personId: OtherPersonId));

            var result = await BuildSut().HandleAsync(
                new StartActivityResponseCommand(AssignmentId, PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Theory]
        [InlineData(AssignmentStatuses.Completada)]
        [InlineData(AssignmentStatuses.Cancelada)]
        public async Task TerminalStatus_ReturnsConflict(int status)
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(AnAssignment(status: status));

            var result = await BuildSut().HandleAsync(
                new StartActivityResponseCommand(AssignmentId, PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task ValidStart_Pending_CreatesResponseAndUpdatesStatus()
        {
            var assignment = AnAssignment(status: AssignmentStatuses.Pendiente);
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(assignment, AnAssignment(status: AssignmentStatuses.EnProgreso));
            _repo.CountResponsesAsync(AssignmentId, Arg.Any<CancellationToken>()).Returns(0);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(
                new StartActivityResponseCommand(AssignmentId, PersonId), default);

            result.Success.Should().BeTrue();
            await _repo.Received(1).CreateResponseAsync(
                Arg.Is<DomainActivityResponse>(r => r.AttemptCount == 1 && r.AssignmentId == AssignmentId),
                Arg.Any<CancellationToken>());
            await _repo.Received(1).UpdateAsync(
                Arg.Is<ActivityAssignment>(a => a.StatusId == AssignmentStatuses.EnProgreso),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValidStart_AlreadyInProgress_CreatesResponseWithoutStatusUpdate()
        {
            var assignment = AnAssignment(status: AssignmentStatuses.EnProgreso);
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(assignment, AnAssignment(status: AssignmentStatuses.EnProgreso));
            _repo.CountResponsesAsync(AssignmentId, Arg.Any<CancellationToken>()).Returns(1);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(
                new StartActivityResponseCommand(AssignmentId, PersonId), default);

            result.Success.Should().BeTrue();
            await _repo.Received(1).CreateResponseAsync(
                Arg.Is<DomainActivityResponse>(r => r.AttemptCount == 2),
                Arg.Any<CancellationToken>());
            await _repo.DidNotReceive().UpdateAsync(Arg.Any<ActivityAssignment>(), Arg.Any<CancellationToken>());
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // CompleteActivityResponseCommandHandler
    // ══════════════════════════════════════════════════════════════════════════════

    public class CompleteActivityResponseCommandHandlerTests
    {
        private readonly IActivityAssignmentRepository _repo     = Substitute.For<IActivityAssignmentRepository>();
        private readonly IRoadmapRepository           _roadmaps = Substitute.For<IRoadmapRepository>();
        private readonly IUnitOfWork                  _uow      = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider            _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService           _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid OtherPersonId = Guid.NewGuid();
        private const int AssignmentId = 4;
        private const int ResponseId = 9;
        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        public CompleteActivityResponseCommandHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
        }

        private CompleteActivityResponseCommandHandler BuildSut() =>
            new(_repo, _roadmaps, _uow, _dateTime, _encryption);

        private static CompleteActivityResponseCommand Cmd(
            Guid? personId = null, decimal success = 85m) => new(
            AssignmentId, ResponseId, personId ?? PersonId,
            success, TimeSpentSeconds: 120, RequiredSupport: false,
            FrustrationLevel: null, ResponsePattern: null, Observations: null);

        private static ActivityAssignment AnAssignment(Guid? personId = null) => new()
        {
            Id = AssignmentId, ActivityId = 1, PersonId = personId ?? PersonId,
            StatusId = AssignmentStatuses.EnProgreso, AssignedAt = new DateTime(2025, 1, 1)
        };

        private static DomainActivityResponse AResponse(bool alreadyCompleted = false) => new()
        {
            Id = ResponseId, AssignmentId = AssignmentId,
            StartedAt = new DateTime(2025, 6, 1, 10, 0, 0),
            CompletedAt = alreadyCompleted ? new DateTime(2025, 6, 1, 11, 0, 0) : null,
            AttemptCount = 1
        };

        [Fact]
        public async Task AssignmentNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns((ActivityAssignment?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task DifferentPerson_ReturnsForbidden()
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(AnAssignment(personId: OtherPersonId));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task ResponseNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(AnAssignment());
            _repo.GetResponseByIdAsync(ResponseId, Arg.Any<CancellationToken>())
                .Returns((DomainActivityResponse?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task ResponseFromDifferentAssignment_ReturnsNotFound()
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(AnAssignment());
            _repo.GetResponseByIdAsync(ResponseId, Arg.Any<CancellationToken>())
                .Returns(new DomainActivityResponse { Id = ResponseId, AssignmentId = 999 });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task ResponseAlreadyCompleted_ReturnsConflict()
        {
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(AnAssignment());
            _repo.GetResponseByIdAsync(ResponseId, Arg.Any<CancellationToken>())
                .Returns(AResponse(alreadyCompleted: true));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Theory]
        [InlineData(85, ActivityResponseResult.Exito)]
        [InlineData(60, ActivityResponseResult.Parcial)]
        [InlineData(30, ActivityResponseResult.Fallido)]
        public async Task ValidComplete_SetsResultBySuccessPercentage(decimal percentage, ActivityResponseResult expectedResult)
        {
            var assignment = AnAssignment();
            var response = AResponse();
            _repo.GetByIdAsync(AssignmentId, Arg.Any<CancellationToken>())
                .Returns(assignment, AnAssignment());
            _repo.GetResponseByIdAsync(ResponseId, Arg.Any<CancellationToken>())
                .Returns(response);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(Cmd(success: percentage), default);

            result.Success.Should().BeTrue();
            response.Result.Should().Be(expectedResult);
            response.CompletedAt.Should().Be(Now);
            response.SuccessPercentage.Should().Be(percentage);
            assignment.StatusId.Should().Be(AssignmentStatuses.Completada);
            await _repo.Received(1).UpdateResponseAsync(response, Arg.Any<CancellationToken>());
            await _repo.Received(1).UpdateAsync(assignment, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
