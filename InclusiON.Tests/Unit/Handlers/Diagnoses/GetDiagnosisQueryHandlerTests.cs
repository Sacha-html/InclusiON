using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Handlers;
using InclusiON.Application.UseCases.Diagnoses.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Diagnoses
{
    public class GetDiagnosisQueryHandlerTests
    {
        private readonly IDiagnosesRepository _repo = Substitute.For<IDiagnosesRepository>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private const int DiagnosisId = 7;
        private static readonly Guid PersonId = Guid.NewGuid();

        private static Diagnosis ADiagnosis() => new()
        {
            Id = DiagnosisId, PersonId = PersonId,
            DiagnosisDate = DateTime.UtcNow,
            PrimaryDiagnosis = "TEA",
            Professional = new Professional { FirstName = "Dr", LastName = "House" },
        };

        // ── GetDiagnosisById ─────────────────────────────────────────────────

        [Fact]
        public async Task GetDiagnosisById_NotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(DiagnosisId, Arg.Any<CancellationToken>())
                 .Returns((Diagnosis?)null);

            var handler = new GetDiagnosisByIdQueryHandler(_repo, _encryption);
            var result = await handler.HandleAsync(new GetDiagnosisByIdQuery(DiagnosisId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task GetDiagnosisById_Found_ReturnsDiagnosis()
        {
            _repo.GetByIdAsync(DiagnosisId, Arg.Any<CancellationToken>())
                 .Returns(ADiagnosis());

            var handler = new GetDiagnosisByIdQueryHandler(_repo, _encryption);
            var result = await handler.HandleAsync(new GetDiagnosisByIdQuery(DiagnosisId), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(DiagnosisId);
        }
    }
}
