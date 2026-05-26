using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Application.UseCases.Roadmap.Handlers;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Tests.Unit.Handlers.Roadmap;

// ════════════════════════════════════════════════════════════════════════════
// GetAdaptiveEngineConfigQueryHandler
// ════════════════════════════════════════════════════════════════════════════

public class GetAdaptiveEngineConfigQueryHandlerTests
{
    private readonly IAdaptiveEngineRepository _repo = Substitute.For<IAdaptiveEngineRepository>();
    private GetAdaptiveEngineConfigQueryHandler Sut() => new(_repo);

    [Fact]
    public async Task HandleAsync_NoConfig_ReturnsSuccessWithNullData()
    {
        _repo.GetConfigAsync(1, Arg.Any<CancellationToken>()).Returns((AdaptiveEngineConfig?)null);

        var result = await Sut().HandleAsync(new GetAdaptiveEngineConfigQuery(1), default);

        result.Success.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ConfigExists_ReturnsMappedDto()
    {
        var config = new AdaptiveEngineConfig
        {
            Id                             = 7,
            PersonRoadmapActivityId        = 1,
            IsEnabled                      = true,
            MinDifficultyLevel             = 1,
            MaxDifficultyLevel             = 5,
            ConsecutiveSuccessToUpgrade    = 3,
            ConsecutiveFailuresToDowngrade = 2,
            SuccessThresholdPercent        = 70,
            FrustrationThreshold           = 3,
        };
        _repo.GetConfigAsync(1, Arg.Any<CancellationToken>()).Returns(config);

        var result = await Sut().HandleAsync(new GetAdaptiveEngineConfigQuery(1), default);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(7);
        result.Data.IsEnabled.Should().BeTrue();
        result.Data.SuccessThresholdPercent.Should().Be(70);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// UpsertAdaptiveEngineConfigCommandHandler
// ════════════════════════════════════════════════════════════════════════════

public class UpsertAdaptiveEngineConfigCommandHandlerTests
{
    private readonly IAdaptiveEngineRepository _adaptive = Substitute.For<IAdaptiveEngineRepository>();
    private readonly IRoadmapRepository        _roadmap  = Substitute.For<IRoadmapRepository>();

    private UpsertAdaptiveEngineConfigCommandHandler Sut() => new(_adaptive, _roadmap);

    private static UpsertAdaptiveEngineConfigCommand DefaultCmd(int activityEntryId = 1) => new(
        PersonRoadmapActivityId:        activityEntryId,
        IsEnabled:                      true,
        MinDifficultyLevel:             1,
        MaxDifficultyLevel:             5,
        MinTimeLimitSeconds:            null,
        MaxTimeLimitSeconds:            null,
        ConsecutiveSuccessToUpgrade:    3,
        ConsecutiveFailuresToDowngrade: 2,
        SuccessThresholdPercent:        70,
        FrustrationThreshold:           3);

    [Fact]
    public async Task HandleAsync_ActivityNotFound_ReturnsNotFound()
    {
        _roadmap.GetActivityByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((PersonRoadmapActivity?)null);

        var result = await Sut().HandleAsync(DefaultCmd(), default);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CallsUpsertAndReturnsDto()
    {
        var activity = new PersonRoadmapActivity { Id = 1 };
        var saved = new AdaptiveEngineConfig
        {
            Id                             = 1,
            PersonRoadmapActivityId        = 1,
            IsEnabled                      = true,
            MinDifficultyLevel             = 1,
            MaxDifficultyLevel             = 5,
            ConsecutiveSuccessToUpgrade    = 3,
            ConsecutiveFailuresToDowngrade = 2,
            SuccessThresholdPercent        = 70,
            FrustrationThreshold           = 3,
        };

        _roadmap.GetActivityByIdAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _adaptive.UpsertConfigAsync(1, Arg.Any<AdaptiveEngineConfig>(), Arg.Any<CancellationToken>())
                 .Returns(saved);

        var result = await Sut().HandleAsync(DefaultCmd(), default);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsEnabled.Should().BeTrue();
        await _adaptive.Received(1).UpsertConfigAsync(1, Arg.Any<AdaptiveEngineConfig>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_MapsAllFieldsToIncomingConfig()
    {
        var activity = new PersonRoadmapActivity { Id = 1 };
        AdaptiveEngineConfig? captured = null;

        _roadmap.GetActivityByIdAsync(1, Arg.Any<CancellationToken>()).Returns(activity);
        _adaptive.UpsertConfigAsync(1, Arg.Do<AdaptiveEngineConfig>(c => captured = c), Arg.Any<CancellationToken>())
                 .Returns(x => x.ArgAt<AdaptiveEngineConfig>(1));

        var cmd = DefaultCmd() with
        {
            IsEnabled                      = false,
            MinDifficultyLevel             = 2,
            MaxDifficultyLevel             = 4,
            SuccessThresholdPercent        = 80,
            FrustrationThreshold           = 5,
            ConsecutiveSuccessToUpgrade    = 4,
            ConsecutiveFailuresToDowngrade = 3,
        };

        await Sut().HandleAsync(cmd, default);

        captured.Should().NotBeNull();
        captured!.IsEnabled.Should().BeFalse();
        captured.MinDifficultyLevel.Should().Be(2);
        captured.MaxDifficultyLevel.Should().Be(4);
        captured.SuccessThresholdPercent.Should().Be(80);
        captured.FrustrationThreshold.Should().Be(5);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// DeleteAdaptiveEngineConfigCommandHandler
// ════════════════════════════════════════════════════════════════════════════

public class DeleteAdaptiveEngineConfigCommandHandlerTests
{
    private readonly IAdaptiveEngineRepository _repo = Substitute.For<IAdaptiveEngineRepository>();
    private DeleteAdaptiveEngineConfigCommandHandler Sut() => new(_repo);

    [Fact]
    public async Task HandleAsync_CallsDeleteConfigAndReturnsSuccess()
    {
        var result = await Sut().HandleAsync(new DeleteAdaptiveEngineConfigCommand(42), default);

        result.Success.Should().BeTrue();
        await _repo.Received(1).DeleteConfigAsync(42, Arg.Any<CancellationToken>());
    }
}

// ════════════════════════════════════════════════════════════════════════════
// GetSkillRadarQueryHandler
// ════════════════════════════════════════════════════════════════════════════

public class GetSkillRadarQueryHandlerTests
{
    private readonly IAdaptiveEngineRepository _repo = Substitute.For<IAdaptiveEngineRepository>();
    private GetSkillRadarQueryHandler Sut() => new(_repo);

    private static readonly Guid PersonId = Guid.NewGuid();

    [Fact]
    public async Task HandleAsync_EmptyRoadmap_ReturnsSuccessEmptyList()
    {
        _repo.GetSkillRadarAsync(PersonId, Arg.Any<CancellationToken>())
             .Returns(new List<SkillRadarPointResponse>());

        var result = await Sut().HandleAsync(new GetSkillRadarQuery(PersonId), default);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithPoints_ReturnsMappedPoints()
    {
        var points = new List<SkillRadarPointResponse>
        {
            new() { AreaName = "Comunicación", AvgSuccessPercent = 82.5, TotalResponses = 4 },
            new() { AreaName = "Motricidad",   AvgSuccessPercent = null,  TotalResponses = 0 },
        };
        _repo.GetSkillRadarAsync(PersonId, Arg.Any<CancellationToken>()).Returns(points);

        var result = await Sut().HandleAsync(new GetSkillRadarQuery(PersonId), default);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].AreaName.Should().Be("Comunicación");
        result.Data[0].AvgSuccessPercent.Should().Be(82.5);
        result.Data[1].AvgSuccessPercent.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_PassesPersonIdToRepo()
    {
        _repo.GetSkillRadarAsync(PersonId, Arg.Any<CancellationToken>())
             .Returns(new List<SkillRadarPointResponse>());

        await Sut().HandleAsync(new GetSkillRadarQuery(PersonId), default);

        await _repo.Received(1).GetSkillRadarAsync(PersonId, Arg.Any<CancellationToken>());
    }
}
