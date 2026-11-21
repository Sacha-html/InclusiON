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
    /// (nuevo o vinculado explícitamente por ExistingTutorId) y asignación de aula
    /// en una única transacción de base de datos.
    /// </summary>
    public class CreatePersonWithTutorCommandHandler : ICommandHandler<CreatePersonWithTutorCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IAssignmentsRepository _assignmentsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IIdentityService _identityService;
        private readonly IUniquenessValidationService _uniquenessValidation;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly ILogger<CreatePersonWithTutorCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IRoadmapInitializer _roadmapInitializer;
        private readonly IRealTimeNotifier _notifier;
        private readonly INotificationService? _notificationService;

        public CreatePersonWithTutorCommandHandler(
            IPersonsRepository repository,
            IFamilyRepository familyRepository,
            IAssignmentsRepository assignmentsRepository,
            IProfessionalsRepository professionalsRepository,
            IIdentityService identityService,
            IUniquenessValidationService uniquenessValidation,
            IUnitOfWork unitOfWork,
            IBackgroundJobRepository backgroundJobs,
            ILogger<CreatePersonWithTutorCommandHandler> logger,
            IDateTimeProvider dateTime,
            IRoadmapInitializer roadmapInitializer,
            IRealTimeNotifier notifier,
            INotificationService? notificationService = null)
        {
            _repository = repository;
            _familyRepository = familyRepository;
            _assignmentsRepository = assignmentsRepository;
            _professionalsRepository = professionalsRepository;
            _identityService = identityService;
            _uniquenessValidation = uniquenessValidation;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
            _logger = logger;
            _dateTime = dateTime;
            _roadmapInitializer = roadmapInitializer;
            _notifier = notifier;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(CreatePersonWithTutorCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var studentDoc = UniquenessNormalizer.NormalizeDocument(command.DocumentNumber);

                // 1. Validar documento del alumno (unicidad global, tanto activos como inactivos)
                if (studentDoc is not null)
                {
                    var studentDocCheck = await _uniquenessValidation.CheckDocumentAsync(studentDoc, null, cancellationToken);
                    if (!studentDocCheck.IsAvailable)
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            studentDocCheck.Message ?? ErrorMessages.DocumentAlreadyExists);
                    }
                }

                // 2. Resolver tutor (vinculación explícita de existente o creación de nuevo tutor)
                FamilyRepresentative? tutor = null;
                User? tutorUser = null;
                Guid supervisorUserId;
                var newTutorCreated = false;
                var tutorPassword = PasswordGenerator.GenerateTemporary();
                string? normalizedTutorEmail = null;

                if (command.ExistingTutorId.HasValue)
                {
                    if (command.ExistingTutorId.Value == Guid.Empty)
                    {
                        return ApiResponse<PersonResponse>.ErrorResult(
                            ErrorCode.ValidationFailed,
                            "Debe seleccionar un tutor existente válido.");
                    }

                    tutor = await _familyRepository.GetByIdAsync(command.ExistingTutorId.Value, cancellationToken);
                    if (tutor is null)
                    {
                        return ApiResponse<PersonResponse>.NotFound("Tutor / Familiar");
                    }

                    if (!tutor.IsActive || tutor.Status != FamilyStatusEnum.Active || (tutor.User is not null && !tutor.User.IsActive))
                    {
                        return ApiResponse<PersonResponse>.ErrorResult(
                            ErrorCode.BusinessRuleViolation,
                            "El tutor seleccionado no se encuentra activo. Reactivalo antes de vincularlo.");
                    }

                    supervisorUserId = tutor.UserId;
                }
                else
                {
                    normalizedTutorEmail = UniquenessNormalizer.NormalizeEmail(command.TutorEmail);
                    var tutorDoc = UniquenessNormalizer.NormalizeDocument(command.TutorDocumentNumber);

                    if (string.IsNullOrWhiteSpace(command.TutorFirstName) ||
                        string.IsNullOrWhiteSpace(command.TutorLastName) ||
                        normalizedTutorEmail is null)
                    {
                        return ApiResponse<PersonResponse>.ErrorResult(
                            ErrorCode.ValidationFailed,
                            "Los datos obligatorios del tutor (nombre, apellido y email) son requeridos.");
                    }

                    // El alumno y el tutor no pueden compartir el mismo DNI
                    if (studentDoc is not null && tutorDoc is not null &&
                        string.Equals(studentDoc, tutorDoc, StringComparison.OrdinalIgnoreCase))
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            "El DNI del alumno y el DNI del tutor no pueden ser iguales.");
                    }

                    // Validar unicidad global del email del tutor
                    var emailCheck = await _uniquenessValidation.CheckEmailAsync(
                        normalizedTutorEmail, cancellationToken: cancellationToken);
                    if (!emailCheck.IsAvailable)
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.EmailAlreadyExists,
                            "El email del tutor ya se encuentra registrado en el sistema. Si el tutor ya tiene cuenta, seleccioná 'Vincular tutor existente'.");
                    }

                    // Validar unicidad global del documento del tutor
                    if (tutorDoc is not null)
                    {
                        var tutorDocCheck = await _uniquenessValidation.CheckDocumentAsync(tutorDoc, null, cancellationToken);
                        if (!tutorDocCheck.IsAvailable)
                        {
                            return ApiResponse<PersonResponse>.Conflict(
                                ErrorCode.DocumentAlreadyExists,
                                tutorDocCheck.Message ?? "El documento del tutor ya se encuentra registrado.");
                        }
                    }

                    newTutorCreated = true;
                    tutorUser = new User
                    {
                        UserName = normalizedTutorEmail,
                        Email = normalizedTutorEmail,
                        Name = command.TutorFirstName.Trim(),
                        Surname = command.TutorLastName.Trim(),
                        IsActive = true,
                        CreatedAt = _dateTime.UtcNow,
                        EmailConfirmed = true,
                        LockoutEnabled = true,
                        MustChangePassword = true
                    };

                    tutor = new FamilyRepresentative
                    {
                        FirstName = command.TutorFirstName.Trim(),
                        LastName = command.TutorLastName.Trim(),
                        DocumentNumber = tutorDoc,
                        Phone = command.TutorPhone?.Trim(),
                        Relationship = command.TutorRelationship
                    };

                    supervisorUserId = tutorUser.Id;
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

                // Preparar creación de alumno nuevo
                var baseStudentUsername = GenerateUsername(command.FirstName, command.LastName);
                var studentEmail = $"{baseStudentUsername}@inclusion.local";

                var studentUser = new User
                {
                    UserName = baseStudentUsername,
                    Email = studentEmail,
                    Name = command.FirstName.Trim(),
                    Surname = command.LastName.Trim(),
                    IsActive = true,
                    CreatedAt = _dateTime.UtcNow,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                var student = new PersonWithDisability
                {
                    FirstName = command.FirstName.Trim(),
                    LastName = command.LastName.Trim(),
                    DocumentNumber = studentDoc,
                    BirthDate = command.BirthDate,
                    PhotoUrl = command.PhotoUrl,
                    AvatarColor = AvatarColors.Random(),
                    Embedding = new PersonEmbedding(),
                    User = studentUser
                };

                // 4. Ejecutar todo en una sola transacción
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (newTutorCreated)
                    {
                        var (tutorSucceeded, tutorErrors) = await _identityService.CreateUserAsync(tutorUser!, tutorPassword);
                        if (!tutorSucceeded)
                        {
                            throw new InvalidOperationException($"Error al crear el usuario del tutor: {string.Join(", ", tutorErrors)}");
                        }
                        await _identityService.AddToRoleAsync(tutorUser!, RoleNames.FamilyRepresentative);
                        tutor!.UserId = tutorUser!.Id;
                        supervisorUserId = tutorUser.Id;
                        if (tutor.Id == Guid.Empty) tutor.Id = Guid.NewGuid();
                        await _familyRepository.CreateAsync(tutor, ct);
                    }

                    var studentPassword = PasswordGenerator.GenerateTemporary();
                    var (studentSucceeded, studentErrors) = await _identityService.CreateUserAsync(student.User, studentPassword);
                    if (!studentSucceeded)
                    {
                        throw new InvalidOperationException($"Error al crear el usuario del alumno: {string.Join(", ", studentErrors)}");
                    }
                    await _identityService.AddToRoleAsync(student.User, RoleNames.PersonWithDisability);
                    student.UserId = student.User.Id;
                    student.SupervisorUserId = supervisorUserId;

                    if (student.Id == Guid.Empty) student.Id = Guid.NewGuid();

                    // Enlace con el tutor
                    student.PersonRepresentatives.Add(new PersonRepresentative
                    {
                        PersonId = student.Id,
                        RepresentativeId = tutor!.Id,
                        Relationship = command.TutorRelationship,
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
                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                // Pasos post-commit best-effort
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

                if (newTutorCreated && normalizedTutorEmail is not null)
                {
                    await RunPostCommitStepAsync(
                        student.Id,
                        "tutor email job creation",
                        () => _backgroundJobs.CreateAsync(
                            JobTypes.Email,
                            JsonSerializer.Serialize(new EmailPayload
                            {
                                To = normalizedTutorEmail,
                                Subject = "Bienvenido a InclusiON — Tu cuenta ha sido creada",
                                TemplateName = "PasswordReset",
                                Replacements = new Dictionary<string, string?>
                                {
                                    { "UserName", command.TutorFirstName?.Trim() },
                                    { "TemporaryPassword", tutorPassword },
                                    { "Year", _dateTime.UtcNow.Year.ToString() }
                                }
                            }),
                            maxRetries: 2,
                            cancellationToken: cancellationToken));
                }

                if (professional.UserId != Guid.Empty)
                {
                    await RunPostCommitStepAsync(
                        student.Id,
                        "professional and tutor notifications",
                        () => NotifyNewStudentAndTutorAsync(professional.UserId, supervisorUserId, student, cancellationToken));
                }

                _logger.LogInformation(
                    "Alumno {StudentId} creado con Tutor {TutorId} (nuevo={NewTutor}) exitosamente.",
                    student.Id, tutor?.Id, newTutorCreated);

                var response = PersonMapper.ToResponse(student);
                if (newTutorCreated)
                {
                    response.TutorTemporaryPassword = tutorPassword;
                }

                var successMessage = newTutorCreated
                    ? "Alumno y tutor registrados exitosamente."
                    : "Alumno registrado y vinculado al tutor existente exitosamente.";
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

        private async Task NotifyNewStudentAndTutorAsync(Guid professionalUserId, Guid? tutorUserId, PersonWithDisability person, CancellationToken cancellationToken)
        {
            var personName = $"{person.FirstName} {person.LastName}".Trim();

            if (_notificationService is null)
            {
                await _notifier.NotifyUserAsync(
                    professionalUserId.ToString(),
                    "🎓 Nuevo alumno asignado",
                    $"Tienes un nuevo alumno, {personName}. Llená el perfil funcional.",
                    actionUrl: $"/pro/persons/{person.Id}",
                    cancellationToken: cancellationToken);
                return;
            }

            var notifications = new List<NotificationMessage>
            {
                new(
                    UserId: professionalUserId,
                    Type: NotificationTypes.Assignment,
                    EventCode: NotificationEventCodes.StudentCreated,
                    Title: "Nuevo alumno asignado",
                    Message: $"Tenés un nuevo alumno asignado: {personName}. Completá su perfil funcional.",
                    ActionUrl: $"/pro/persons/{_notificationService.EncryptId(person.Id)}")
            };

            if (tutorUserId.HasValue && tutorUserId.Value != Guid.Empty)
            {
                notifications.Add(new NotificationMessage(
                    UserId: tutorUserId.Value,
                    Type: NotificationTypes.System,
                    EventCode: NotificationEventCodes.StudentCreated,
                    Title: "Bienvenido a InclusiON",
                    Message: $"Tu cuenta fue vinculada como representante de {personName}. Ya podés consultar su progreso, actividades y calendario.",
                    ActionUrl: "/family"));
            }

            await _notificationService.NotifyManyAsync(notifications, cancellationToken);
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
