using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class MovePersonToClassroomCommandHandler
        : ICommandHandler<MovePersonToClassroomCommand, ApiResponse<ProfessionalPersonResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public MovePersonToClassroomCommandHandler(
            IAssignmentsRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ProfessionalPersonResponse>> HandleAsync(
            MovePersonToClassroomCommand command,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _repository.MovePersonToClassroomAsync(
                command.ProfessionalId,
                command.PersonId,
                command.ClassroomId,
                cancellationToken);

            if (assignment == null)
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.NotFound,
                    "No se encontró una asignación activa para este alumno y profesional.");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = ProfessionalPersonResponse.MapToResponse(assignment);
            return ApiResponse<ProfessionalPersonResponse>.SuccessResult(
                response,
                command.ClassroomId.HasValue
                    ? "Alumno movido al aula correctamente."
                    : "Alumno desvinculado del aula correctamente.");
        }
    }
}
