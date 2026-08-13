using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    /// <summary>
    /// Manejador de comandos para la creación de un aula y la asignación masiva de alumnos.
    /// </summary>
    public class CreateClassroomCommandHandler
        : ICommandHandler<CreateClassroomCommand, ApiResponse<List<ProfessionalPersonResponse>>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public CreateClassroomCommandHandler(
            IAssignmentsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<List<ProfessionalPersonResponse>>> HandleAsync(
            CreateClassroomCommand command, CancellationToken cancellationToken)
        {
            // Validar que el profesional existe
            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional == null)
            {
                return ApiResponse<List<ProfessionalPersonResponse>>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    "El profesional no existe.");
            }

            // Validar que el profesional está aprobado
            if (professional.Status != ProfessionalStatusEnum.Approved)
            {
                return ApiResponse<List<ProfessionalPersonResponse>>.ErrorResult(
                    ErrorCode.ProfessionalNotApproved,
                    "El profesional debe estar aprobado para asignarle un aula.");
            }

            // Validar caso de borde: nombre vacío
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return ApiResponse<List<ProfessionalPersonResponse>>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    "El nombre del aula no puede estar vacío.");
            }

            if (command.PersonIds == null || command.PersonIds.Count == 0)
            {
                return ApiResponse<List<ProfessionalPersonResponse>>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    "Debe seleccionar al menos un alumno para crear el aula.");
            }

            // Crear el Aula
            var classroom = new Classroom
            {
                Id = Guid.NewGuid(),
                Name = command.Name.Trim(),
                ProfessionalId = command.ProfessionalId,
                IsActive = true,
                CreatedAt = _dateTime.UtcNow
            };

            await _repository.CreateClassroomAsync(classroom, cancellationToken);

            var resultList = new List<ProfessionalPerson>();

            // Procesar asignaciones de personas (si hay)
            var personIds = command.PersonIds ?? new List<Guid>();
            foreach (var personId in personIds)
            {
                var person = await _personsRepository.GetByIdAsync(personId, cancellationToken);
                if (person == null)
                {
                    return ApiResponse<List<ProfessionalPersonResponse>>.ErrorResult(
                        ErrorCode.PersonNotFound,
                        $"El alumno con ID {personId} no existe.");
                }

                var existing = await _repository.GetAssignmentAsync(command.ProfessionalId, personId, cancellationToken);

                if (existing != null)
                {
                    // Si ya existe (activa o inactiva), la asociamos al aula y la activamos
                    existing.IsActive = true;
                    existing.ClassroomId = classroom.Id;
                    existing.IsPrimaryProfessional = command.IsPrimaryProfessional;
                    existing.CanSuperviseLogin = command.CanSuperviseLogin;
                    existing.AssignedAt = _dateTime.UtcNow;
                    existing.Person = person;
                    existing.Classroom = classroom;
                    resultList.Add(existing);
                }
                else
                {
                    // Crear nueva asignación
                    var assignment = new ProfessionalPerson
                    {
                        ProfessionalId = command.ProfessionalId,
                        PersonId = personId,
                        ClassroomId = classroom.Id,
                        IsPrimaryProfessional = command.IsPrimaryProfessional,
                        CanSuperviseLogin = command.CanSuperviseLogin,
                        AssignedAt = _dateTime.UtcNow,
                        IsActive = true,
                        Person = person,
                        Classroom = classroom
                    };

                    await _repository.CreateAssignmentAsync(assignment, cancellationToken);
                    resultList.Add(assignment);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = resultList.Select(ProfessionalPersonResponse.MapToResponse).ToList();
            return ApiResponse<List<ProfessionalPersonResponse>>.SuccessResult(response, "Aula creada y alumnos asignados exitosamente.");
        }
    }
}
