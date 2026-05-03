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
        private readonly ILogger<CreatePersonCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public CreatePersonCommandHandler(
            IPersonsRepository repository,
            IIdentityService identityService,
            IPasswordHasher passwordHasher,
            IPinHasher pinHasher,
            IUnitOfWork unitOfWork,
            ILogger<CreatePersonCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _identityService = identityService;
            _passwordHasher = passwordHasher;
            _pinHasher = pinHasher;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
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
            var baseUsername = $"{firstName.ToLower().Replace(" ", "")}.{lastName.ToLower().Replace(" ", "")}";
            var timestamp = _dateTime.UtcNow.Ticks % 10000;
            return $"{baseUsername}{timestamp}";
        }

    }
}
