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
    public class UpdateDiagnosisCommandHandlerTests
    {
        private readonly IDiagnosesRepository _repo = Substitute.For<IDiagnosesRepository>();
        private readonly IProfessionalsRepository _proRepo = Substitute.For<IProfessionalsRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly Guid OtherProfId = Guid.NewGuid();
        private const int DiagnosisId = 7;

        private UpdateDiagnosisCommandHandler BuildSut() =>
            new(_repo, _proRepo, _uow,
                NullLogger<UpdateDiagnosisCommandHandler>.Instance, _dateTime, _encryption);

        private static UpdateDiagnosisCommand Cmd(Guid? profId = null) => new(
            DiagnosisId, profId ?? ProfId,
            DiagnosisDate: new DateTime(2025, 6, 1),
            PrimaryDiagnosis: "TEA actualizado",
            InitialObservations: null, IdentifiedCapabilities: null,
            IdentifiedChallenges: null, RequiredSupports: null,
            PedagogicalObjectives: null, RecommendedStrategies: null);

        private static Professional ApprovedPro(Guid? id = null) => new()
        {
            Id = id ?? ProfId, FirstName = "Dr", LastName = "House",
            Status = ProfessionalStatusEnum.Approved
        };

        private static Diagnosis ADiagnosis(Guid? owner = null) => new()
        {
            Id = DiagnosisId,
            PersonId = Guid.NewGuid(),
            ProfessionalId = owner ?? ProfId,
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
            pro.Status = ProfessionalStatusEnum.Rejected;
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(pro);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotApproved);
        }

        [Fact]
        public async Task DiagnosisNotFound_ReturnsNotFound()
        {
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _repo.GetByIdAsync(DiagnosisId, Arg.Any<CancellationToken>())
                .Returns((Diagnosis?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task DifferentProfessional_ReturnsNotAuthorized()
        {
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            // Diagnosis was created by OtherProfId
            _repo.GetByIdAsync(DiagnosisId, Arg.Any<CancellationToken>())
                .Returns(ADiagnosis(owner: OtherProfId));

            var result = await BuildSut().HandleAsync(Cmd(profId: ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotAuthorizedForResource);
        }

        [Fact]
        public async Task ValidUpdate_UpdatesFieldsAndSaves()
        {
            var now = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            _proRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            var diagnosis = ADiagnosis(owner: ProfId);
            _repo.GetByIdAsync(DiagnosisId, Arg.Any<CancellationToken>()).Returns(diagnosis);
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.PrimaryDiagnosis.Should().Be("TEA actualizado");
            diagnosis.PrimaryDiagnosis.Should().Be("TEA actualizado");
            diagnosis.UpdatedAt.Should().Be(now);
            diagnosis.UpdatedBy.Should().Be(ProfId);
            await _repo.Received(1).UpdateAsync(diagnosis, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
