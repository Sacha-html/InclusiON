using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class LinkFamilyToPersonCommandHandler : ICommandHandler<LinkFamilyToPersonCommand, ApiResponse<PersonRepresentativeResponse>>
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LinkFamilyToPersonCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public LinkFamilyToPersonCommandHandler(
            IFamilyRepository familyRepository,
            IUnitOfWork unitOfWork,
            ILogger<LinkFamilyToPersonCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _familyRepository = familyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<PersonRepresentativeResponse>> HandleAsync(LinkFamilyToPersonCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var family = await _familyRepository.GetByIdAsync(command.FamilyId, cancellationToken);
                if (family == null)
                {
                    return ApiResponse<PersonRepresentativeResponse>.NotFound("Familiar");
                }

                if (!family.User.IsActive || family.Status != FamilyStatusEnum.Active)
                {
                    return ApiResponse<PersonRepresentativeResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "El familiar no esta activo");
                }

                var existingLink = await _familyRepository.GetPersonRepresentativeAsync(command.PersonId, command.FamilyId, cancellationToken);
                if (existingLink != null && existingLink.IsActive)
                {
                    return ApiResponse<PersonRepresentativeResponse>.Conflict(
                        ErrorCode.Conflict,
                        "El familiar ya esta vinculado a esta persona");
                }

                var relationship = command.Relationship;
                if (relationship is "Madre" or "Padre")
                {
                    var existingOfType = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(command.PersonId, cancellationToken);
                    if (existingOfType.Any(pr => pr.IsActive && pr.Relationship == relationship))
                    {
                        return ApiResponse<PersonRepresentativeResponse>.Conflict(
                            ErrorCode.Conflict,
                            $"Ya existe un familiar con relacion '{relationship}' vinculado a esta persona");
                    }
                }

                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (command.IsPrimary)
                    {
                        var currentPrimary = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(command.PersonId, ct);
                        foreach (var pr in currentPrimary.Where(p => p.IsActive))
                        {
                            pr.IsPrimary = false;
                            pr.UpdatedAt = _dateTime.UtcNow;
                            await _familyRepository.UpdatePersonRepresentativeAsync(pr, ct);
                        }
                    }

                    PersonRepresentative personRepresentative;

                    if (existingLink != null && !existingLink.IsActive)
                    {
                        existingLink.IsActive = true;
                        existingLink.Relationship = command.Relationship;
                        existingLink.IsPrimary = command.IsPrimary;
                        existingLink.UpdatedAt = _dateTime.UtcNow;
                        existingLink.EndedAt = null;
                        existingLink.UnlinkObservation = null;
                        personRepresentative = existingLink;
                        await _familyRepository.UpdatePersonRepresentativeAsync(personRepresentative, ct);
                    }
                    else
                    {
                        personRepresentative = new PersonRepresentative
                        {
                            PersonId = command.PersonId,
                            RepresentativeId = command.FamilyId,
                            Relationship = command.Relationship,
                            IsPrimary = command.IsPrimary,
                            IsActive = true,
                            HasInformedConsent = false,
                            CanSuperviseLogin = true,
                            CreatedAt = _dateTime.UtcNow
                        };

                        await _familyRepository.CreatePersonRepresentativeAsync(personRepresentative, ct);
                    }

                    var history = new PersonRepresentativeHistory
                    {
                        PersonRepresentativeId = personRepresentative.Id,
                        PersonId = command.PersonId,
                        RepresentativeId = command.FamilyId,
                        ChangeType = PersonRepresentativeChangeType.Linked,
                        Relationship = command.Relationship,
                        WasPrimary = command.IsPrimary,
                        ChangedByUserId = command.ChangedByUserId,
                        CreatedAt = _dateTime.UtcNow
                    };
                    await _familyRepository.CreatePersonRepresentativeHistoryAsync(history, ct);

                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation(
                    "Familiar {FamilyId} vinculado a persona {PersonId} por usuario {UserId}",
                    command.FamilyId, command.PersonId, command.ChangedByUserId);

                var response = PersonRepresentativeResponse.MapToResponse(
                    command.PersonId,
                    family,
                    command.Relationship,
                    command.IsPrimary);

                return ApiResponse<PersonRepresentativeResponse>.SuccessResult(response, "Familiar vinculado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vincular familiar {FamilyId} a persona {PersonId}",
                    command.FamilyId, command.PersonId);
                return ApiResponse<PersonRepresentativeResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Ocurrió un error al vincular el familiar");
            }
        }
    }
}
