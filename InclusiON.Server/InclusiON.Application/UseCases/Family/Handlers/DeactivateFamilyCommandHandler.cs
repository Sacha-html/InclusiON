using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class DeactivateFamilyCommandHandler : ICommandHandler<DeactivateFamilyCommand, ApiResponse<FamilyResponse>>
    {
        private readonly IFamilyRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateFamilyCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IAssignmentsRepository? _assignmentsRepository;
        private readonly IProfessionalsRepository? _professionalsRepository;
        private readonly IAdminInstitutionRepository? _adminInstitutionRepository;
        private readonly INotificationService? _notificationService;

        public DeactivateFamilyCommandHandler(
            IFamilyRepository repository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateFamilyCommandHandler> logger,
            IDateTimeProvider dateTime,
            IAssignmentsRepository? assignmentsRepository = null,
            IProfessionalsRepository? professionalsRepository = null,
            IAdminInstitutionRepository? adminInstitutionRepository = null,
            INotificationService? notificationService = null)
        {
            _repository                 = repository;
            _identityService            = identityService;
            _unitOfWork                 = unitOfWork;
            _logger                     = logger;
            _dateTime                   = dateTime;
            _assignmentsRepository      = assignmentsRepository;
            _professionalsRepository    = professionalsRepository;
            _adminInstitutionRepository = adminInstitutionRepository;
            _notificationService        = notificationService;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(DeactivateFamilyCommand command, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdForUpdateAsync(command.FamilyId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<FamilyResponse>.NotFound("Familiar");
            }

            var linkedPersonIds = family.PersonRepresentatives?
                .Where(pr => pr.IsActive)
                .Select(pr => pr.PersonId)
                .Distinct()
                .ToList() ?? new List<Guid>();

            // Desactivar relaciones y suspender alumnos que se queden sin representantes
            var suspendedStudents = await _repository.DeactivateRepresentativeAndSuspendDependentStudentsAsync(family.UserId, _dateTime.UtcNow, cancellationToken);
            suspendedStudents ??= new List<string>();

            if (family.User != null)
            {
                family.User.IsActive = false;
            }

            family.IsActive = false;
            family.Status = FamilyStatusEnum.Terminated;
            family.UpdatedAt = _dateTime.UtcNow;

            await _repository.CreateFamilyStatusHistoryAsync(new FamilyStatusHistory
            {
                FamilyId = family.Id,
                OldStatus = FamilyStatusEnum.Active,
                NewStatus = FamilyStatusEnum.Terminated,
                Observation = "Familiar desactivado",
                CreatedAt = _dateTime.UtcNow
            }, cancellationToken);
            await _repository.UpdateAsync(family, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Familiar desactivado: {FamilyId}", family.Id);

            if (_notificationService is not null && suspendedStudents.Count > 0)
            {
                var notifications = new List<NotificationMessage>();
                var familyName = $"{family.FirstName} {family.LastName}".Trim();
                var suspendedList = string.Join(", ", suspendedStudents);
                var profInstitutionIds = new HashSet<int>();

                if (_assignmentsRepository is not null)
                {
                    foreach (var personId in linkedPersonIds)
                    {
                        var profs = await _assignmentsRepository.GetProfessionalsByPersonIdAsync(personId, cancellationToken);
                        foreach (var pp in profs.Where(p => p.IsActive && p.Professional is not null && p.Professional.UserId != Guid.Empty))
                        {
                            if (_professionalsRepository is not null)
                            {
                                var instIds = await _professionalsRepository.GetInstitutionIdsAsync(pp.ProfessionalId, cancellationToken);
                                foreach (var instId in instIds) profInstitutionIds.Add(instId);
                            }

                            notifications.Add(new NotificationMessage(
                                UserId: pp.Professional.UserId,
                                Type: NotificationTypes.Alert,
                                EventCode: NotificationEventCodes.StudentSuspendedCb03,
                                Title: "Alerta: Alumno suspendido sin tutor activo",
                                Message: $"Al dar de baja al familiar {familyName}, se suspendió automáticamente a: {suspendedList}.",
                                ActionUrl: "/pro/persons"));
                        }
                    }
                }

                if (_adminInstitutionRepository is not null)
                {
                    var admins = await _adminInstitutionRepository.GetAdminsForInstitutionsIncludingGlobalAsync(
                        profInstitutionIds.ToList(), cancellationToken);
                    foreach (var admin in admins.Where(a => a.IsActive))
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: admin.Id,
                            Type: NotificationTypes.Alert,
                            EventCode: NotificationEventCodes.StudentSuspendedCb03,
                            Title: "Alumno(s) suspendido(s) por baja de tutor (CB-03)",
                            Message: $"Al dar de baja al familiar {familyName}, quedaron suspendidos automáticamente: {suspendedList}.",
                            ActionUrl: "/admin/persons"));
                    }
                }

                await _notificationService.NotifyManyAsync(notifications, cancellationToken);
            }

            var response = FamilyResponse.MapToResponse(family, false, suspendedStudents);
            var studentNames = string.Join(", ", suspendedStudents);
            var successMessage = suspendedStudents.Count > 0
                ? $"Familiar desvinculado exitosamente. Al ser el único tutor activo registrado, el alumno {studentNames} fue dado de baja automáticamente hasta que se registre o invite a un nuevo responsable legal"
                : "Familiar desactivado exitosamente";

            return ApiResponse<FamilyResponse>.SuccessResult(response, successMessage);
        }
    }
}
