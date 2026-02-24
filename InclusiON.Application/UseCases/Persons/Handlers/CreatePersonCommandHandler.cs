using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreatePersonCommandHandler> _logger;

        public CreatePersonCommandHandler(
            IPersonsRepository repository,
            IIdentityService identityService,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            ILogger<CreatePersonCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                            "Ya existe una persona con este numero de documento");
                    }
                }

                // Generar email y username unicos basados en nombre
                var baseUsername = GenerateUsername(command.FirstName, command.LastName);
                var email = $"{baseUsername}@inclusion.local";
                var password = GenerateDefaultPassword();

                // Crear usuario
                var user = new User
                {
                    UserName = baseUsername,
                    Email = email,
                    Name = command.FirstName,
                    Surname = command.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
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
                    // Configuracion de acceso
                    AutonomyLevelId = command.AutonomyLevelId,
                    LoginMethodId = command.LoginMethodId,
                    SupervisorUserId = command.SupervisorUserId,
                    AvatarColor = command.AvatarColor ?? GenerateRandomColor()
                };

                // Hash del PIN si se proporciona
                if (!string.IsNullOrWhiteSpace(command.Pin))
                {
                    person.PinCodeHash = _passwordHasher.HashPassword(command.Pin);
                }

                // Crear usuario, asignar rol y persona en transaccion
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var (succeeded, errors) = await _identityService.CreateUserAsync(user, password);
                    if (!succeeded)
                    {
                        throw new InvalidOperationException($"Error al crear usuario: {string.Join(", ", errors)}");
                    }

                    await _identityService.AddToRoleAsync(user, "Person");

                    person.UserId = user.Id;
                    await _repository.CreateAsync(person, ct);
                }, cancellationToken);

                _logger.LogInformation("Persona creada: {PersonId}, Usuario: {UserId}", person.Id, user.Id);

                var response = MapToResponse(person);
                return ApiResponse<PersonResponse>.SuccessResult(response, "Persona creada exitosamente");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Error al crear usuario"))
            {
                _logger.LogWarning(ex, "Error de validacion al crear persona");
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear persona: {FirstName} {LastName}", command.FirstName, command.LastName);
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al crear persona");
            }
        }

        private static PersonResponse MapToResponse(PersonWithDisability person)
        {
            return new PersonResponse
            {
                Id = person.Id,
                UserId = person.UserId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                DocumentNumber = person.DocumentNumber,
                BirthDate = person.BirthDate,
                PhotoUrl = person.PhotoUrl,
                AttentionLevel = person.AttentionLevel,
                CommunicationLevel = person.CommunicationLevel,
                UsesAAC = person.UsesAAC,
                UsesSignLanguage = person.UsesSignLanguage,
                MotorSkillLevel = person.MotorSkillLevel,
                InterestsAndMotivators = person.InterestsAndMotivators,
                LearningStyle = person.LearningStyle,
                AvailableResources = person.AvailableResources,
                AdditionalTherapies = person.AdditionalTherapies,
                RequiresLargeFont = person.RequiresLargeFont,
                RequiresHighContrast = person.RequiresHighContrast,
                VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                SoundSensitivity = person.SoundSensitivity,
                AutonomyLevelId = person.AutonomyLevelId,
                LoginMethodId = person.LoginMethodId,
                HasPinConfigured = !string.IsNullOrEmpty(person.PinCodeHash),
                SupervisorUserId = person.SupervisorUserId,
                AvatarColor = person.AvatarColor,
                DisabilityTypeId = person.DisabilityTypeId,
                IsActive = true,
                CreatedAt = person.CreatedAt
            };
        }

        private static string GenerateUsername(string firstName, string lastName)
        {
            var baseUsername = $"{firstName.ToLower().Replace(" ", "")}.{lastName.ToLower().Replace(" ", "")}";
            var timestamp = DateTime.UtcNow.Ticks % 10000;
            return $"{baseUsername}{timestamp}";
        }

        private static string GenerateDefaultPassword()
        {
            return $"Temp@{Guid.NewGuid().ToString()[..8]}";
        }

        private static string GenerateRandomColor()
        {
            var colors = new[]
            {
                "#2196F3", "#4CAF50", "#FF9800", "#9C27B0",
                "#F44336", "#00BCD4", "#795548", "#607D8B"
            };
            var random = new Random();
            return colors[random.Next(colors.Length)];
        }
    }
}
