using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class UpdateFamilyCommandHandler : ICommandHandler<UpdateFamilyCommand, ApiResponse<FamilyResponse>>
    {
        private readonly IFamilyRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IUniquenessValidationService _uniquenessValidation;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateFamilyCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public UpdateFamilyCommandHandler(
            IFamilyRepository repository,
            IIdentityService identityService,
            IUniquenessValidationService uniquenessValidation,
            IUnitOfWork unitOfWork,
            ILogger<UpdateFamilyCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _identityService = identityService;
            _uniquenessValidation = uniquenessValidation;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(UpdateFamilyCommand command, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdAsync(command.FamilyId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<FamilyResponse>.NotFound("Familiar");
            }

            var normalizedDoc = UniquenessNormalizer.NormalizeDocument(command.DocumentNumber);
            var normalizedEmail = UniquenessNormalizer.NormalizeEmail(command.Email) ?? string.Empty;

            if (normalizedDoc is not null)
            {
                var docCheck = await _uniquenessValidation.CheckDocumentAsync(normalizedDoc, command.FamilyId, cancellationToken);
                if (!docCheck.IsAvailable)
                {
                    return ApiResponse<FamilyResponse>.Conflict(
                        ErrorCode.DocumentAlreadyExists,
                        docCheck.Message ?? ErrorMessages.DocumentAlreadyExists);
                }
            }

            // Actualizar email si cambio
            if (family.User != null && !string.Equals(family.User.Email?.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                // Verificar unicidad global del nuevo email
                var emailCheck = await _uniquenessValidation.CheckEmailAsync(
                    normalizedEmail,
                    excludeUserId: family.UserId,
                    excludeEntityId: family.Id,
                    cancellationToken: cancellationToken);
                if (!emailCheck.IsAvailable)
                {
                    return ApiResponse<FamilyResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        emailCheck.Message ?? ErrorMessages.EmailAlreadyRegistered);
                }

                family.User.Email = normalizedEmail;
                family.User.UserName = normalizedEmail;
                family.User.NormalizedEmail = normalizedEmail.ToUpperInvariant();
                family.User.NormalizedUserName = normalizedEmail.ToUpperInvariant();
            }

            family.FirstName = command.FirstName.Trim();
            family.LastName = command.LastName.Trim();
            family.DocumentNumber = normalizedDoc;
            family.Phone = command.Phone?.Trim();
            if (command.Relationship is not null)
                family.Relationship = command.Relationship;
            family.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(family, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Familiar actualizado: {FamilyId}", family.Id);

            var response = FamilyResponse.MapToResponse(family);
            return ApiResponse<FamilyResponse>.SuccessResult(response, "Familiar actualizado exitosamente");
        }
    }
}
