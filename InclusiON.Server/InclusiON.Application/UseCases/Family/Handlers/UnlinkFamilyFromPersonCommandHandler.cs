using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Domain.Enums;
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
        private readonly IAssignmentsRepository? _assignmentsRepository;
        private readonly IProfessionalsRepository? _professionalsRepository;
        private readonly IAdminInstitutionRepository? _adminInstitutionRepository;
        private readonly INotificationService? _notificationService;

        public UnlinkFamilyFromPersonCommandHandler(
            IFamilyRepository familyRepository,
            IUnitOfWork unitOfWork,
            ILogger<UnlinkFamilyFromPersonCommandHandler> logger,
            IDateTimeProvider dateTime,
            IAssignmentsRepository? assignmentsRepository = null,
            IProfessionalsRepository? professionalsRepository = null,
            IAdminInstitutionRepository? adminInstitutionRepository = null,
            INotificationService? notificationService = null)
        {
            _familyRepository           = familyRepository;
            _unitOfWork                 = unitOfWork;
            _logger                     = logger;
            _dateTime                   = dateTime;
            _assignmentsRepository      = assignmentsRepository;
            _professionalsRepository    = professionalsRepository;
            _adminInstitutionRepository = adminInstitutionRepository;
            _notificationService        = notificationService;
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

                bool studentSuspended = false;

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

                    // CB-03: Si era el único tutor activo registrado, dar de baja automáticamente al alumno
                    var remainingActiveTutors = await _familyRepository.GetActiveRepresentativesCountForPersonAsync(
                        command.PersonId, command.FamilyId, ct);

                    if (remainingActiveTutors == 0 && link.Person != null)
                    {
                        link.Person.IsActive = false;
                        link.Person.UpdatedAt = _dateTime.UtcNow;
                        link.Person.UpdatedBy = command.ChangedByUserId;
                        if (link.Person.User != null)
                        {
                            link.Person.User.IsActive = false;
                        }
                        studentSuspended = true;
                    }

                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation(
                    "Familiar {FamilyId} desvinculado de persona {PersonId} por usuario {UserId}. Motivo: {Observation}. Alumno suspendido: {StudentSuspended}",
                    command.FamilyId, command.PersonId, command.ChangedByUserId, command.Observation, studentSuspended);

                var studentName = link.Person != null
                    ? $"{link.Person.FirstName} {link.Person.LastName}".Trim()
                    : string.Empty;

                if (_notificationService is not null)
                {
                    var notifications = new List<NotificationMessage>();
                    var familyName = $"{family.FirstName} {family.LastName}".Trim();
                    var encryptedPersonId = _notificationService.EncryptId(command.PersonId);
                    var displayStudent = string.IsNullOrWhiteSpace(studentName) ? "el alumno" : studentName;

                    if (family.UserId != Guid.Empty)
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: family.UserId,
                            Type: NotificationTypes.Alert,
                            EventCode: NotificationEventCodes.FamilyUnlinked,
                            Title: "Vínculo familiar finalizado",
                            Message: $"Se finalizó tu vinculación como representante familiar de {displayStudent}.",
                            ActionUrl: "/family"));
                    }

                    var profInstitutionIds = new HashSet<int>();
                    if (_assignmentsRepository is not null)
                    {
                        var profs = await _assignmentsRepository.GetProfessionalsByPersonIdAsync(command.PersonId, cancellationToken);
                        foreach (var pp in profs.Where(p => p.IsActive && p.Professional is not null && p.Professional.UserId != Guid.Empty))
                        {
                            if (_professionalsRepository is not null)
                            {
                                var instIds = await _professionalsRepository.GetInstitutionIdsAsync(pp.ProfessionalId, cancellationToken);
                                foreach (var instId in instIds) profInstitutionIds.Add(instId);
                            }

                            if (pp.Professional.UserId == command.ChangedByUserId) continue;

                            notifications.Add(new NotificationMessage(
                                UserId: pp.Professional.UserId,
                                Type: studentSuspended ? NotificationTypes.Alert : NotificationTypes.Assignment,
                                EventCode: studentSuspended ? NotificationEventCodes.StudentSuspendedCb03 : NotificationEventCodes.FamilyUnlinked,
                                Title: studentSuspended ? "Alerta: Alumno suspendido sin tutor activo" : "Familiar desvinculado",
                                Message: studentSuspended
                                    ? $"{familyName} fue desvinculado de {displayStudent}. Al quedar sin tutor activo, el alumno fue suspendido automáticamente."
                                    : $"{familyName} fue desvinculado como representante de {displayStudent}.",
                                ActionUrl: $"/pro/persons/{encryptedPersonId}"));
                        }
                    }

                    if (studentSuspended && _adminInstitutionRepository is not null)
                    {
                        var admins = await _adminInstitutionRepository.GetAdminsForInstitutionsIncludingGlobalAsync(
                            profInstitutionIds.ToList(), cancellationToken);
                        foreach (var admin in admins.Where(a => a.IsActive && a.Id != command.ChangedByUserId))
                        {
                            notifications.Add(new NotificationMessage(
                                UserId: admin.Id,
                                Type: NotificationTypes.Alert,
                                EventCode: NotificationEventCodes.StudentSuspendedCb03,
                                Title: "Alumno suspendido por falta de tutor (CB-03)",
                                Message: $"El alumno {displayStudent} fue suspendido automáticamente tras desvincular a su único tutor activo ({familyName}).",
                                ActionUrl: $"/admin/persons/{encryptedPersonId}"));
                        }
                    }

                    await _notificationService.NotifyManyAsync(notifications, cancellationToken);
                }

                var response = PersonRepresentativeResponse.MapToResponse(
                    command.PersonId,
                    family,
                    link.Relationship!,
                    link.IsPrimary,
                    false,
                    _dateTime.UtcNow,
                    command.Observation,
                    studentSuspended,
                    studentName);

                var successMessage = studentSuspended
                    ? $"Familiar desvinculado exitosamente. Al ser el único tutor activo registrado, el alumno {studentName} fue dado de baja automáticamente hasta que se registre o invite a un nuevo responsable legal"
                    : "Familiar desvinculado exitosamente";

                return ApiResponse<PersonRepresentativeResponse>.SuccessResult(response, successMessage);
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
