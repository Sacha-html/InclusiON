using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Activities
{
    public class CancelActivityAssignmentCommandHandlerTests
    {
        private readonly IActivityAssignmentRepository _repo     = Substitute.For<IActivityAssignmentRepository>();
        private readonly IUnitOfWork                   _uow      = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider             _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService            _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid ProfId = Guid.NewGuid();

        public CancelActivityAssignmentCommandHandlerTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
        }

        private CancelActivityAssignmentCommandHandler BuildSut() =>
            new(_repo, _uow, _dateTime, _encryption);

        private static ActivityAssignment AnAssignment(int id = 1) => new()
        {
            Id                        = id,
            ActivityId                = 10,
            PersonId                  = Guid.NewGuid(),
            AssignedByProfessionalId  = ProfId,
            StatusId                  = AssignmentStatuses.Pendiente,
            AssignedAt                = DateTime.UtcNow.AddDays(-1),
        };

        [Fact]
        public async Task AssignmentNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((ActivityAssignment?)null);

            var result = await BuildSut().HandleAsync(new CancelActivityAssignmentCommand(1, ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task WrongProfessional_ReturnsForbidden()
        {
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnAssignment());

            var result = await BuildSut().HandleAsync(
                new CancelActivityAssignmentCommand(1, Guid.NewGuid()), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task NotInPendienteStatus_ReturnsBusinessRuleViolation()
        {
            var assignment = AnAssignment();
            assignment.StatusId = AssignmentStatuses.EnProgreso;
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(assignment);

            var result = await BuildSut().HandleAsync(new CancelActivityAssignmentCommand(1, ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValidCancel_SetsCanceladaAndSaves()
        {
            var assignment = AnAssignment(1);
            var now        = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(assignment);
            // Second call returns same assignment for the reload after save
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(assignment);

            var result = await BuildSut().HandleAsync(new CancelActivityAssignmentCommand(1, ProfId), default);

            result.Success.Should().BeTrue();
            assignment.StatusId.Should().Be(AssignmentStatuses.Cancelada);
            assignment.UpdatedAt.Should().Be(now);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _repo.Received(1).UpdateAsync(assignment, Arg.Any<CancellationToken>());
        }
    }
}
