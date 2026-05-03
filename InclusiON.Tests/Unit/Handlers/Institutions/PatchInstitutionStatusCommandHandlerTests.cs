using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Commands;
using InclusiON.Application.UseCases.Institutions.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Institutions
{
    public class PatchInstitutionStatusCommandHandlerTests
    {
        private readonly IInstitutionsRepository _repository  = Substitute.For<IInstitutionsRepository>();
        private readonly IUnitOfWork             _unitOfWork  = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider       _dateTime    = Substitute.For<IDateTimeProvider>();

        private PatchInstitutionStatusCommandHandler BuildSut() =>
            new(_repository, _unitOfWork, _dateTime);

        private static readonly DateTime FixedNow =
            new(2026, 4, 28, 12, 0, 0, DateTimeKind.Utc);

        private static EducationalInstitution ActiveInstitution() =>
            new() { Id = 1, Name = "Escuela N° 1", IsActive = true };

        private static EducationalInstitution InactiveInstitution() =>
            new() { Id = 1, Name = "Escuela N° 1", IsActive = false };

        // ── Institución no encontrada ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_InstitutionNotFound_ReturnsNotFound()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                       .Returns((EducationalInstitution?)null);

            // Act
            var result = await BuildSut().HandleAsync(
                new PatchInstitutionStatusCommand(1, false), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── Transiciones no-op (máquina de estados) ─────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyInactive_DeactivateRequested_ReturnsConflict()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                       .Returns(InactiveInstitution());

            // Act
            var result = await BuildSut().HandleAsync(
                new PatchInstitutionStatusCommand(1, false), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_AlreadyActive_ActivateRequested_ReturnsConflict()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                       .Returns(ActiveInstitution());

            // Act
            var result = await BuildSut().HandleAsync(
                new PatchInstitutionStatusCommand(1, true), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── Validación de integridad en baja ────────────────────────────────

        [Fact]
        public async Task HandleAsync_ActiveWithProfessionals_Deactivate_ReturnsConflict()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                       .Returns(ActiveInstitution());
            _repository.HasActiveProfessionalsAsync(1, Arg.Any<CancellationToken>())
                       .Returns(true);

            // Act
            var result = await BuildSut().HandleAsync(
                new PatchInstitutionStatusCommand(1, false), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_Activate_DoesNotCheckProfessionals()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                       .Returns(InactiveInstitution());
            _dateTime.UtcNow.Returns(FixedNow);

            // Act
            await BuildSut().HandleAsync(new PatchInstitutionStatusCommand(1, true), default);

            // Assert
            await _repository.DidNotReceive()
                .HasActiveProfessionalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        // ── Baja exitosa ────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ActiveNoProfessionals_Deactivate_ReturnsSuccess()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                       .Returns(ActiveInstitution());
            _repository.HasActiveProfessionalsAsync(1, Arg.Any<CancellationToken>())
                       .Returns(false);
            _dateTime.UtcNow.Returns(FixedNow);

            // Act
            var result = await BuildSut().HandleAsync(
                new PatchInstitutionStatusCommand(1, false), default);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_Deactivate_SetsIsActiveAndUpdatedAt()
        {
            // Arrange
            var institution = ActiveInstitution();
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(institution);
            _repository.HasActiveProfessionalsAsync(1, Arg.Any<CancellationToken>()).Returns(false);
            _dateTime.UtcNow.Returns(FixedNow);

            // Act
            await BuildSut().HandleAsync(new PatchInstitutionStatusCommand(1, false), default);

            // Assert
            institution.IsActive.Should().BeFalse();
            institution.UpdatedAt.Should().Be(FixedNow);
        }

        [Fact]
        public async Task HandleAsync_Deactivate_CallsUpdateAndSaveChanges()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ActiveInstitution());
            _repository.HasActiveProfessionalsAsync(1, Arg.Any<CancellationToken>()).Returns(false);
            _dateTime.UtcNow.Returns(FixedNow);

            // Act
            await BuildSut().HandleAsync(new PatchInstitutionStatusCommand(1, false), default);

            // Assert
            await _repository.Received(1).UpdateAsync(
                Arg.Is<EducationalInstitution>(i => !i.IsActive),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── Reactivación exitosa ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Inactive_Activate_ReturnsSuccess()
        {
            // Arrange
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                       .Returns(InactiveInstitution());
            _dateTime.UtcNow.Returns(FixedNow);

            // Act
            var result = await BuildSut().HandleAsync(
                new PatchInstitutionStatusCommand(1, true), default);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_Activate_SetsIsActiveAndUpdatedAt()
        {
            // Arrange
            var institution = InactiveInstitution();
            _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(institution);
            _dateTime.UtcNow.Returns(FixedNow);

            // Act
            await BuildSut().HandleAsync(new PatchInstitutionStatusCommand(1, true), default);

            // Assert
            institution.IsActive.Should().BeTrue();
            institution.UpdatedAt.Should().Be(FixedNow);
        }
    }
}
