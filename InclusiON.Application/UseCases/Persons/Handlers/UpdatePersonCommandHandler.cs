using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class UpdatePersonCommandHandler : ICommandHandler<UpdatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly ILogger<UpdatePersonCommandHandler> _logger;

        public UpdatePersonCommandHandler(
            IPersonsRepository repository,
            ILogger<UpdatePersonCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(UpdatePersonCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var person = await _repository.GetByIdAsync(command.PersonId, cancellationToken);

                if (person == null)
                {
                    return ApiResponse<PersonResponse>.ErrorResult(
                        ErrorCode.PersonNotFound,
                        "Persona no encontrada");
                }

                // Validar documento unico si cambio
                if (!string.IsNullOrWhiteSpace(command.DocumentNumber) && command.DocumentNumber != person.DocumentNumber)
                {
                    var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, command.PersonId, cancellationToken);
                    if (documentExists)
                    {
                        return ApiResponse<PersonResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            "Ya existe una persona con este numero de documento");
                    }
                }

                // Actualizar campos solo si se proporcionan
                if (command.FirstName != null) person.FirstName = command.FirstName;
                if (command.LastName != null) person.LastName = command.LastName;
                if (command.DocumentNumber != null) person.DocumentNumber = command.DocumentNumber;
                if (command.BirthDate.HasValue) person.BirthDate = command.BirthDate.Value;
                if (command.DisabilityTypeId.HasValue) person.DisabilityTypeId = command.DisabilityTypeId;
                if (command.PhotoUrl != null) person.PhotoUrl = command.PhotoUrl;

                // Perfil funcional
                if (command.AttentionLevel.HasValue) person.AttentionLevel = command.AttentionLevel;
                if (command.CommunicationLevel.HasValue) person.CommunicationLevel = command.CommunicationLevel;
                if (command.UsesAAC.HasValue) person.UsesAAC = command.UsesAAC.Value;
                if (command.UsesSignLanguage.HasValue) person.UsesSignLanguage = command.UsesSignLanguage.Value;
                if (command.MotorSkillLevel.HasValue) person.MotorSkillLevel = command.MotorSkillLevel;

                // Preferencias
                if (command.InterestsAndMotivators != null) person.InterestsAndMotivators = command.InterestsAndMotivators;
                if (command.LearningStyle != null) person.LearningStyle = command.LearningStyle;
                if (command.AvailableResources != null) person.AvailableResources = command.AvailableResources;
                if (command.AdditionalTherapies != null) person.AdditionalTherapies = command.AdditionalTherapies;

                // Accesibilidad
                if (command.RequiresLargeFont.HasValue) person.RequiresLargeFont = command.RequiresLargeFont.Value;
                if (command.RequiresHighContrast.HasValue) person.RequiresHighContrast = command.RequiresHighContrast.Value;
                if (command.VisualNoiseSensitivity.HasValue) person.VisualNoiseSensitivity = command.VisualNoiseSensitivity.Value;
                if (command.SoundSensitivity.HasValue) person.SoundSensitivity = command.SoundSensitivity.Value;

                // Configuracion de acceso
                if (command.AutonomyLevelId.HasValue) person.AutonomyLevelId = command.AutonomyLevelId;
                if (command.SupervisorUserId.HasValue) person.SupervisorUserId = command.SupervisorUserId;
                if (command.AvatarColor != null) person.AvatarColor = command.AvatarColor;

                await _repository.UpdateAsync(person, cancellationToken);

                // Recargar con relaciones
                person = await _repository.GetByIdAsync(command.PersonId, cancellationToken);

                _logger.LogInformation("Persona actualizada: {PersonId}", command.PersonId);

                var response = new PersonResponse
                {
                    Id = person!.Id,
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
                    AutonomyLevelName = person.AutonomyLevel?.Name,
                    LoginMethodId = person.LoginMethodId,
                    LoginMethodName = person.LoginMethod?.Name,
                    HasPinConfigured = !string.IsNullOrEmpty(person.PinCodeHash),
                    SupervisorUserId = person.SupervisorUserId,
                    SupervisorName = person.SupervisorUser != null
                        ? $"{person.SupervisorUser.Name} {person.SupervisorUser.Surname}".Trim()
                        : null,
                    AvatarColor = person.AvatarColor,
                    DisabilityTypeId = person.DisabilityTypeId,
                    DisabilityTypeName = person.DisabilityType?.Name,
                    IsActive = person.User?.IsActive ?? false,
                    CreatedAt = person.CreatedAt,
                    UpdatedAt = person.UpdatedAt
                };

                return ApiResponse<PersonResponse>.SuccessResult(response, "Persona actualizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar persona: {PersonId}", command.PersonId);
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al actualizar persona");
            }
        }
    }
}
