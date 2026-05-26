using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Assignments
{
    public class RemoveInstitutionAssignmentCommandHandlerTests
    {
        private readonly IAssignmentsRepository _assignRepo = Substitute.For<IAssignmentsRepository>();
        private readonly IUnitOfWork            _uow        = Substitute.For<IUnitOfWork>();

        private RemoveInstitutionAssignmentCommandHandler BuildSut() =>
            new(_assignRepo, _uow);

        private static readonly Guid ProfId = Guid.NewGuid();
        private const int InstId = 42;

        private static RemoveInstitutionAssignmentCommand Cmd() => new(ProfId, InstId);

        [Fact]
        public async Task HandleAsync_AssignmentNotFound_ReturnsNotFound()
        {
            _assignRepo.GetInstitutionAssignmentAsync(ProfId, InstId, Arg.Any<CancellationToken>())
                       .Returns((ProfessionalInstitution?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_AlreadyInactive_ReturnsInvalidOperation()
        {
            _assignRepo.GetInstitutionAssignmentAsync(ProfId, InstId, Arg.Any<CancellationToken>())
                       .Returns(new ProfessionalInstitution { IsActive = false });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        [Fact]
        public async Task HandleAsync_ActiveAssignment_DeactivatesAndSaves()
        {
            var assignment = new ProfessionalInstitution { IsActive = true, ProfessionalId = ProfId, InstitutionId = InstId };
            _assignRepo.GetInstitutionAssignmentAsync(ProfId, InstId, Arg.Any<CancellationToken>())
                       .Returns(assignment);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            assignment.IsActive.Should().BeFalse();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
