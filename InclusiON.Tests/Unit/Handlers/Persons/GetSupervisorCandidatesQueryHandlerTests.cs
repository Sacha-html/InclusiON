using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    public class GetSupervisorCandidatesQueryHandlerTests
    {
        private readonly IPersonsRepository _repo = Substitute.For<IPersonsRepository>();

        private GetSupervisorCandidatesQueryHandler BuildSut() => new(_repo);

        private static readonly Guid PersonId = Guid.NewGuid();

        [Fact]
        public async Task CombinesProfessionalsAndFamilyAndSortsByFullName()
        {
            var proUserId = Guid.NewGuid();
            var familyUserId = Guid.NewGuid();

            _repo.GetSupervisingProfessionalsAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<Professional>
                {
                    new() { UserId = proUserId, FirstName = "Zara", LastName = "Gómez" }
                });

            _repo.GetActiveRepresentativesAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<PersonRepresentative>
                {
                    new()
                    {
                        Relationship = "Madre",
                        Representative = new FamilyRepresentative
                        {
                            UserId = familyUserId,
                            FirstName = "Ana",
                            LastName = "López"
                        }
                    }
                });

            var result = await BuildSut().HandleAsync(new GetSupervisorCandidatesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            // Sorted by FullName: "Ana López" < "Zara Gómez"
            result.Data![0].FullName.Should().Be("Ana López");
            result.Data[0].Type.Should().Be("Family");
            result.Data[0].Relationship.Should().Be("Madre");
            result.Data[1].FullName.Should().Be("Zara Gómez");
            result.Data[1].Type.Should().Be("Professional");
        }

        [Fact]
        public async Task NoCandidates_ReturnsEmptyList()
        {
            _repo.GetSupervisingProfessionalsAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<Professional>());
            _repo.GetActiveRepresentativesAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<PersonRepresentative>());

            var result = await BuildSut().HandleAsync(new GetSupervisorCandidatesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
