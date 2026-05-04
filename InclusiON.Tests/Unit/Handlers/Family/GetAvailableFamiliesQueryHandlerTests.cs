using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class GetAvailableFamiliesQueryHandlerTests
    {
        private readonly IFamilyRepository _repo = Substitute.For<IFamilyRepository>();

        private GetAvailableFamiliesQueryHandler BuildSut() => new(_repo);

        private static FamilyRepresentative AFamily(string first, string last) => new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FirstName = first,
            LastName = last,
        };

        [Fact]
        public async Task WithSearch_ReturnsMappedFamilies()
        {
            var personId = Guid.NewGuid();
            _repo.GetAvailableFamiliesAsync("Mar", personId, Arg.Any<CancellationToken>())
                .Returns(new List<(FamilyRepresentative Family, bool WasPreviouslyLinked)>
                {
                    (AFamily("María", "García"), false),
                    (AFamily("Marta", "López"), true)
                });

            var query = new GetAvailableFamiliesQuery("Mar", personId);
            var result = await BuildSut().HandleAsync(query, default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data![0].FirstName.Should().Be("María");
            result.Data[0].WasPreviouslyLinked.Should().BeFalse();
            result.Data[1].WasPreviouslyLinked.Should().BeTrue();
        }

        [Fact]
        public async Task EmptyResult_ReturnsEmptyList()
        {
            _repo.GetAvailableFamiliesAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(new List<(FamilyRepresentative, bool)>());

            var result = await BuildSut().HandleAsync(new GetAvailableFamiliesQuery(null), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
