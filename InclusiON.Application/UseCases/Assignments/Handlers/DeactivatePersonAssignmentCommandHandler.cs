using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class DeactivatePersonAssignmentCommandHandler
        : ICommandHandler<DeactivatePersonAssignmentCommand, ApiResponse<ProfessionalPersonResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivatePersonAssignmentCommandHandler(
            IAssignmentsRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ProfessionalPersonResponse>> HandleAsync(
            DeactivatePersonAssignmentCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetAssignmentAsync(command.ProfessionalId, command.PersonId, cancellationToken);

            if (assignment == null)
            {
                return ApiResponse<ProfessionalPersonResponse>.NotFound("Asignacion profesional-persona");
            }

            if (!assignment.IsActive)
            {
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    "La asignacion ya se encuentra inactiva.");
            }

            assignment.IsActive = false;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = GetPersonsByProfessionalQueryHandler.MapToResponse(assignment);
            return ApiResponse<ProfessionalPersonResponse>.SuccessResult(response, "Asignacion desactivada exitosamente.");
        }
    }
}
