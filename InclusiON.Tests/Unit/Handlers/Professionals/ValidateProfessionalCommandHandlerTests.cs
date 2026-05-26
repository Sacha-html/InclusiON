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
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    public class ValidateProfessionalCommandHandlerTests
    {
        private readonly IProfessionalsRepository     _prosRepo   = Substitute.For<IProfessionalsRepository>();
        private readonly IIdentityService             _identity   = Substitute.For<IIdentityService>();
        private readonly IBackgroundJobRepository     _backgroundJobs = Substitute.For<IBackgroundJobRepository>();
        private readonly IAdminInstitutionRepository  _adminRepo  = Substitute.For<IAdminInstitutionRepository>();
        private readonly IHttpContextService          _httpCtx    = Substitute.For<IHttpContextService>();
        private readonly IUnitOfWork                  _uow        = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider            _dateTime   = Substitute.For<IDateTimeProvider>();

        private ValidateProfessionalCommandHandler BuildSut() =>
            new(_prosRepo, _identity, _backgroundJobs, _adminRepo, _httpCtx, _uow,
                NullLogger<ValidateProfessionalCommandHandler>.Instance, _dateTime);

        private static readonly Guid ProfId  = Guid.NewGuid();
        private static readonly Guid AdminId = Guid.NewGuid();

        private static ValidateProfessionalCommand ApproveCmd() =>
            new(ProfessionalId: ProfId, IsApproved: true, Observation: null);

        private static ValidateProfessionalCommand RejectCmd(string? obs = "Documentación incompleta") =>
            new(ProfessionalId: ProfId, IsApproved: false, Observation: obs);

        private static Professional APendingProfessional() => new()
        {
            Id        = ProfId,
            UserId    = Guid.NewGuid(),
            FirstName = "Ana",
            LastName  = "García",
            Status    = ProfessionalStatusEnum.Pending,
            User      = new User { IsActive = false, Email = "ana@test.com" },
            ProfessionalInstitutions = [],
        };

        private void SetupGlobalAdmin()
        {
            _httpCtx.GetCurrentUserId().Returns(AdminId);
            // Lista vacía → admin global
            _adminRepo.GetActiveInstitutionIdsByAdminAsync(AdminId, Arg.Any<CancellationToken>())
                      .Returns(new List<int>());
            _prosRepo.GetInstitutionIdsAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(new List<int>());
        }

        private void SetupTransaction() =>
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));

        // ── Profesional no encontrado ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(ApproveCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Estado no pendiente ──────────────────────────────────────────────

        [Theory]
        [InlineData(ProfessionalStatusEnum.Approved)]
        [InlineData(ProfessionalStatusEnum.Rejected)]
        [InlineData(ProfessionalStatusEnum.Terminated)]
        public async Task HandleAsync_StatusNotPending_ReturnsBusinessRuleViolation(ProfessionalStatusEnum status)
        {
            var professional = APendingProfessional();
            professional.Status = status;
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(professional);

            var result = await BuildSut().HandleAsync(ApproveCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        // ── Sin admin en contexto ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoAdminInContext_ReturnsUnauthorized()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(APendingProfessional());
            _httpCtx.GetCurrentUserId().Returns((Guid?)null);

            var result = await BuildSut().HandleAsync(ApproveCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Unauthorized);
        }

        // ── Rechazo sin motivo ───────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task HandleAsync_RejectWithoutObservation_ReturnsValidationFailed(string? obs)
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(APendingProfessional());
            SetupGlobalAdmin();

            var result = await BuildSut().HandleAsync(RejectCmd(obs), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Happy path: aprobación ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Approve_SetsApprovedAndActivatesUser()
        {
            var professional = APendingProfessional();
            var user         = professional.User;
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(professional);
            SetupGlobalAdmin();
            SetupTransaction();
            _identity.FindByIdAsync(professional.UserId).Returns(user);
            _identity.UpdateUserAsync(Arg.Any<User>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.ResetPasswordAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));

            var result = await BuildSut().HandleAsync(ApproveCmd(), default);

            result.Success.Should().BeTrue();
            professional.Status.Should().Be(ProfessionalStatusEnum.Approved);
            user.IsActive.Should().BeTrue();
            user.MustChangePassword.Should().BeTrue();
        }

        // ── Happy path: rechazo ───────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Reject_SetsRejectedAndSaves()
        {
            var professional = APendingProfessional();
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(professional);
            SetupGlobalAdmin();

            var result = await BuildSut().HandleAsync(RejectCmd(), default);

            result.Success.Should().BeTrue();
            professional.Status.Should().Be(ProfessionalStatusEnum.Rejected);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
