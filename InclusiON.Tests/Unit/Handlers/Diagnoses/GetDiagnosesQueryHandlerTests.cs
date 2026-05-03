using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Handlers;
using InclusiON.Application.UseCases.Diagnoses.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Diagnoses
{
    public class GetDiagnosesQueryHandlerTests
    {
        private readonly IDiagnosesRepository _repo = Substitute.For<IDiagnosesRepository>();

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly Guid ProfUserId = Guid.NewGuid();

        private static Diagnosis ADiagnosis(int id, string primary) => new()
        {
            Id = id,
            PersonId = PersonId,
            ProfessionalId = ProfId,
            DiagnosisDate = new DateTime(2024, 1, 1),
            PrimaryDiagnosis = primary,
            Professional = new Professional { FirstName = "Dr", LastName = "House", UserId = ProfUserId }
        };

        [Fact]
        public async Task GetDiagnoses_ReturnsMappedList()
        {
            _repo.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<Diagnosis>
                {
                    ADiagnosis(1, "TEA"),
                    ADiagnosis(2, "TDAH")
                });

            var handler = new GetDiagnosesQueryHandler(_repo);
            var result = await handler.HandleAsync(new GetDiagnosesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data![0].PrimaryDiagnosis.Should().Be("TEA");
            result.Data[1].PrimaryDiagnosis.Should().Be("TDAH");
            result.Data[0].ProfessionalName.Should().Be("Dr House");
        }

        [Fact]
        public async Task GetDiagnoses_EmptyList_ReturnsSuccess()
        {
            _repo.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<Diagnosis>());

            var result = await new GetDiagnosesQueryHandler(_repo)
                .HandleAsync(new GetDiagnosesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
