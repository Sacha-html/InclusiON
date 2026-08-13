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
        private readonly IIdentityService _identityService;
        private readonly IPinHasher _pinHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly ILogger<CreatePersonWithTutorCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IRoadmapInitializer _roadmapInitializer;

        public CreatePersonWithTutorCommandHandler(
            IPersonsRepository repository,
            IFamilyRepository familyRepository,
            IAssignmentsRepository assignmentsRepository,
            IIdentityService identityService,
            IPinHasher pinHasher,
            IUnitOfWork unitOfWork,
            IBackgroundJobRepository backgroundJobs,
            ILogger<CreatePersonWithTutorCommandHandler> logger,
            IDateTimeProvider dateTime,
            IRoadmapInitializer roadmapInitializer)
        {
            _repository = repository;
            _familyRepository = familyRepository;
            _assignmentsRepository = assignmentsRepository;
            _identityService = identityService;
            _pinHasher = pinHasher;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
            _logger = logger;
            _dateTime = dateTime;
            _roadmapInitializer = roadmapInitializer;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(CreatePersonWithTutorCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validar documento de alumno único si se proporciona
                if (!string.IsNullOrWhiteSpace(command.DocumentNumber))
                {
                    var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, null, cancellationToken);
                    if (documentExists)
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            "El documento del alumno ya se encuentra registrado.");
                    }
                }

                // 2. Validar documento de tutor único si se proporciona
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

                // 3. Validar email del tutor único
                var existingTutorUser = await _identityService.FindByEmailAsync(command.TutorEmail);
                if (existingTutorUser != null)
                {
                    return ApiResponse<PersonResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        "El email del tutor ya se encuentra registrado.");
                }

                // 4. Validar y verificar que el Aula exista y obtener el ProfessionalId
                if (!command.ClassroomId.HasValue)
                {
                    return ApiResponse<PersonResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "Debe seleccionar un profesional y aula para el alumno.");
                }

                var classroom = await _assignmentsRepository.GetClassroomByIdAsync(command.ClassroomId.Value, cancellationToken);
                if (classroom == null)
                {
                    return ApiResponse<PersonResponse>.ErrorResult(
                        ErrorCode.NotFound,
                        "El aula especificada no existe.");
                }
                Guid professionalId = classroom.ProfessionalId;

                // 5. Preparar creación del alumno
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

                var student = new PersonWithDisability
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    DocumentNumber = command.DocumentNumber,
                    BirthDate = command.BirthDate,
                    DisabilityTypeId = command.DisabilityTypeId ?? 1,
                    PhotoUrl = command.PhotoUrl,
                    AttentionLevel = command.AttentionLevel,
                    CommunicationLevel = command.CommunicationLevel,
                    UsesAAC = command.UsesAAC,
                    UsesSignLanguage = command.UsesSignLanguage,
                    MotorSkillLevel = command.MotorSkillLevel,
                    InterestsAndMotivators = command.InterestsAndMotivators,
                    LearningStyle = command.LearningStyle,
                    AvailableResources = command.AvailableResources,
                    AdditionalTherapies = command.AdditionalTherapies,
                    RequiresLargeFont = command.RequiresLargeFont,
                    RequiresHighContrast = command.RequiresHighContrast,
                    VisualNoiseSensitivity = command.VisualNoiseSensitivity,
                    SoundSensitivity = command.SoundSensitivity,
                    ColorBlindnessType = command.ColorBlindnessType,
                    AutonomyLevelId = command.AutonomyLevelId,
                    LoginMethodId = command.LoginMethodId,
                    AvatarColor = command.AvatarColor ?? AvatarColors.Random()
                };

                if (!string.IsNullOrWhiteSpace(command.Pin))
                {
                    student.PinCodeHash = _pinHasher.Hash(command.Pin);
                }
                student.Embedding = new PersonEmbedding();

                // 6. Preparar creación del tutor
                var tutorPassword = PasswordGenerator.GenerateTemporary();

                var tutorUser = new User
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

                var tutor = new FamilyRepresentative
                {
                    FirstName = command.TutorFirstName,
                    LastName = command.TutorLastName,
                    DocumentNumber = command.TutorDocumentNumber,
                    Phone = command.TutorPhone,
                    Relationship = command.TutorRelationship
                };

                // 7. Ejecutar todo en una sola transacción
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    // A. Crear usuario tutor
                    var (tutorSucceeded, tutorErrors) = await _identityService.CreateUserAsync(tutorUser, tutorPassword);
                    if (!tutorSucceeded)
                    {
                        throw new InvalidOperationException($"Error al crear el usuario del tutor: {string.Join(", ", tutorErrors)}");
                    }
                    await _identityService.AddToRoleAsync(tutorUser, RoleNames.FamilyRepresentative);
                    tutor.UserId = tutorUser.Id;

                    // B. Crear usuario alumno
                    var (studentSucceeded, studentErrors) = await _identityService.CreateUserAsync(studentUser, studentPassword);
                    if (!studentSucceeded)
                    {
                        throw new InvalidOperationException($"Error al crear el usuario del alumno: {string.Join(", ", studentErrors)}");
                    }
                    await _identityService.AddToRoleAsync(studentUser, RoleNames.PersonWithDisability);
                    student.UserId = studentUser.Id;

                    // El tutor es el supervisor del alumno para el login asistido/PIN
                    student.SupervisorUserId = tutorUser.Id;

                    if (student.Id == Guid.Empty) student.Id = Guid.NewGuid();
                    if (tutor.Id == Guid.Empty) tutor.Id = Guid.NewGuid();

                    // C. Crear enlace de parentesco (PersonRepresentative)
                    var relationshipLink = new PersonRepresentative
                    {
                        PersonId = student.Id,
                        RepresentativeId = tutor.Id,
                        IsPrimary = true,
                        IsActive = true,
                        CreatedAt = _dateTime.UtcNow
                    };
                    student.PersonRepresentatives.Add(relationshipLink);

                    // D. Asignar Aula obligatoria
                    var assignment = new ProfessionalPerson
                    {
                        ProfessionalId = professionalId,
                        PersonId = student.Id,
                        ClassroomId = command.ClassroomId.Value,
                        IsPrimaryProfessional = true,
                        CanSuperviseLogin = true,
                        IsActive = true,
                        AssignedAt = _dateTime.UtcNow
                    };
                    student.ProfessionalPersons.Add(assignment);

                    // E. Guardar perfiles y relaciones en base de datos en un solo SaveChangesAsync
                    await _repository.CreateAsync(student, ct);
                    await _familyRepository.CreateAsync(tutor, ct);

                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                // 8. Inicializar Roadmap Estándar
                await _roadmapInitializer.InitializeStudentRoadmapAsync(student.Id, student.SupervisorUserId, cancellationToken);

                // 9. Crear Job de Embeddings para el Alumno
                await _backgroundJobs.CreateAsync(
                    JobTypes.Embedding,
                    BuildEmbeddingPayload(student, command),
                    maxRetries: 3,
                    cancellationToken: cancellationToken);

                // 10. Crear Job de Envío de Email con contraseña temporal para el Tutor
                await _backgroundJobs.CreateAsync(
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
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Alumno {StudentId} y Tutor {TutorId} creados exitosamente en transacción.", student.Id, tutor.Id);

                var response = PersonMapper.ToResponse(student);
                response.TutorTemporaryPassword = tutorPassword;
                return ApiResponse<PersonResponse>.SuccessResult(response, "Alumno y tutor registrados exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar alumno con tutor");
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.InternalError, $"Error en el servidor: {ex.Message}");
            }
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

        private static string BuildEmbeddingPayload(PersonWithDisability person, CreatePersonWithTutorCommand command) =>
            JsonSerializer.Serialize(new
            {
                entity_type = "person",
                entity_id = person.Id.ToString(),
                description = string.Join(" ", new[] { command.InterestsAndMotivators, command.LearningStyle }
                                  .Where(s => !string.IsNullOrWhiteSpace(s))),
                instructions = string.Join(" ", new[] { command.AdditionalTherapies, command.AvailableResources }
                                  .Where(s => !string.IsNullOrWhiteSpace(s))),
                content_json = JsonSerializer.Serialize(new
                {
                    uses_aac = command.UsesAAC,
                    uses_sign_language = command.UsesSignLanguage,
                    attention_level = command.AttentionLevel,
                    communication_level = command.CommunicationLevel,
                    motor_skill_level = command.MotorSkillLevel,
                    autonomy_level_id = command.AutonomyLevelId,
                    disability_type_id = command.DisabilityTypeId,
                }),
            });
    }
}
