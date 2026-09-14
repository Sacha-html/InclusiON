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
    public class GetDiagnosesQueryHandlerTests
    {
        private readonly IDiagnosesRepository _repo = Substitute.For<IDiagnosesRepository>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

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
            IsActive = true,
            Professional = new Professional { FirstName = "Dr", LastName = "House", UserId = ProfUserId }
        };

        [Fact]
        public async Task GetDiagnoses_ReturnsMappedList()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns(callInfo => $"ENC:{callInfo.Arg<string>()}");
            var list = new List<Diagnosis>
            {
                ADiagnosis(1, "TEA"),
                ADiagnosis(2, "TDAH")
            };
            _repo.GetPagedByPersonIdAsync(PersonId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
                .Returns(new PagedResponse<Diagnosis> { Data = list, TotalRecords = list.Count, TotalPages = 1, CurrentPage = 1, PageSize = 100 });

            var handler = new GetDiagnosesQueryHandler(_repo, _encryption);
            var result = await handler.HandleAsync(new GetDiagnosesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data!.Data.Should().HaveCount(2);
            result.Data!.Data[0].PrimaryDiagnosis.Should().Be("TEA");
            result.Data!.Data[1].PrimaryDiagnosis.Should().Be("TDAH");
            result.Data!.Data[0].ProfessionalName.Should().Be("Dr House");
            result.Data!.Data.Should().OnlyContain(d => !string.IsNullOrWhiteSpace(d.EncryptedId));
        }

        [Fact]
        public async Task GetDiagnoses_EmptyList_ReturnsSuccess()
        {
            _repo.GetPagedByPersonIdAsync(PersonId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
                .Returns(new PagedResponse<Diagnosis>());

            var result = await new GetDiagnosesQueryHandler(_repo, _encryption)
                .HandleAsync(new GetDiagnosesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data!.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDiagnoses_ForwardsStatusFilterAndMapsInactiveItems()
        {
            var inactive = ADiagnosis(3, "TDAH");
            inactive.IsActive = false;
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:id");
            _repo.GetPagedByPersonIdAsync(PersonId, 1, 10, false, Arg.Any<CancellationToken>())
                .Returns(new PagedResponse<Diagnosis> { Data = [inactive] });

            var result = await new GetDiagnosesQueryHandler(_repo, _encryption)
                .HandleAsync(new GetDiagnosesQuery(PersonId, 1, 10, false), default);

            result.Data!.Data.Single().IsActive.Should().BeFalse();
            await _repo.Received(1).GetPagedByPersonIdAsync(PersonId, 1, 10, false, Arg.Any<CancellationToken>());
        }
    }
}
