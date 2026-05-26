using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Application.UseCases.Invitations.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Invitations
{
    public class CreateInvitationCommandHandlerTests
    {
        private readonly IInvitationsRepository   _invRepo    = Substitute.For<IInvitationsRepository>();
        private readonly IProfessionalsRepository _prosRepo   = Substitute.For<IProfessionalsRepository>();
        private readonly IIdentityService         _identity   = Substitute.For<IIdentityService>();
        private readonly IUnitOfWork              _uow        = Substitute.For<IUnitOfWork>();
        private readonly IBackgroundJobRepository _backgroundJobs = Substitute.For<IBackgroundJobRepository>();
        private readonly IDateTimeProvider        _dateTime   = Substitute.For<IDateTimeProvider>();

        private CreateInvitationCommandHandler BuildSut() =>
            new(_invRepo, _prosRepo, _identity, _uow, _backgroundJobs,
                NullLogger<CreateInvitationCommandHandler>.Instance, _dateTime);

        private static readonly Guid ProfId = Guid.NewGuid();

        private static CreateInvitationCommand Cmd(string email = "familiar@test.com") =>
            new(ProfId, null, email, "Ana", "Lopez", "Madre", null);

        private static Professional ApprovedPro() => new()
        {
            Id = ProfId, Status = ProfessionalStatusEnum.Approved,
            User = new User { IsActive = true }, ProfessionalInstitutions = [],
        };

        // ── Profesional no encontrado ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsProfessionalNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        // ── Profesional no aprobado ──────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotApproved_ReturnsProfessionalNotApproved()
        {
            var pro = ApprovedPro();
            pro.Status = ProfessionalStatusEnum.Pending;
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(pro);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotApproved);
        }

        // ── Email ya registrado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmailAlreadyExists_ReturnsEmailAlreadyExists()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _identity.FindByEmailAsync("familiar@test.com").Returns(new User());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_CreatesInvitationAndSaves()
        {
            var now = DateTime.UtcNow;
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _identity.FindByEmailAsync("familiar@test.com").Returns((User?)null);
            _dateTime.UtcNow.Returns(now);
            _invRepo.CreateAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>())
                    .Returns(ci => (Invitation)ci[0]);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _invRepo.Received(1).CreateAsync(
                Arg.Is<Invitation>(i => i.Email == "familiar@test.com" && !i.IsUsed && i.CreatedByProfessionalId == ProfId),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
