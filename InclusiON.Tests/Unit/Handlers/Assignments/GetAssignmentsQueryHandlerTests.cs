using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Handlers;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Assignments
{
    public class GetAssignmentsQueryHandlerTests
    {
        private readonly IAssignmentsRepository _repo = Substitute.For<IAssignmentsRepository>();

        private static readonly Guid ProfId = Guid.NewGuid();

        // ── GetPersonsByProfessional ─────────────────────────────────────────

        [Fact]
        public async Task GetPersonsByProfessional_ReturnsAllAssignments()
        {
            _repo.GetPersonsByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                 .Returns(new List<ProfessionalPerson>
                 {
                     new() { ProfessionalId = ProfId, IsActive = true },
                     new() { ProfessionalId = ProfId, IsActive = false },
                 });

            var handler = new GetPersonsByProfessionalQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetPersonsByProfessionalQuery(ProfId), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(2);
        }

        // ── GetInstitutionsByProfessional ────────────────────────────────────

        [Fact]
        public async Task GetInstitutionsByProfessional_ReturnsAllAssignments()
        {
            _repo.GetInstitutionsByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                 .Returns(new List<ProfessionalInstitution>
                 {
                     new() { ProfessionalId = ProfId, InstitutionId = 1, IsActive = true },
                 });

            var handler = new GetInstitutionsByProfessionalQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetInstitutionsByProfessionalQuery(ProfId), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(1);
        }
    }
}
