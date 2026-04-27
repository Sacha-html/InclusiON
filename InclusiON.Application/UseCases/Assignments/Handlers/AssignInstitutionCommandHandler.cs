using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class AssignInstitutionCommandHandler
        : ICommandHandler<AssignInstitutionCommand, ApiResponse<ProfessionalInstitutionResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IInstitutionsRepository _institutionsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public AssignInstitutionCommandHandler(
            IAssignmentsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IInstitutionsRepository institutionsRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _institutionsRepository = institutionsRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ProfessionalInstitutionResponse>> HandleAsync(
            AssignInstitutionCommand command, CancellationToken cancellationToken)
        {
            // Validar que el profesional existe
            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional == null)
            {
                return ApiResponse<ProfessionalInstitutionResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            // Validar que el profesional está aprobado
            if (professional.Status != ProfessionalStatusEnum.Approved)
            {
                return ApiResponse<ProfessionalInstitutionResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotApproved,
                    ErrorMessages.ProfessionalNotApprovedForInstitutionAssignment);
            }

            // Validar que la institucion existe
            var institution = await _institutionsRepository.GetByIdAsync(command.InstitutionId, cancellationToken);
            if (institution == null)
            {
                return ApiResponse<ProfessionalInstitutionResponse>.NotFound("Institucion educativa");
            }

            // Validar que no exista una asignacion activa
            var existing = await _repository.GetInstitutionAssignmentAsync(command.ProfessionalId, command.InstitutionId, cancellationToken);
            if (existing != null && existing.IsActive)
            {
                return ApiResponse<ProfessionalInstitutionResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    "La asignacion profesional-institucion ya existe y esta activa.");
            }

            // Si existe pero esta inactiva, reactivar
            if (existing != null && !existing.IsActive)
            {
                existing.IsActive = true;
                existing.AssignedAt = _dateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                existing.Institution = institution;
                var reactivatedResponse = ProfessionalInstitutionResponse.MapToResponse(existing);
                return ApiResponse<ProfessionalInstitutionResponse>.SuccessResult(reactivatedResponse, "Asignacion reactivada exitosamente.");
            }

            var assignment = new ProfessionalInstitution
            {
                ProfessionalId = command.ProfessionalId,
                InstitutionId = command.InstitutionId,
                AssignedAt = _dateTime.UtcNow,
                IsActive = true
            };

            await _repository.CreateInstitutionAssignmentAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            assignment.Institution = institution;
            var response = ProfessionalInstitutionResponse.MapToResponse(assignment);
            return ApiResponse<ProfessionalInstitutionResponse>.SuccessResult(response, "Institucion asignada al profesional exitosamente.");
        }
    }
}
