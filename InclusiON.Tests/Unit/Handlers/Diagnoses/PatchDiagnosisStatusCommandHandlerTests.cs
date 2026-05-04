using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Commands;
using InclusiON.Application.UseCases.Diagnoses.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Diagnoses
{
    public class PatchDiagnosisStatusCommandHandlerTests
    {
        private readonly IDiagnosesRepository _repo = Substitute.For<IDiagnosesRepository>();
        private readonly IUnitOfWork         _uow  = Substitute.For<IUnitOfWork>();

        private PatchDiagnosisStatusCommandHandler BuildSut() =>
            new(_repo, _uow, NullLogger<PatchDiagnosisStatusCommandHandler>.Instance);

        private static readonly Guid CreatorId   = Guid.NewGuid();
        private static readonly Guid OtherProfId = Guid.NewGuid();

        private static Diagnosis ADiagnosis(bool isActive = true) =>
            new()
            {
                Id             = 1,
                ProfessionalId = CreatorId,
                IsActive       = isActive,
                CreatedAt      = DateTime.UtcNow,
                CreatedBy      = CreatorId,
            };

        private static PatchDiagnosisStatusCommand Deactivate(Guid? profId = null) =>
            new(DiagnosisId: 1, IsActive: false, RequestedByProfessionalId: profId ?? CreatorId);

        private static PatchDiagnosisStatusCommand Activate(Guid? profId = null) =>
            new(DiagnosisId: 1, IsActive: true, RequestedByProfessionalId: profId ?? CreatorId);

        // ── No encontrado ────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DiagnosisNotFound_ReturnsNotFound()
        {
            _repo.GetByIdIgnoreActiveAsync(1, Arg.Any<CancellationToken>()).Returns((Diagnosis?)null);

            var result = await BuildSut().HandleAsync(Deactivate(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── No-op checks ─────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyInactive_ReturnsBusinessRuleViolation()
        {
            _repo.GetByIdIgnoreActiveAsync(1, Arg.Any<CancellationToken>()).Returns(ADiagnosis(isActive: false));

            var result = await BuildSut().HandleAsync(Deactivate(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        [Fact]
        public async Task HandleAsync_AlreadyActive_ReturnsBusinessRuleViolation()
        {
            _repo.GetByIdIgnoreActiveAsync(1, Arg.Any<CancellationToken>()).Returns(ADiagnosis(isActive: true));

            var result = await BuildSut().HandleAsync(Activate(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        // ── Autorización ──────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DifferentProfessional_ReturnsNotAuthorized()
        {
            _repo.GetByIdIgnoreActiveAsync(1, Arg.Any<CancellationToken>()).Returns(ADiagnosis());

            var result = await BuildSut().HandleAsync(Deactivate(OtherProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotAuthorizedForResource);
        }

        [Fact]
        public async Task HandleAsync_AdminRequester_CanDeactivateWithoutCreatorCheck()
        {
            var diagnosis = ADiagnosis();
            _repo.GetByIdIgnoreActiveAsync(1, Arg.Any<CancellationToken>()).Returns(diagnosis);

            // Admin: RequestedByProfessionalId = null
            var cmd = new PatchDiagnosisStatusCommand(DiagnosisId: 1, IsActive: false, RequestedByProfessionalId: null);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeTrue();
            diagnosis.IsActive.Should().BeFalse();
        }

        // ── Happy paths ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_CreatorDeactivates_DeactivatesAndSaves()
        {
            var diagnosis = ADiagnosis(isActive: true);
            _repo.GetByIdIgnoreActiveAsync(1, Arg.Any<CancellationToken>()).Returns(diagnosis);

            var result = await BuildSut().HandleAsync(Deactivate(), default);

            result.Success.Should().BeTrue();
            diagnosis.IsActive.Should().BeFalse();
            await _repo.Received(1).UpdateAsync(diagnosis, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_CreatorReactivates_ActivatesAndSaves()
        {
            var diagnosis = ADiagnosis(isActive: false);
            _repo.GetByIdIgnoreActiveAsync(1, Arg.Any<CancellationToken>()).Returns(diagnosis);

            var result = await BuildSut().HandleAsync(Activate(), default);

            result.Success.Should().BeTrue();
            diagnosis.IsActive.Should().BeTrue();
            await _repo.Received(1).UpdateAsync(diagnosis, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
