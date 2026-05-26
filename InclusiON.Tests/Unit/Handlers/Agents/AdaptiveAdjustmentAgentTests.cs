using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Workers;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Agents;

/// <summary>
/// Unit tests for <see cref="AdaptiveAdjustmentAgent"/> — the MDA background worker.
/// </summary>
public class AdaptiveAdjustmentAgentTests
{
    private readonly IAdaptiveEngineRepository _repo    = Substitute.For<IAdaptiveEngineRepository>();
    private readonly IUnitOfWork               _uow     = Substitute.For<IUnitOfWork>();
    private readonly IBackgroundJobRepository  _jobs    = Substitute.For<IBackgroundJobRepository>();

    private AdaptiveAdjustmentAgent BuildSut() =>
        new(_repo, _uow, _jobs, NullLogger<AdaptiveAdjustmentAgent>.Instance);

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static BackgroundJob MakeJob(int praid = 1, int rid = 10, int aid = 5, string profId = "prof-1")
        => new()
        {
            Id        = 1,
            JobTypeId = JobTypes.AdaptiveAdjustment,
            StatusId  = BackgroundJobStatuses.Running,
            Payload   = JsonSerializer.Serialize(new
            {
                PersonRoadmapActivityId = praid,
                ActivityResponseId      = rid,
                AssignmentId            = aid,
                ProfessionalUserId      = profId,
            }),
            RetryCount = 0,
            MaxRetries = 3,
        };

    private static PersonRoadmapActivity MakeActivity(AdaptiveEngineConfig? config = null, int difficulty = 2)
        => new()
        {
            Id             = 1,
            DifficultyLevel = difficulty,
            AdaptiveConfig  = config,
        };

    private static AdaptiveEngineConfig DefaultConfig(bool enabled = true) => new()
    {
        IsEnabled                      = enabled,
        MinDifficultyLevel             = 1,
        MaxDifficultyLevel             = 5,
        ConsecutiveSuccessToUpgrade    = 3,
        ConsecutiveFailuresToDowngrade = 2,
        SuccessThresholdPercent        = 70,
        FrustrationThreshold           = 4,
    };

    private static ActivityResponse MakeResponse(
        ActivityResponseResult result, decimal? pct = null, int? frustration = null)
        => new()
        {
            Result            = result,
            SuccessPercentage = pct,
            FrustrationLevel  = frustration,
            CompletedAt       = DateTime.UtcNow,
        };

    // ── Config absent / disabled ────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ActivityNotFound_Returns_EarlyNoSave()
    {
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns((PersonRoadmapActivity?)null);

        await BuildSut().HandleAsync(MakeJob(), default);

        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ConfigNull_Returns_EarlyNoSave()
    {
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>())
             .Returns(MakeActivity(config: null));

        await BuildSut().HandleAsync(MakeJob(), default);

        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ConfigDisabled_Returns_EarlyNoSave()
    {
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>())
             .Returns(MakeActivity(config: DefaultConfig(enabled: false)));

        await BuildSut().HandleAsync(MakeJob(), default);

        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NoResponses_Returns_EarlyNoSave()
    {
        var config = DefaultConfig();
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>())
             .Returns(MakeActivity(config: config));
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>());

