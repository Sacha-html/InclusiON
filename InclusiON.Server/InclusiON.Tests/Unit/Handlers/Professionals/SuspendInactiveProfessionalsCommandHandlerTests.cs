using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    public class SuspendInactiveProfessionalsCommandHandlerTests
    {
        private readonly IProfessionalsRepository _repo = Substitute.For<IProfessionalsRepository>();
        private readonly IHttpContextService _http = Substitute.For<IHttpContextService>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        private static readonly Guid AdminId = Guid.NewGuid();

        private SuspendInactiveProfessionalsCommandHandler BuildSut() =>
            new(_repo, _http, _uow,
                NullLogger<SuspendInactiveProfessionalsCommandHandler>.Instance, _dateTime);

        private static SuspendInactiveProfessionalsCommand Cmd(int days = 90) => new(days);

        private static Professional InactivePro() => new()
        {
            Id = Guid.NewGuid(),
            FirstName = "Juan", LastName = "Pérez",
            Status = ProfessionalStatusEnum.Approved,
            ProfessionalInstitutions = new List<ProfessionalInstitution>
            {
                new() { ProfessionalId = Guid.NewGuid(), InstitutionId = 1, IsActive = true }
            }
        };

        [Fact]
        public async Task NoInactiveProfessionals_ReturnsSuspendedCountZeroAndSkipsSave()
        {
            _repo.GetInactiveProfessionalsAsync(90, Arg.Any<CancellationToken>())
                .Returns(new List<Professional>());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.SuspendedCount.Should().Be(0);
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await _repo.DidNotReceive().AddStatusHistoryAsync(Arg.Any<ProfessionalStatusHistory>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WithInactiveProfessionals_SuspendsSetsStatusAndSaves()
        {
            var pro = InactivePro();
            _repo.GetInactiveProfessionalsAsync(90, Arg.Any<CancellationToken>())
                .Returns(new List<Professional> { pro });
            _http.GetCurrentUserId().Returns(AdminId);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.SuspendedCount.Should().Be(1);
            pro.Status.Should().Be(ProfessionalStatusEnum.Suspended);
            pro.ProfessionalInstitutions.Should().OnlyContain(pi => pi.IsActive == false);
            await _repo.Received(1).AddStatusHistoryAsync(
                Arg.Is<ProfessionalStatusHistory>(h =>
                    h.ProfessionalId == pro.Id &&
                    h.OldStatus == ProfessionalStatusEnum.Approved &&
                    h.NewStatus == ProfessionalStatusEnum.Suspended &&
                    h.ChangedByUserId == AdminId &&
                    h.CreatedAt == Now),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task MultipleProfessionals_SuspendsAllAndSavesOnce()
        {
            var pros = new List<Professional> { InactivePro(), InactivePro() };
            _repo.GetInactiveProfessionalsAsync(30, Arg.Any<CancellationToken>())
                .Returns(pros);
            _http.GetCurrentUserId().Returns(AdminId);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(Cmd(days: 30), default);

            result.Data!.SuspendedCount.Should().Be(2);
            await _repo.Received(2).AddStatusHistoryAsync(Arg.Any<ProfessionalStatusHistory>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task NullAdminId_UsesEmptyGuidAsChangedBy()
        {
            var pro = InactivePro();
            _repo.GetInactiveProfessionalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new List<Professional> { pro });
            _http.GetCurrentUserId().Returns((Guid?)null);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _repo.Received(1).AddStatusHistoryAsync(
                Arg.Is<ProfessionalStatusHistory>(h => h.ChangedByUserId == Guid.Empty),
                Arg.Any<CancellationToken>());
        }
    }
}
