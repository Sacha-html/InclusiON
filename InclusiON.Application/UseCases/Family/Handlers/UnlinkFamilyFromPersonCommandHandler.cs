using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class UnlinkFamilyFromPersonCommandHandler : ICommandHandler<UnlinkFamilyFromPersonCommand, ApiResponse<PersonRepresentativeResponse>>
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UnlinkFamilyFromPersonCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public UnlinkFamilyFromPersonCommandHandler(
            IFamilyRepository familyRepository,
            IUnitOfWork unitOfWork,
            ILogger<UnlinkFamilyFromPersonCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _familyRepository = familyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<PersonRepresentativeResponse>> HandleAsync(UnlinkFamilyFromPersonCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var link = await _familyRepository.GetPersonRepresentativeAsync(command.PersonId, command.FamilyId, cancellationToken);
                if (link == null)
                {
                    return ApiResponse<PersonRepresentativeResponse>.NotFound("Vinculacion");
                }

                if (!link.IsActive)
                {
                    return ApiResponse<PersonRepresentativeResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "La vinculacion ya esta inactiva");
                }

                var family = await _familyRepository.GetByIdAsync(command.FamilyId, cancellationToken);
                if (family == null)
                {
                    return ApiResponse<PersonRepresentativeResponse>.NotFound("Familiar");
                }

                if (string.IsNullOrWhiteSpace(command.Observation))
                {
                    return ApiResponse<PersonRepresentativeResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "El motivo de desvinculacion es requerido");
                }

                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    link.IsActive = false;
                    link.EndedAt = _dateTime.UtcNow;
                    link.UnlinkObservation = command.Observation;
                    link.UpdatedAt = _dateTime.UtcNow;

                    await _familyRepository.UpdatePersonRepresentativeAsync(link, ct);

                    var history = new PersonRepresentativeHistory
                    {
                        PersonRepresentativeId = link.Id,
                        PersonId = command.PersonId,
                        RepresentativeId = command.FamilyId,
                        ChangeType = PersonRepresentativeChangeType.Unlinked,
                        Relationship = link.Relationship,
                        WasPrimary = link.IsPrimary,
                        Observation = command.Observation,
                        ChangedByUserId = command.ChangedByUserId,
                        CreatedAt = _dateTime.UtcNow
                    };
                    await _familyRepository.CreatePersonRepresentativeHistoryAsync(history, ct);

                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation(
                    "Familiar {FamilyId} desvinculado de persona {PersonId} por usuario {UserId}. Motivo: {Observation}",
                    command.FamilyId, command.PersonId, command.ChangedByUserId, command.Observation);

                var response = PersonRepresentativeResponse.MapToResponse(
                    command.PersonId,
                    family,
                    link.Relationship!,
                    link.IsPrimary,
                    false,
                    _dateTime.UtcNow,
                    command.Observation);

                return ApiResponse<PersonRepresentativeResponse>.SuccessResult(response, "Familiar desvinculado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desvincular familiar {FamilyId} de persona {PersonId}",
                    command.FamilyId, command.PersonId);
                return ApiResponse<PersonRepresentativeResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Ocurrió un error al desvincular el familiar");
            }
        }
    }
}
