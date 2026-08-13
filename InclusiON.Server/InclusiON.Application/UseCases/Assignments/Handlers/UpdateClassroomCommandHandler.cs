using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    /// <summary>
    /// Manejador para renombrar un aula existente.
    /// </summary>
    public class UpdateClassroomCommandHandler
        : ICommandHandler<UpdateClassroomCommand, ApiResponse<ClassroomResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateClassroomCommandHandler(IAssignmentsRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ClassroomResponse>> HandleAsync(
            UpdateClassroomCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                return ApiResponse<ClassroomResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    "El nombre del aula no puede estar vacío.");

            var classroom = await _repository.GetClassroomByIdAsync(command.ClassroomId, cancellationToken);
            if (classroom == null)
                return ApiResponse<ClassroomResponse>.ErrorResult(
                    ErrorCode.NotFound,
                    "El aula no existe.");

            if (classroom.ProfessionalId != command.ProfessionalId)
                return ApiResponse<ClassroomResponse>.ErrorResult(
                    ErrorCode.Forbidden,
                    "El aula no pertenece a este profesional.");

            await _repository.UpdateClassroomAsync(command.ClassroomId, command.Name, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Recargar para obtener datos actualizados
            var updated = await _repository.GetClassroomByIdAsync(command.ClassroomId, cancellationToken);
            return ApiResponse<ClassroomResponse>.SuccessResult(
                ClassroomResponse.MapToResponse(updated!),
                "Aula renombrada exitosamente.");
        }
    }
}
