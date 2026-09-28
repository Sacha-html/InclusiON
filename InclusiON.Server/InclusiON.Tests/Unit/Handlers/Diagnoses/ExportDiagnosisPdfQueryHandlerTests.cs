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
    public class ExportDiagnosisPdfQueryHandlerTests
    {
        private readonly IDiagnosesRepository _repo = Substitute.For<IDiagnosesRepository>();
        private readonly IDiagnosisPdfService _pdfService = Substitute.For<IDiagnosisPdfService>();

        private ExportDiagnosisPdfQueryHandler BuildSut() => new(_repo, _pdfService);

        [Fact]
        public async Task DiagnosisNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns((Diagnosis?)null);

            var result = await BuildSut().HandleAsync(new ExportDiagnosisPdfQuery(7), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task DiagnosisFound_ReturnsGeneratedPdfBytes()
        {
            var diagnosis = new Diagnosis { Id = 7 };
            var pdfBytes = new byte[] { 1, 2, 3 };
            _repo.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(diagnosis);
            _pdfService.Generate(diagnosis).Returns(pdfBytes);

            var result = await BuildSut().HandleAsync(new ExportDiagnosisPdfQuery(7), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(pdfBytes);
        }
    }
}
