using System.Text.Json;
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
    public class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPinHasher _pinHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly ILogger<CreatePersonCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        private readonly IRoadmapInitializer _roadmapInitializer;

        public CreatePersonCommandHandler(
            IPersonsRepository repository,
            IIdentityService identityService,
            IPasswordHasher passwordHasher,
            IPinHasher pinHasher,
            IUnitOfWork unitOfWork,
            IBackgroundJobRepository backgroundJobs,
            ILogger<CreatePersonCommandHandler> logger,
            IDateTimeProvider dateTime,
            IRoadmapInitializer roadmapInitializer)
        {
            _repository = repository;
            _identityService = identityService;
            _passwordHasher = passwordHasher;
            _pinHasher = pinHasher;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
            _logger = logger;
            _dateTime = dateTime;
            _roadmapInitializer = roadmapInitializer;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(CreatePersonCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validar documento unico
                if (!string.IsNullOrWhiteSpace(command.DocumentNumber))
                {
                    var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, null, cancellationToken);
                    if (documentExists)
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            ErrorMessages.DocumentAlreadyExists);
                    }
                }

                // Generar email y username unicos basados en nombre
                var baseUsername = GenerateUsername(command.FirstName, command.LastName);
                var email = $"{baseUsername}@inclusion.local";
                var password = PasswordGenerator.GenerateTemporary();

                // Crear usuario
                var user = new User
                {
                    UserName = baseUsername,
                    Email = email,
                    Name = command.FirstName,
                    Surname = command.LastName,
                    IsActive = true,
                    CreatedAt = _dateTime.UtcNow,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                // Crear persona
                var person = new PersonWithDisability
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    DocumentNumber = command.DocumentNumber,
                    BirthDate = command.BirthDate,
                    DisabilityTypeId = command.DisabilityTypeId,
                    PhotoUrl = command.PhotoUrl,
                    // Perfil funcional
                    AttentionLevel = command.AttentionLevel,
                    CommunicationLevel = command.CommunicationLevel,
                    UsesAAC = command.UsesAAC,
                    UsesSignLanguage = command.UsesSignLanguage,
                    MotorSkillLevel = command.MotorSkillLevel,
                    // Preferencias
                    InterestsAndMotivators = command.InterestsAndMotivators,
                    LearningStyle = command.LearningStyle,
                    AvailableResources = command.AvailableResources,
                    AdditionalTherapies = command.AdditionalTherapies,
                    // Accesibilidad
                    RequiresLargeFont = command.RequiresLargeFont,
                    RequiresHighContrast = command.RequiresHighContrast,
                    VisualNoiseSensitivity = command.VisualNoiseSensitivity,
                    SoundSensitivity = command.SoundSensitivity,
                    ColorBlindnessType = command.ColorBlindnessType,
                    // Configuracion de acceso
                    AutonomyLevelId = command.AutonomyLevelId,
                    LoginMethodId = command.LoginMethodId,
                    SupervisorUserId = command.SupervisorUserId,
                    AvatarColor = command.AvatarColor ?? AvatarColors.Random()
                };

                // Hash del PIN si se proporciona
                if (!string.IsNullOrWhiteSpace(command.Pin))
                {
                    person.PinCodeHash = _pinHasher.Hash(command.Pin);
                }

                person.Embedding = new PersonEmbedding();

                // Crear usuario, asignar rol y persona en transaccion
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var (succeeded, errors) = await _identityService.CreateUserAsync(user, password);
                    if (!succeeded)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.UserCreationError, string.Join(", ", errors)));
                    }

                    await _identityService.AddToRoleAsync(user, RoleNames.PersonWithDisability);

                    person.UserId = user.Id;
                    await _repository.CreateAsync(person, ct);
                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                // Inicializar Roadmap Estándar de 10 Niveles
                await _roadmapInitializer.InitializeStudentRoadmapAsync(person.Id, person.SupervisorUserId, cancellationToken);

                await _backgroundJobs.CreateAsync(
                    JobTypes.Embedding,
                    BuildEmbeddingPayload(person, command),
                    maxRetries: 3,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Persona creada: {PersonId}, Usuario: {UserId}", person.Id, user.Id);

                var response = PersonMapper.ToResponse(person);
                return ApiResponse<PersonResponse>.SuccessResult(response, SuccessMessages.PersonCreated);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith(ErrorMessages.UserCreationError.Replace("{0}", "")))
            {
                _logger.LogWarning(ex, "Error de validacion al crear persona");
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    "No se pudo crear el usuario. Verificá que los datos ingresados sean válidos.");
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

        private static string BuildEmbeddingPayload(PersonWithDisability person, CreatePersonCommand command) =>
            JsonSerializer.Serialize(new
            {
                entity_type = "person",
                entity_id   = person.Id.ToString(),
                description = string.Join(" ", new[] { command.InterestsAndMotivators, command.LearningStyle }
                                  .Where(s => !string.IsNullOrWhiteSpace(s))),
                instructions = string.Join(" ", new[] { command.AdditionalTherapies, command.AvailableResources }
                                  .Where(s => !string.IsNullOrWhiteSpace(s))),
                content_json = JsonSerializer.Serialize(new
                {
                    uses_aac             = command.UsesAAC,
                    uses_sign_language   = command.UsesSignLanguage,
                    attention_level      = command.AttentionLevel,
                    communication_level  = command.CommunicationLevel,
                    motor_skill_level    = command.MotorSkillLevel,
                    autonomy_level_id    = command.AutonomyLevelId,
                    disability_type_id   = command.DisabilityTypeId,
                }),
            });
    }
}
