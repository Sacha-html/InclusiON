using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Mappers;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class UpdatePersonCommandHandler : ICommandHandler<UpdatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePersonCommandHandler> _logger;

        public UpdatePersonCommandHandler(
            IPersonsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<UpdatePersonCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(UpdatePersonCommand command, CancellationToken cancellationToken)
        {
            var person = await _repository.GetByIdAsync(command.PersonId, cancellationToken);

            if (person == null)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            // Validar documento unico si cambio
            if (!string.IsNullOrWhiteSpace(command.DocumentNumber) && command.DocumentNumber != person.DocumentNumber)
            {
                var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, command.PersonId, cancellationToken);
                if (documentExists)
                {
                    return ApiResponse<PersonResponse>.Conflict(
                        ErrorCode.DocumentAlreadyExists,
                        ErrorMessages.DocumentAlreadyExists);
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Persona actualizada: {PersonId}", command.PersonId);

            var response = PersonMapper.ToResponse(person);
            return ApiResponse<PersonResponse>.SuccessResult(response, SuccessMessages.PersonUpdated);
        }
    }
}
