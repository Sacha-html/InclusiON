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
            var families = new List<(FamilyRepresentative Family, bool WasPreviouslyLinked)>
            {
                (AFamily("María", "García"), false),
                (AFamily("Marta", "López"), true)
            };

            _repo.GetAvailableFamiliesAsync("Mar", personId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns((families, families.Count));

            var query = new GetAvailableFamiliesQuery("Mar", personId);
            var result = await BuildSut().HandleAsync(query, default);

            result.Success.Should().BeTrue();
            result.Data!.Data.Should().HaveCount(2);
            result.Data!.Data[0].FirstName.Should().Be("María");
            result.Data!.Data[0].WasPreviouslyLinked.Should().BeFalse();
            result.Data!.Data[1].WasPreviouslyLinked.Should().BeTrue();
        }

        [Fact]
        public async Task EmptyResult_ReturnsEmptyList()
        {
            var empty = new List<(FamilyRepresentative, bool)>();
            _repo.GetAvailableFamiliesAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns((empty, 0));

            var result = await BuildSut().HandleAsync(new GetAvailableFamiliesQuery(null), default);

            result.Success.Should().BeTrue();
            result.Data!.Data.Should().BeEmpty();
            result.Data!.TotalRecords.Should().Be(0);
        }
    }
}
