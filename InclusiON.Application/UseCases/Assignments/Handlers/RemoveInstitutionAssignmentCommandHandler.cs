using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class RemoveInstitutionAssignmentCommandHandler
        : ICommandHandler<RemoveInstitutionAssignmentCommand, ApiResponse<ProfessionalInstitutionResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveInstitutionAssignmentCommandHandler(
            IAssignmentsRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ProfessionalInstitutionResponse>> HandleAsync(
            RemoveInstitutionAssignmentCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetInstitutionAssignmentAsync(command.ProfessionalId, command.InstitutionId, cancellationToken);

            if (assignment == null)
            {
                return ApiResponse<ProfessionalInstitutionResponse>.NotFound("Asignacion profesional-institucion");
            }

            if (!assignment.IsActive)
            {
                return ApiResponse<ProfessionalInstitutionResponse>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    "La asignacion ya se encuentra inactiva.");
            }

            assignment.IsActive = false;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = ProfessionalInstitutionResponse.MapToResponse(assignment);
            return ApiResponse<ProfessionalInstitutionResponse>.SuccessResult(response, "Asignacion removida exitosamente.");
        }
    }
}
