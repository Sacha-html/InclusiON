using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class ReactivateFamilyCommandHandler : ICommandHandler<ReactivateFamilyCommand, ApiResponse<FamilyResponse>>
    {
        private readonly IFamilyRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReactivateFamilyCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public ReactivateFamilyCommandHandler(
            IFamilyRepository repository,
            IIdentityService identityService,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            ILogger<ReactivateFamilyCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _identityService = identityService;
            _backgroundJobs = backgroundJobs;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(
            ReactivateFamilyCommand command, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdForUpdateAsync(command.FamilyId, cancellationToken);
            if (family is null)
                return ApiResponse<FamilyResponse>.NotFound("Familiar");

            if (family.IsActive && family.User.IsActive)
            {
                return ApiResponse<FamilyResponse>.ErrorResult(
                    ErrorCode.BusinessRuleViolation,
                    "El familiar ya se encuentra activo.");
            }

            var temporaryPassword = PasswordGenerator.GenerateTemporary();
            var now = _dateTime.UtcNow;
            var oldStatus = family.Status;
            IEnumerable<string> errors = [];
            var passwordReset = false;

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var resetResult = await _identityService.ResetPasswordAsync(family.User, temporaryPassword);
                passwordReset = resetResult.Succeeded;
                errors = resetResult.Errors;
                if (!passwordReset)
                    return;

                family.IsActive = true;
                family.Status = FamilyStatusEnum.Active;
                family.UpdatedAt = now;
                family.User.IsActive = true;
                family.User.MustChangePassword = true;
                family.User.LockoutEnd = null;
                family.User.AccessFailedCount = 0;

                await _identityService.UpdateUserAsync(family.User);
                await _repository.CreateFamilyStatusHistoryAsync(new FamilyStatusHistory
                {
                    FamilyId = family.Id,
                    OldStatus = oldStatus,
                    NewStatus = FamilyStatusEnum.Active,
                    Observation = "Familiar reactivado",
                    ChangedByUserId = command.RequestedByUserId,
                    CreatedAt = now,
                    CreatedBy = command.RequestedByUserId
                }, ct);
                await _repository.UpdateAsync(family, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            if (!passwordReset)
            {
                return ApiResponse<FamilyResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    $"Error al generar contraseña: {string.Join(", ", errors)}");
            }

            _logger.LogInformation("Familiar reactivado: {FamilyId}, Usuario: {UserId}", family.Id, family.UserId);

            if (!string.IsNullOrEmpty(family.User.Email))
            {
                await _backgroundJobs.CreateAsync(
                    JobTypes.Email,
                    JsonSerializer.Serialize(new EmailPayload
                    {
                        To = family.User.Email,
                        Subject = "Tu cuenta ha sido reactivada — InclusiON",
                        TemplateName = "AccountReactivated",
                        Replacements = new Dictionary<string, string?>
                        {
                            { "UserName", family.User.Name ?? family.FirstName },
                            { "TemporaryPassword", temporaryPassword },
                            { "Year", now.Year.ToString() }
                        }
                    }),
                    maxRetries: 2,
                    cancellationToken: cancellationToken);
            }

            var response = FamilyResponse.MapToResponse(family);
            response.TemporaryPassword = temporaryPassword;
            return ApiResponse<FamilyResponse>.SuccessResult(
                response,
                "Familiar reactivado exitosamente. Se enviaron las credenciales por email.");
        }
    }
}
