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
    public class DeactivateProfessionalCommandHandlerTests
    {
        private readonly IProfessionalsRepository  _prosRepo   = Substitute.For<IProfessionalsRepository>();
        private readonly IRefreshTokensRepository  _tokenRepo  = Substitute.For<IRefreshTokensRepository>();
        private readonly IHttpContextService       _httpCtx    = Substitute.For<IHttpContextService>();
        private readonly IUnitOfWork               _uow        = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider         _dateTime   = Substitute.For<IDateTimeProvider>();

        private DeactivateProfessionalCommandHandler BuildSut() =>
            new(_prosRepo, _tokenRepo, _httpCtx, _uow,
                NullLogger<DeactivateProfessionalCommandHandler>.Instance, _dateTime);

        private static readonly Guid ProfId  = Guid.NewGuid();
        private static readonly Guid AdminId = Guid.NewGuid();

        private static DeactivateProfessionalCommand Cmd() =>
            new(ProfessionalId: ProfId, Observation: "Baja voluntaria");

        private static Professional AProfessional(ProfessionalStatusEnum status = ProfessionalStatusEnum.Approved) =>
            new()
            {
                Id     = ProfId,
                UserId = Guid.NewGuid(),
                Status = status,
                User   = new User { IsActive = true },
                ProfessionalInstitutions = [],
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

        // ── Ya dado de baja ──────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyTerminated_ReturnsBusinessRuleViolation()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(AProfessional(ProfessionalStatusEnum.Terminated));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.BusinessRuleViolation);
        }

        // ── Sin admin en contexto ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoAdminInContext_ReturnsUnauthorized()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns(AProfessional());
            _httpCtx.GetCurrentUserId().Returns((Guid?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Unauthorized);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ActiveProfessional_SetsTerminatedAndRevokesTokens()
        {
            var professional = AProfessional();
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(professional);
            _httpCtx.GetCurrentUserId().Returns(AdminId);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            professional.Status.Should().Be(ProfessionalStatusEnum.Terminated);
            professional.User.IsActive.Should().BeFalse();
            await _tokenRepo.Received(1).RevokeAllUserTokensAsync(
                professional.UserId, Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
