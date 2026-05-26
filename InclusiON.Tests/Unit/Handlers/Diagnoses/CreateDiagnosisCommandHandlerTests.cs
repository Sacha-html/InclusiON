using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Diagnoses.Commands;
using InclusiON.Application.UseCases.Diagnoses.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Diagnoses
{
    public class CreateDiagnosisCommandHandlerTests
    {
        private readonly IDiagnosesRepository _repo = Substitute.For<IDiagnosesRepository>();
        private readonly IProfessionalsRepository _proRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IPersonsRepository _personRepo = Substitute.For<IPersonsRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        private CreateDiagnosisCommandHandler BuildSut() =>
            new(_repo, _proRepo, _personRepo, _uow,
                NullLogger<CreateDiagnosisCommandHandler>.Instance, _dateTime, _encryption);

        private static CreateDiagnosisCommand Cmd() => new(
            PersonId, ProfId,
            DiagnosisDate: new DateTime(2025, 1, 1),
            PrimaryDiagnosis: "TEA",
            InitialObservations: null, IdentifiedCapabilities: null,
            IdentifiedChallenges: null, RequiredSupports: null,
            PedagogicalObjectives: null, RecommendedStrategies: null);

        private static Professional ApprovedPro() => new()
        {
            Id = ProfId, FirstName = "Dr", LastName = "House",
            Status = ProfessionalStatusEnum.Approved
        };

        private static PersonWithDisability APerson() => new()
        {
            Id = PersonId, BirthDate = new DateTime(2000, 1, 1)
        };

        private static Diagnosis ACreatedDiagnosis() => new()
        {
            Id = 1, PersonId = PersonId, ProfessionalId = ProfId,
            PrimaryDiagnosis = "TEA",
            DiagnosisDate = new DateTime(2025, 1, 1),
            Professional = new Professional { FirstName = "Dr", LastName = "House" }
        };

        [Fact]
        public async Task ProfessionalNotFound_ReturnsProfessionalNotFound()
        {
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        [Fact]
        public async Task ProfessionalNotApproved_ReturnsProfessionalNotApproved()
        {
            var pro = ApprovedPro();
            pro.Status = ProfessionalStatusEnum.Pending;
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(pro);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotApproved);
        }

        [Fact]
        public async Task PersonNotFound_ReturnsPersonNotFound()
        {
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _personRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        [Fact]
        public async Task ValidCommand_CreatesDiagnosisAndReturnsResponse()
        {
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _personRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
            _dateTime.UtcNow.Returns(Now);

            var created = ACreatedDiagnosis();
            _repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(created);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.PrimaryDiagnosis.Should().Be("TEA");
            result.Data.ProfessionalName.Should().Be("Dr House");
            await _repo.Received(1).CreateAsync(Arg.Any<Diagnosis>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
