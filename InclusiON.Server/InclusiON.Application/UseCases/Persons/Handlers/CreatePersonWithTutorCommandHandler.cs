using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Mappers;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Shared.Constants;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    /// <summary>
    /// Handler para crear una persona con discapacidad junto con su tutor a cargo
    /// y asignación de aula en una única transacción de base de datos.
    /// </summary>
    public class CreatePersonWithTutorCommandHandler : ICommandHandler<CreatePersonWithTutorCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IAssignmentsRepository _assignmentsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly ILogger<CreatePersonWithTutorCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IRoadmapInitializer _roadmapInitializer;
        private readonly IRealTimeNotifier _notifier;

        public CreatePersonWithTutorCommandHandler(
            IPersonsRepository repository,
            IFamilyRepository familyRepository,
            IAssignmentsRepository assignmentsRepository,
            IProfessionalsRepository professionalsRepository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IBackgroundJobRepository backgroundJobs,
            ILogger<CreatePersonWithTutorCommandHandler> logger,
            IDateTimeProvider dateTime,
            IRoadmapInitializer roadmapInitializer,
            IRealTimeNotifier notifier)
        {
            _repository = repository;
            _familyRepository = familyRepository;
            _assignmentsRepository = assignmentsRepository;
            _professionalsRepository = professionalsRepository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
            _logger = logger;
            _dateTime = dateTime;
            _roadmapInitializer = roadmapInitializer;
            _notifier = notifier;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(CreatePersonWithTutorCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validar documento de alumno único si se proporciona
                PersonWithDisability? existingStudent = null;
                if (!string.IsNullOrWhiteSpace(command.DocumentNumber))
                {
                    existingStudent = await _repository.GetByDocumentNumberForUpdateAsync(command.DocumentNumber, cancellationToken);
                    if (existingStudent != null && existingStudent.IsActive && (existingStudent.User == null || existingStudent.User.IsActive))
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            $"Ya existe un alumno activo registrado con el DNI {command.DocumentNumber} ({existingStudent.FirstName} {existingStudent.LastName}).");
                    }
                }

                // 2. Validar tutor y email
                var existingTutorUser = await _identityService.FindByEmailAsync(command.TutorEmail);
                FamilyRepresentative? existingTutor = null;

                if (existingTutorUser != null)
                {
                    // Si el tutor ya existe y el alumno es nuevo, rechazar por email duplicado si no es familiar
                    existingTutor = await _familyRepository.GetByUserIdAsync(existingTutorUser.Id, cancellationToken);
                    if (existingTutor == null && existingStudent == null)
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.EmailAlreadyExists,
                            "El email del tutor ya se encuentra registrado.");
                    }
                }
                else
                {
                    // Tutor nuevo: validar documento de tutor único
                    if (!string.IsNullOrWhiteSpace(command.TutorDocumentNumber))
                    {
                        var tutorDocExists = await _familyRepository.ExistsDocumentAsync(command.TutorDocumentNumber, null, cancellationToken);
                        if (tutorDocExists)
                        {
                            return ApiResponse<PersonResponse>.Conflict(
                                ErrorCode.DocumentAlreadyExists,
                                "El documento del tutor ya se encuentra registrado.");
                        }
                    }
                }

                // 3. Validar el profesional responsable y el aula (si se especifica)
                if (command.ProfessionalId == Guid.Empty)
                {
                    return ApiResponse<PersonResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "Debe seleccionar un profesional responsable para el alumno.");
                }

                var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
                if (professional == null)
                {
                    return ApiResponse<PersonResponse>.ErrorResult(
                        ErrorCode.NotFound,
                        "El profesional especificado no existe.");
                }

                if (command.ClassroomId.HasValue)
                {
                    var classroom = await _assignmentsRepository.GetClassroomByIdAsync(command.ClassroomId.Value, cancellationToken);
                    if (classroom == null)
                    {
                        return ApiResponse<PersonResponse>.ErrorResult(
                            ErrorCode.NotFound,
                            "El aula especificada no existe.");
                    }

                    if (classroom.ProfessionalId != command.ProfessionalId)
                    {
                        return ApiResponse<PersonResponse>.ErrorResult(
                            ErrorCode.ValidationFailed,
                            "El aula especificada no pertenece al profesional seleccionado.");
                    }
                }

                var isReactivation = existingStudent != null;
                var tutorPassword = PasswordGenerator.GenerateTemporary();
                var newTutorCreated = false;
                User? tutorUser = existingTutorUser;
                FamilyRepresentative? tutor = existingTutor;

                PersonWithDisability student;

                if (isReactivation)
                {
                    student = existingStudent!;
                }
                else
                {
                    // Preparar creación de alumno nuevo
                    var baseStudentUsername = GenerateUsername(command.FirstName, command.LastName);
                    var studentEmail = $"{baseStudentUsername}@inclusion.local";
                    var studentPassword = PasswordGenerator.GenerateTemporary();

                    var studentUser = new User
                    {
                        UserName = baseStudentUsername,
                        Email = studentEmail,
                        Name = command.FirstName,
                        Surname = command.LastName,
                        IsActive = true,
                        CreatedAt = _dateTime.UtcNow,
                        EmailConfirmed = true,
                        LockoutEnabled = true
                    };

                    student = new PersonWithDisability
                    {
                        FirstName = command.FirstName,
                        LastName = command.LastName,
                        DocumentNumber = command.DocumentNumber,
                        BirthDate = command.BirthDate,
                        PhotoUrl = command.PhotoUrl,
                        AvatarColor = AvatarColors.Random()
                    };
                    student.Embedding = new PersonEmbedding();

                    // Guardamos la referencia para el commit
                    student.User = studentUser;
                }

                // Preparar tutor si no existe
                if (tutorUser == null)
                {
                    newTutorCreated = true;
                    tutorUser = new User
                    {
                        UserName = command.TutorEmail,
                        Email = command.TutorEmail,
                        Name = command.TutorFirstName,
                        Surname = command.TutorLastName,
                        IsActive = true,
                        CreatedAt = _dateTime.UtcNow,
                        EmailConfirmed = true,
                        LockoutEnabled = true,
                        MustChangePassword = true
                    };

                    tutor = new FamilyRepresentative
                    {
                        FirstName = command.TutorFirstName,
                        LastName = command.TutorLastName,
                        DocumentNumber = command.TutorDocumentNumber,
                        Phone = command.TutorPhone,
                        Relationship = command.TutorRelationship
                    };
                }

                // 4. Ejecutar todo en una sola transacción
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (newTutorCreated)
                    {
                        var (tutorSucceeded, tutorErrors) = await _identityService.CreateUserAsync(tutorUser, tutorPassword);
                        if (!tutorSucceeded)
                        {
                            throw new InvalidOperationException($"Error al crear el usuario del tutor: {string.Join(", ", tutorErrors)}");
                        }
                        await _identityService.AddToRoleAsync(tutorUser, RoleNames.FamilyRepresentative);
                        tutor!.UserId = tutorUser.Id;
                        if (tutor.Id == Guid.Empty) tutor.Id = Guid.NewGuid();
                        await _familyRepository.CreateAsync(tutor, ct);
                    }
                    else if (tutor != null)
                    {
                        tutor.IsActive = true;
                        tutor.Status = FamilyStatusEnum.Active;
                        tutor.UpdatedAt = _dateTime.UtcNow;
                        tutorUser.IsActive = true;
                        await _identityService.UpdateUserAsync(tutorUser);
                        await _familyRepository.UpdateAsync(tutor, ct);
                    }

                    if (isReactivation)
                    {
                        // Reactivación / Reingreso de alumno existente (evita duplicar registros)
                        student.IsActive = true;
                        student.FirstName = command.FirstName;
                        student.LastName = command.LastName;
                        student.BirthDate = command.BirthDate;
                        student.UpdatedAt = _dateTime.UtcNow;
                        if (!string.IsNullOrEmpty(command.PhotoUrl))
                        {
                            student.PhotoUrl = command.PhotoUrl;
                        }

                        if (student.User != null)
                        {
                            student.User.IsActive = true;
                            student.User.Name = command.FirstName;
                            student.User.Surname = command.LastName;
                            await _identityService.UpdateUserAsync(student.User);
                        }

                        student.SupervisorUserId = tutorUser.Id;

                        // Desmarcar otros vínculos primarios para que este tutor sea el principal
                        foreach (var existingLink in student.PersonRepresentatives.Where(pr => pr.IsActive))
                        {
                            existingLink.IsPrimary = false;
                            existingLink.UpdatedAt = _dateTime.UtcNow;
                        }

                        // Vincular o reactivar vínculo con el tutor
                        var relLink = student.PersonRepresentatives.FirstOrDefault(pr => pr.RepresentativeId == tutor!.Id);
                        if (relLink != null)
                        {
                            relLink.IsActive = true;
                            relLink.IsPrimary = true;
                            relLink.Relationship = command.TutorRelationship;
                            relLink.EndedAt = null;
                            relLink.UnlinkObservation = null;
                            relLink.UpdatedAt = _dateTime.UtcNow;
                        }
                        else
                        {
                            student.PersonRepresentatives.Add(new PersonRepresentative
                            {
                                PersonId = student.Id,
                                RepresentativeId = tutor!.Id,
                                Relationship = command.TutorRelationship,
                                IsPrimary = true,
                                IsActive = true,
                                CreatedAt = _dateTime.UtcNow
                            });
                        }

                        // Actualizar o crear asignación profesional
                        foreach (var pp in student.ProfessionalPersons.Where(p => p.IsActive))
                        {
                            pp.IsPrimaryProfessional = false;
                        }

                        var profAssignment = student.ProfessionalPersons.FirstOrDefault(p => p.ProfessionalId == command.ProfessionalId);
                        if (profAssignment != null)
                        {
                            profAssignment.IsActive = true;
                            profAssignment.ClassroomId = command.ClassroomId;
                            profAssignment.IsPrimaryProfessional = true;
                            profAssignment.CanSuperviseLogin = true;
                            profAssignment.AssignedAt = _dateTime.UtcNow;
                        }
                        else
                        {
                            student.ProfessionalPersons.Add(new ProfessionalPerson
                            {
                                ProfessionalId = command.ProfessionalId,
                                PersonId = student.Id,
                                ClassroomId = command.ClassroomId,
                                IsPrimaryProfessional = true,
                                CanSuperviseLogin = true,
                                IsActive = true,
                                AssignedAt = _dateTime.UtcNow
                            });
                        }

                        await _repository.UpdateAsync(student, ct);
                    }
                    else
                    {
                        // Creación de Alumno Nuevo
                        var studentPassword = PasswordGenerator.GenerateTemporary();
                        var (studentSucceeded, studentErrors) = await _identityService.CreateUserAsync(student.User, studentPassword);
                        if (!studentSucceeded)
                        {
                            throw new InvalidOperationException($"Error al crear el usuario del alumno: {string.Join(", ", studentErrors)}");
                        }
                        await _identityService.AddToRoleAsync(student.User, RoleNames.PersonWithDisability);
                        student.UserId = student.User.Id;
                        student.SupervisorUserId = tutorUser.Id;

                        if (student.Id == Guid.Empty) student.Id = Guid.NewGuid();

                        // Enlace con el tutor
                        student.PersonRepresentatives.Add(new PersonRepresentative
                        {
                            PersonId = student.Id,
                            RepresentativeId = tutor!.Id,
                            IsPrimary = true,
                            IsActive = true,
                            CreatedAt = _dateTime.UtcNow
                        });

                        // Enlace con el profesional
                        student.ProfessionalPersons.Add(new ProfessionalPerson
                        {
                            ProfessionalId = command.ProfessionalId,
                            PersonId = student.Id,
                            ClassroomId = command.ClassroomId,
                            IsPrimaryProfessional = true,
                            CanSuperviseLogin = true,
                            IsActive = true,
                            AssignedAt = _dateTime.UtcNow
                        });

                        await _repository.CreateAsync(student, ct);
                    }

                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                // Todo lo que sigue al commit es best-effort. La persistencia ya fue
                // confirmada y un fallo de infraestructura posterior no debe convertir
                // una creación exitosa en un HTTP 500. Cada paso se aísla para que un
                // fallo no impida ejecutar los restantes, y se registra para diagnóstico.
                await RunPostCommitStepAsync(
                    student.Id,
                    "roadmap initialization",
                    () => _roadmapInitializer.InitializeStudentRoadmapAsync(
                        student.Id, professional.UserId, cancellationToken));

                await RunPostCommitStepAsync(
                    student.Id,
                    "student embedding job creation",
                    () => _backgroundJobs.CreateAsync(
                        JobTypes.Embedding,
                        BuildEmbeddingPayload(student),
                        maxRetries: 3,
                        cancellationToken: cancellationToken));

                await RunPostCommitStepAsync(
                    student.Id,
                    "tutor email job creation",
                    () => _backgroundJobs.CreateAsync(
                        JobTypes.Email,
                        JsonSerializer.Serialize(new EmailPayload
                        {
                            To = command.TutorEmail,
                            Subject = "Bienvenido a InclusiON — Tu cuenta ha sido creada",
                            TemplateName = "PasswordReset",
                            Replacements = new Dictionary<string, string?>
                            {
                                { "UserName", command.TutorFirstName },
                                { "TemporaryPassword", tutorPassword },
                                { "Year", _dateTime.UtcNow.Year.ToString() }
                            }
                        }),
                        maxRetries: 2,
                        cancellationToken: cancellationToken));

                if (professional != null && professional.UserId != Guid.Empty)
                {
                    await RunPostCommitStepAsync(
                        student.Id,
                        "professional notification",
                        () => NotifyNewStudentAsync(professional.UserId, student, cancellationToken));
                }

                _logger.LogInformation(
                    isReactivation ? "Alumno {StudentId} reactivado con Tutor {TutorId} exitosamente." : "Alumno {StudentId} y Tutor {TutorId} creados exitosamente.",
                    student.Id, tutor?.Id);

                var response = PersonMapper.ToResponse(student);
                if (newTutorCreated)
                {
                    response.TutorTemporaryPassword = tutorPassword;
                }
                var successMessage = isReactivation
                    ? $"El alumno {student.FirstName} {student.LastName} (DNI {student.DocumentNumber}) fue reactivado exitosamente junto a su tutor legal."
                    : "Alumno y tutor registrados exitosamente.";
                return ApiResponse<PersonResponse>.SuccessResult(response, successMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar alumno con tutor");
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.InternalError, $"Error en el servidor: {ex.Message}");
            }
        }

        private async Task RunPostCommitStepAsync(Guid studentId, string stepName, Func<Task> step)
        {
            try
            {
                await step();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Alumno {StudentId} persistido, pero falló el paso posterior {PostCommitStep}.",
                    studentId, stepName);
            }
        }

        private Task NotifyNewStudentAsync(Guid professionalUserId, PersonWithDisability person, CancellationToken cancellationToken)
        {
            var personName = $"{person.FirstName} {person.LastName}";
            return _notifier.NotifyUserAsync(
                professionalUserId.ToString(),
                "🎓 Nuevo alumno asignado",
                $"Tienes un nuevo alumno, {personName}. Llená el perfil funcional.",
                actionUrl: $"/#/pro/persons/{person.Id}",
                cancellationToken: cancellationToken);
        }

        private string GenerateUsername(string firstName, string lastName)
        {
            var cleanFirstName = RemoveDiacritics(firstName.ToLower()).Replace(" ", "");
            var cleanLastName = RemoveDiacritics(lastName.ToLower()).Replace(" ", "");

            cleanFirstName = new string(cleanFirstName.Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '.').ToArray());
            cleanLastName = new string(cleanLastName.Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '.').ToArray());

            var baseUsername = $"{cleanFirstName}.{cleanLastName}";
            var timestamp = _dateTime.UtcNow.Ticks % 10000;
            return $"{baseUsername}{timestamp}";
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        private static string BuildEmbeddingPayload(PersonWithDisability person) =>
            JsonSerializer.Serialize(new
            {
                entity_type = "person",
                entity_id = person.Id.ToString(),
                description = string.Join(" ", new[] { person.InterestsAndMotivators, person.LearningStyle }
                                  .Where(s => !string.IsNullOrWhiteSpace(s))),
                instructions = string.Join(" ", new[] { person.AdditionalTherapies, person.AvailableResources }
                                  .Where(s => !string.IsNullOrWhiteSpace(s))),
                content_json = JsonSerializer.Serialize(new
                {
                    uses_aac = person.UsesAAC,
                    uses_sign_language = person.UsesSignLanguage,
                    attention_level = person.AttentionLevel,
                    communication_level = person.CommunicationLevel,
                    motor_skill_level = person.MotorSkillLevel,
                    autonomy_level_id = person.AutonomyLevelId,
                    disability_type_id = person.DisabilityTypeId,
                }),
            });
    }
}
