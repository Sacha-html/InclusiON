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

    private static PersonWithDisability APerson(int? loginMethodId = 3) => new()
    {
        Id            = PersonId,
        UserId        = Guid.NewGuid(),
        FirstName     = "Ana",
        LastName      = "García",
        LoginMethodId = loginMethodId
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

    // ── Sin destinatarios ────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_NoRecipients_ReturnsSuccessWithoutNotifying()
    {
        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson(loginMethodId: null));
        _personsRepo.GetActiveRepresentativesAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<PersonRepresentative>());

        var result = await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        result.Success.Should().BeTrue();
        await _notifier.DidNotReceiveWithAnyArgs()
            .NotifyUserAsync(default!, default!, default!, default, default);
    }

    // ── Notifica a Tutor (Siempre) ───────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_HasRepresentatives_NotifiesTutors()
    {
        var person = APerson(loginMethodId: null); // standard login, not assisted
        var tutorUserId = Guid.NewGuid();
        var representative = new PersonRepresentative
        {
            PersonId = PersonId,
            Representative = new FamilyRepresentative { UserId = tutorUserId }
        };

        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);
        _personsRepo.GetActiveRepresentativesAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<PersonRepresentative> { representative });

        var result = await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        result.Success.Should().BeTrue();
        await _notifier.Received(1).NotifyUserAsync(
            tutorUserId.ToString(),
            Arg.Is<string>(t => t.Contains("ayuda")),
            Arg.Is<string>(m => m.Contains("Ana García")),
            Arg.Is<string>("/#/family/dashboard"),
            Arg.Any<CancellationToken>());
    }

    // ── Ingreso Asistido: Notifica a Tutor y Profesional ──────────────────────

    [Fact]
    public async Task HandleAsync_AssistedLogin_NotifiesTutorAndProfessional()
    {
        var person = APerson(loginMethodId: 3); // assisted login
        var tutorUserId = Guid.NewGuid();
        var representative = new PersonRepresentative
        {
            PersonId = PersonId,
            Representative = new FamilyRepresentative { UserId = tutorUserId }
        };
        var profUserId = Guid.NewGuid();
        var prof = AProfessional(profUserId);

        _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);
        _personsRepo.GetActiveRepresentativesAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<PersonRepresentative> { representative });
        _personsRepo.GetSupervisingProfessionalsAsync(PersonId, Arg.Any<CancellationToken>())
                    .Returns(new List<Professional> { prof });

        var result = await BuildSut().HandleAsync(new RequestHelpCommand(PersonId), default);

        result.Success.Should().BeTrue();
        // Verificamos que se notifica al tutor
        await _notifier.Received(1).NotifyUserAsync(
            tutorUserId.ToString(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>("/#/family/dashboard"),
            Arg.Any<CancellationToken>());

        // Verificamos que se notifica al profesional supervisor
        await _notifier.Received(1).NotifyUserAsync(
            profUserId.ToString(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(url => url != null && url.Contains(PersonId.ToString())),
            Arg.Any<CancellationToken>());
    }
}
