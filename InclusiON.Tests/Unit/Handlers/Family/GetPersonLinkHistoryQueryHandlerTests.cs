using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class GetPersonLinkHistoryQueryHandlerTests
    {
        private readonly IFamilyRepository _repo = Substitute.For<IFamilyRepository>();

        private GetPersonLinkHistoryQueryHandler BuildSut() => new(_repo);

        private static readonly Guid PersonId = Guid.NewGuid();

        [Fact]
        public async Task ReturnsHistoryMappedToResponse()
        {
            var repId = Guid.NewGuid();
            _repo.GetPersonRepresentativeHistoryAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<PersonRepresentativeHistory>
                {
                    new()
                    {
                        PersonId = PersonId,
                        RepresentativeId = repId,
                        ChangeType = PersonRepresentativeChangeType.Linked,
                        Relationship = "Madre",
                        Representative = new FamilyRepresentative { FirstName = "Ana", LastName = "Ruiz" }
                    }
                });

            var result = await BuildSut().HandleAsync(new GetPersonLinkHistoryQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].PersonId.Should().Be(PersonId);
            result.Data[0].Action.Should().Be("Linked");
            result.Data[0].FamilyFullName.Should().Be("Ana Ruiz");
        }

        [Fact]
        public async Task NoHistory_ReturnsEmptyList()
        {
            _repo.GetPersonRepresentativeHistoryAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<PersonRepresentativeHistory>());

            var result = await BuildSut().HandleAsync(new GetPersonLinkHistoryQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
