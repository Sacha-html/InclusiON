using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    public class UpdateProfessionalCommandHandlerTests
    {
        private readonly IProfessionalsRepository _prosRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IUnitOfWork              _uow      = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime = Substitute.For<IDateTimeProvider>();

        private UpdateProfessionalCommandHandler BuildSut() =>
            new(_prosRepo, _uow,
                NullLogger<UpdateProfessionalCommandHandler>.Instance, _dateTime);

        private static readonly Guid ProfId = Guid.NewGuid();

        private static UpdateProfessionalCommand Cmd(string? doc = null) =>
            new(ProfessionalId: ProfId, FirstName: "Pedro", LastName: "Ruiz",
                DocumentNumber: doc, Phone: "1122334455");

        private static Professional AProfessional() => new()
        {
            Id             = ProfId,
            UserId         = Guid.NewGuid(),
            FirstName      = "Juan",
            LastName       = "García",
            DocumentNumber = "12345678",
            Status         = ProfessionalStatusEnum.Approved,
            User           = new User { IsActive = true },
            ProfessionalInstitutions = [],
        };

        // ── Profesional no encontrado ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsProfessionalNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        // ── Documento ya existe ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DuplicateDocument_ReturnsDocumentAlreadyExists()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(AProfessional());
            _prosRepo.ExistsDocumentAsync("99999999", ProfId, Arg.Any<CancellationToken>())
                     .Returns(true);

            var result = await BuildSut().HandleAsync(Cmd(doc: "99999999"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_UpdatesFieldsAndSaves()
        {
            var professional = AProfessional();
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(professional);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            professional.FirstName.Should().Be("Pedro");
            professional.LastName.Should().Be("Ruiz");
            professional.Phone.Should().Be("1122334455");
            await _prosRepo.Received(1).UpdateAsync(professional, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
