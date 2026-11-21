using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class UpdateFamilyCommandHandlerTests
    {
        private readonly IFamilyRepository            _familyRepo = Substitute.For<IFamilyRepository>();
        private readonly IIdentityService             _identity   = Substitute.For<IIdentityService>();
        private readonly IUniquenessValidationService _uniqueness = Substitute.For<IUniquenessValidationService>();
        private readonly IUnitOfWork                  _uow        = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider            _dateTime   = Substitute.For<IDateTimeProvider>();

        public UpdateFamilyCommandHandlerTests()
        {
            _uniqueness.CheckDocumentAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Available());
            _uniqueness.CheckEmailAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(EmailCheckResult.Available());
        }

        private UpdateFamilyCommandHandler BuildSut() =>
            new(_familyRepo, _identity, _uniqueness, _uow,
                NullLogger<UpdateFamilyCommandHandler>.Instance, _dateTime);

        private static readonly Guid FamilyId = Guid.NewGuid();

        private static UpdateFamilyCommand Cmd(string email = "nueva@test.com", string? doc = null) =>
            new(FamilyId, "Pedro", "Ruiz", email, doc, "1122334455", "Padre");

        private static FamilyRepresentative AFamily() => new()
        {
            UserId    = Guid.NewGuid(),
            FirstName = "María",
            LastName  = "López",
            IsActive  = true,
            User      = new User { Id = Guid.NewGuid(), Email = "viejo@test.com", IsActive = true },
        };

        // ── Familiar no encontrado ───────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_FamilyNotFound_ReturnsNotFound()
        {
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>())
                       .Returns((FamilyRepresentative?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Documento duplicado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DuplicateDocument_ReturnsDocumentAlreadyExists()
        {
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(AFamily());
            _uniqueness.CheckDocumentAsync("99999999", FamilyId, Arg.Any<CancellationToken>())
                       .Returns(DocumentCheckResult.Taken("Familiar", false, "El DNI 99999999 ya se encuentra registrado."));

            var result = await BuildSut().HandleAsync(Cmd(doc: "99999999"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        // ── Email ya en uso por otro usuario ─────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmailTakenByOther_ReturnsEmailAlreadyExists()
        {
            var family = AFamily();
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);
            _uniqueness.CheckEmailAsync("nueva@test.com", family.UserId, family.Id, false, Arg.Any<CancellationToken>())
                       .Returns(EmailCheckResult.Taken("Este email ya se encuentra registrado en el sistema."));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_UpdatesFieldsAndSaves()
        {
            var family = AFamily();
            var now    = DateTime.UtcNow;
            _familyRepo.GetByIdAsync(FamilyId, Arg.Any<CancellationToken>()).Returns(family);
            _identity.FindByEmailAsync("nueva@test.com").Returns((User?)null);
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            family.FirstName.Should().Be("Pedro");
            family.LastName.Should().Be("Ruiz");
            family.UpdatedAt.Should().Be(now);
            await _familyRepo.Received(1).UpdateAsync(family, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
