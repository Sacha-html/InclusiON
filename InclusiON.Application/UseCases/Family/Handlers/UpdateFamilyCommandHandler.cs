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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateFamilyCommandHandler> _logger;

        public UpdateFamilyCommandHandler(
            IFamilyRepository repository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<UpdateFamilyCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(UpdateFamilyCommand command, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdAsync(command.FamilyId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<FamilyResponse>.NotFound("Familiar");
            }

            if (!string.IsNullOrWhiteSpace(command.DocumentNumber))
            {
                var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, command.FamilyId, cancellationToken);
                if (documentExists)
                {
                    return ApiResponse<FamilyResponse>.Conflict(
                        ErrorCode.DocumentAlreadyExists,
                        ErrorMessages.DocumentAlreadyExists);
                }
            }

            // Actualizar email si cambio
            if (family.User != null && !string.Equals(family.User.Email, command.Email, StringComparison.OrdinalIgnoreCase))
            {
                // Verificar que el nuevo email no este en uso
                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null && existingUser.Id != family.UserId)
                {
                    return ApiResponse<FamilyResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                family.User.Email = command.Email;
                family.User.UserName = command.Email;
                family.User.NormalizedEmail = command.Email.ToUpperInvariant();
                family.User.NormalizedUserName = command.Email.ToUpperInvariant();
            }

            family.FirstName = command.FirstName;
            family.LastName = command.LastName;
            family.DocumentNumber = command.DocumentNumber;
            family.Phone = command.Phone;
            family.Relationship = command.Relationship;
            family.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(family, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Familiar actualizado: {FamilyId}", family.Id);

            var response = FamilyResponse.MapToResponse(family);
            return ApiResponse<FamilyResponse>.SuccessResult(response, "Familiar actualizado exitosamente");
        }
    }
}
