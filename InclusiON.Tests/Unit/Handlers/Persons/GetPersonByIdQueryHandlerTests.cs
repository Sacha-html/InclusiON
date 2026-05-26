using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    public class GetPersonByIdQueryHandlerTests
    {
        private readonly IPersonsRepository _repo = Substitute.For<IPersonsRepository>();

        private static readonly Guid PersonId = Guid.NewGuid();

        private GetPersonByIdQueryHandler BuildSut() => new(_repo);

        private static PersonWithDisability APerson() => new()
        {
            Id        = PersonId,
            UserId    = Guid.NewGuid(),
            FirstName = "Lucía",
            LastName  = "Martínez",
            BirthDate = new DateTime(2005, 3, 15),
        };

        [Fact]
        public async Task PersonNotFound_ReturnsPersonNotFound()
        {
            _repo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                 .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(new GetPersonByIdQuery(PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        [Fact]
        public async Task PersonFound_ReturnsMappedResponse()
        {
            _repo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                 .Returns(APerson());

            var result = await BuildSut().HandleAsync(new GetPersonByIdQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(PersonId);
            result.Data.FirstName.Should().Be("Lucía");
            result.Data.LastName.Should().Be("Martínez");
        }
    }
}