        await BuildSut().HandleAsync(MakeJob(), default);

        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── No adjustment needed ───────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_OneSuccess_BelowThreshold_NoAdjustment()
    {
        var config   = DefaultConfig();     // needs 3 consecutive for upgrade
        var activity = MakeActivity(config: config, difficulty: 2);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Exito, pct: 80m),
             });

        await BuildSut().HandleAsync(MakeJob(), default);

        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── DifficultyUp ──────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ThreeConsecutiveSuccesses_IncreasesDifficulty()
    {
        var config   = DefaultConfig();     // ConsecutiveSuccessToUpgrade = 3
        var activity = MakeActivity(config: config, difficulty: 2);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Exito, pct: 90m),
                 MakeResponse(ActivityResponseResult.Exito, pct: 85m),
                 MakeResponse(ActivityResponseResult.Exito, pct: 80m),
             });

        await BuildSut().HandleAsync(MakeJob(), default);

        activity.DifficultyLevel.Should().Be(3);   // 2 → 3
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _repo.Received(1).AddAdjustmentLogAsync(
            Arg.Is<AdaptiveAdjustmentLog>(l => l.AdjustmentType == "DifficultyUp"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AlreadyAtMaxDifficulty_NoUpgrade()
    {
        var config   = DefaultConfig();     // MaxDifficultyLevel = 5
        var activity = MakeActivity(config: config, difficulty: 5);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Exito, pct: 90m),
                 MakeResponse(ActivityResponseResult.Exito, pct: 90m),
                 MakeResponse(ActivityResponseResult.Exito, pct: 90m),
             });

        await BuildSut().HandleAsync(MakeJob(), default);

        activity.DifficultyLevel.Should().Be(5);   // stays at max
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── DifficultyDown ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_TwoConsecutiveFailures_DecreasesDifficulty()
    {
        var config   = DefaultConfig();     // ConsecutiveFailuresToDowngrade = 2
        var activity = MakeActivity(config: config, difficulty: 3);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Fallido),
                 MakeResponse(ActivityResponseResult.Fallido),
             });

        await BuildSut().HandleAsync(MakeJob(), default);

        activity.DifficultyLevel.Should().Be(2);   // 3 → 2
        await _repo.Received(1).AddAdjustmentLogAsync(
            Arg.Is<AdaptiveAdjustmentLog>(l => l.AdjustmentType == "DifficultyDown"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AlreadyAtMinDifficulty_NoDowngrade()
    {
        var config   = DefaultConfig();     // MinDifficultyLevel = 1
        var activity = MakeActivity(config: config, difficulty: 1);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Fallido),
                 MakeResponse(ActivityResponseResult.Fallido),
             });

        await BuildSut().HandleAsync(MakeJob(), default);

        activity.DifficultyLevel.Should().Be(1);   // stays at min
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── FrustrationIntervention ───────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_FrustrationAtThreshold_TriggersIntervention()
    {
        var config   = DefaultConfig();     // FrustrationThreshold = 4
        var activity = MakeActivity(config: config, difficulty: 3);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Exito, pct: 75m, frustration: 4),
             });

        await BuildSut().HandleAsync(MakeJob(profId: "prof-abc"), default);

        activity.DifficultyLevel.Should().Be(2);   // 3 → 2
        await _repo.Received(1).AddAdjustmentLogAsync(
            Arg.Is<AdaptiveAdjustmentLog>(l => l.AdjustmentType == "FrustrationIntervention"),
            Arg.Any<CancellationToken>());
        // Should enqueue a push notification for the professional
        await _jobs.Received(1).CreateAsync(
            JobTypes.Push,
            Arg.Any<string>(),
            Arg.Any<DateTime?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FrustrationBelowThreshold_NotTriggered()
    {
        var config   = DefaultConfig();     // FrustrationThreshold = 4
        var activity = MakeActivity(config: config, difficulty: 3);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Exito, pct: 75m, frustration: 3), // < 4
             });

        await BuildSut().HandleAsync(MakeJob(), default);

        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FrustrationNoProf_DoesNotEnqueueNotification()
    {
        var config   = DefaultConfig();
        var activity = MakeActivity(config: config, difficulty: 3);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Exito, pct: 75m, frustration: 4),
             });

        await BuildSut().HandleAsync(MakeJob(profId: ""), default);

        // Adjustment still applied
        activity.DifficultyLevel.Should().Be(2);
        // But no push notification
        await _jobs.DidNotReceive().CreateAsync(
            JobTypes.Push, Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    // ── Clamping ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DifficultyUp_ClampsToMax()
    {
        var config   = DefaultConfig();     // MaxDifficultyLevel = 5
        var activity = MakeActivity(config: config, difficulty: 4);
        _repo.GetWithConfigAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _repo.GetRecentResponsesByAssignmentAsync(5, Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<ActivityResponse>
             {
                 MakeResponse(ActivityResponseResult.Exito, pct: 95m),
                 MakeResponse(ActivityResponseResult.Exito, pct: 95m),
                 MakeResponse(ActivityResponseResult.Exito, pct: 95m),
             });

        await BuildSut().HandleAsync(MakeJob(), default);

        activity.DifficultyLevel.Should().Be(5);   // 4 → 5 (clamped at max)
    }
}
