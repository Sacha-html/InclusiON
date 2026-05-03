using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Handlers;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Reports
{
    public class GetReportByIdQueryHandlerTests
    {
        private readonly IReportsRepository _reportsRepo = Substitute.For<IReportsRepository>();

        private GetReportByIdQueryHandler BuildSut() => new(_reportsRepo);

        private static Report AReport() => new()
        {
            Id             = 5,
            ProfessionalId = Guid.NewGuid(),
            PersonId       = Guid.NewGuid(),
            Status         = ReportStatus.Draft,
            IsActive       = true,
            ReportDate     = DateTime.UtcNow,
            CreatedAt      = DateTime.UtcNow,
        };

        [Fact]
        public async Task HandleAsync_ReportNotFound_ReturnsReportNotFound()
        {
            _reportsRepo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns((Report?)null);

            var result = await BuildSut().HandleAsync(new GetReportByIdQuery(5), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ReportNotFound);
        }

        [Fact]
        public async Task HandleAsync_ReportExists_ReturnsMappedResponse()
        {
            _reportsRepo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(AReport());

            var result = await BuildSut().HandleAsync(new GetReportByIdQuery(5), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(5);
        }
    }
}
