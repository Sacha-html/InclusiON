using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons;

public class RequestHelpCommandHandlerTests
{
    private readonly IPersonsRepository _personsRepo = Substitute.For<IPersonsRepository>();
    private readonly IRealTimeNotifier  _notifier    = Substitute.For<IRealTimeNotifier>();

    private RequestHelpCommandHandler BuildSut() =>
        new(_personsRepo, _notifier, NullLogger<RequestHelpCommandHandler>.Instance);

    private static readonly Guid PersonId = Guid.NewGuid();

    private static PersonWithDisability APerson() => new()
    {
        Id        = PersonId,
        UserId    = Guid.NewGuid(),
        FirstName = "Ana",
        LastName  = "García",
    };

    private static Professional AProfessional(Guid? userId = null) => new()
    {
        Id     = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
    };

    // ── Persona no encontrada ────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_PersonNotFound_ReturnsNotFound()
    {
        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns((PersonWithDisability?)null);

        var result = await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.NotFound);
        await _notifier.DidNotReceiveWithAnyArgs()
            .NotifyUserAsync(default!, default!, default!, default, default);
    }

    // ── Sin supervisores ─────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NoSupervisors_ReturnsSuccessWithoutNotifying()
    {
        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
        _personsRepo.GetSupervisingProfessionalsAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<Professional>());

        var result = await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        result.Success.Should().BeTrue();
        await _notifier.DidNotReceiveWithAnyArgs()
            .NotifyUserAsync(default!, default!, default!, default, default);
    }

    // ── Un supervisor ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_OneSupervisor_NotifiesOnce()
    {
        var profUserId = Guid.NewGuid();
        var prof       = AProfessional(profUserId);

        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
        _personsRepo.GetSupervisingProfessionalsAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<Professional> { prof });

        var result = await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        result.Success.Should().BeTrue();
        await _notifier.Received(1).NotifyUserAsync(
            profUserId.ToString(),
            Arg.Is<string>(t => t.Contains("ayuda")),
            Arg.Is<string>(m => m.Contains("Ana García")),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    // ── Múltiples supervisores ───────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_MultipleSupervisors_NotifiesAll()
    {
        var prof1 = AProfessional();
        var prof2 = AProfessional();
        var prof3 = AProfessional();

        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
        _personsRepo.GetSupervisingProfessionalsAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<Professional> { prof1, prof2, prof3 });

        var result = await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        result.Success.Should().BeTrue();
        await _notifier.Received(3).NotifyUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    // ── ActionUrl contiene PersonId ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_ActionUrl_ContainsPersonId()
    {
        var profUserId = Guid.NewGuid();
        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
        _personsRepo.GetSupervisingProfessionalsAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<Professional> { AProfessional(profUserId) });

        await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        await _notifier.Received(1).NotifyUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is<string?>(url => url != null && url.Contains(PersonId.ToString())),
            Arg.Any<CancellationToken>());
    }
}
