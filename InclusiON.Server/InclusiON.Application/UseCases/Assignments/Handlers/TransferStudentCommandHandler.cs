using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;
using InclusiON.Application.Auditing;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class TransferStudentCommandHandler
        : ICommandHandler<TransferStudentCommand, ApiResponse<TransferStudentResponse>>
    {
        private readonly IAssignmentsRepository _assignmentsRepo;
        private readonly IProfessionalsRepository _professionalsRepo;
        private readonly IPersonsRepository _personsRepo;
        private readonly IActivityAssignmentRepository _activityAssignmentRepo;
        private readonly IReportsRepository _reportsRepo;
        private readonly IAccessAuditLogger _auditLogger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IUnitOfWork _unitOfWork;

        public TransferStudentCommandHandler(
            IAssignmentsRepository assignmentsRepo,
            IProfessionalsRepository professionalsRepo,
            IPersonsRepository personsRepo,
            IActivityAssignmentRepository activityAssignmentRepo,
            IReportsRepository reportsRepo,
            IAccessAuditLogger auditLogger,
            IDateTimeProvider dateTime,
            IUnitOfWork unitOfWork)
        {
            _assignmentsRepo = assignmentsRepo;
            _professionalsRepo = professionalsRepo;
            _personsRepo = personsRepo;
            _activityAssignmentRepo = activityAssignmentRepo;
            _reportsRepo = reportsRepo;
            _auditLogger = auditLogger;
            _dateTime = dateTime;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<TransferStudentResponse>> HandleAsync(
            TransferStudentCommand command, CancellationToken cancellationToken)
        {
            // 1. Validar Persona (alumno)
            var person = await _personsRepo.GetByIdAsync(command.PersonId, cancellationToken);
            if (person == null)
            {
                return ApiResponse<TransferStudentResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    "No se encontró el alumno especificado.");
            }

            // 2. Validar Profesional de origen
            var fromProf = await _professionalsRepo.GetByIdAsync(command.FromProfessionalId, cancellationToken);
            if (fromProf == null)
            {
                return ApiResponse<TransferStudentResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    "No se encontró el profesional de origen especificado.");
            }

            // 3. Validar Profesional de destino
            var toProf = await _professionalsRepo.GetByIdAsync(command.ToProfessionalId, cancellationToken);
            if (toProf == null)
            {
                return ApiResponse<TransferStudentResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    "No se encontró el profesional de destino especificado.");
            }

            if (toProf.Status != ProfessionalStatusEnum.Approved)
            {
                return ApiResponse<TransferStudentResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotApproved,
                    "El profesional de destino no está aprobado en el sistema.");
            }

            // 4. Validar que exista el vínculo actual (y esté activo)
            var oldLink = await _assignmentsRepo.GetAssignmentAsync(command.FromProfessionalId, command.PersonId, cancellationToken);
            if (oldLink == null || !oldLink.IsActive)
            {
                return ApiResponse<TransferStudentResponse>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    "No existe una asignación activa entre el alumno y el profesional de origen.");
            }

            // 5. Desactivar el vínculo actual
            oldLink.IsActive = false;
            
            // 6. Activar/crear el nuevo vínculo
            var newLink = await _assignmentsRepo.GetAssignmentAsync(command.ToProfessionalId, command.PersonId, cancellationToken);
            bool wasPrimary = oldLink.IsPrimaryProfessional;
            bool oldCanSupervise = oldLink.CanSuperviseLogin;

            if (newLink != null)
            {
                newLink.IsActive = true;
                newLink.IsPrimaryProfessional = wasPrimary;
                newLink.CanSuperviseLogin = oldCanSupervise;
                newLink.AssignedAt = _dateTime.UtcNow;
            }
            else
            {
                newLink = new ProfessionalPerson
                {
                    ProfessionalId = command.ToProfessionalId,
                    PersonId = command.PersonId,
                    IsPrimaryProfessional = wasPrimary,
                    CanSuperviseLogin = oldCanSupervise,
                    AssignedAt = _dateTime.UtcNow,
                    IsActive = true
                };
                await _assignmentsRepo.CreateAssignmentAsync(newLink, cancellationToken);
            }

            // 7. Reasignar ActivityAssignments en progreso/pendientes
            var allAssignments = await _activityAssignmentRepo.GetByPersonIdAsync(command.PersonId, cancellationToken);
            var activeAssignments = allAssignments
                .Where(aa => aa.AssignedByProfessionalId == command.FromProfessionalId &&
                             (aa.StatusId == AssignmentStatuses.Pendiente || aa.StatusId == AssignmentStatuses.EnProgreso))
                .ToList();

            foreach (var aa in activeAssignments)
            {
                aa.AssignedByProfessionalId = command.ToProfessionalId;
                aa.UpdatedAt = _dateTime.UtcNow;
                await _activityAssignmentRepo.UpdateAsync(aa, cancellationToken);
            }

            // 8. Reasignar reportes borradores, enviados y rechazados
            var reportsPage = await _reportsRepo.GetPagedAsync(
                page: 1,
                pageSize: 999,
                search: null,
                personId: command.PersonId.ToString(),
                professionalId: command.FromProfessionalId.ToString(),
                reportTypeId: null,
                isActive: true,
                status: null,
                dateFrom: null,
                dateTo: null,
                sortBy: null,
                sortDirection: "ASC",
                cancellationToken: cancellationToken);

            var pendingReports = reportsPage.Data
                .Where(r => r.Status == ReportStatus.Draft || r.Status == ReportStatus.Submitted || r.Status == ReportStatus.Rejected)
                .ToList();

            foreach (var report in pendingReports)
            {
                await _reportsRepo.ReassignReportAsync(report, command.ToProfessionalId, _dateTime.UtcNow, cancellationToken);
            }

            // 9. Guardar cambios en UOW
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 10. Registrar auditoría
            await _auditLogger.LogAsync(new AccessAuditEntry
            {
                UserId = command.AdminUserId,
                Role = command.AdminRole,
                AccessedPersonId = command.PersonId,
                ActionType = "TransferStudent",
                Result = "Success",
                AffectedTable = "ProfessionalPersons",
                AffectedRecordId = oldLink.PersonId.ToString(),
                Details = $"Transferencia de alumno {person.FirstName} {person.LastName} ({command.PersonId}) del profesional {fromProf.FirstName} {fromProf.LastName} ({command.FromProfessionalId}) al profesional {toProf.FirstName} {toProf.LastName} ({command.ToProfessionalId}). Actividades reasignadas: {activeAssignments.Count}, Reportes reasignados: {pendingReports.Count}."
            }, cancellationToken);

            var response = new TransferStudentResponse
            {
                ReassignedActivitiesCount = activeAssignments.Count,
                ReassignedReportsCount = pendingReports.Count,
                Message = $"Se transfirió al alumno exitosamente. Se reasignaron {activeAssignments.Count} actividades y {pendingReports.Count} reportes pendientes."
            };

            return ApiResponse<TransferStudentResponse>.SuccessResult(response, "Transferencia realizada con éxito.");
        }
    }
}
