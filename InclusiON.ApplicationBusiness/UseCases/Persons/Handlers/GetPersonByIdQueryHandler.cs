using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Persons.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.ApplicationBusiness.UseCases.Persons.Handlers
{
    public class GetPersonByIdQueryHandler : IQueryHandler<GetPersonByIdQuery, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly ILogger<GetPersonByIdQueryHandler> _logger;

        public GetPersonByIdQueryHandler(
            IPersonsRepository repository,
            ILogger<GetPersonByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(GetPersonByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var person = await _repository.GetByIdAsync(query.PersonId, cancellationToken);

                if (person == null)
                {
                    return ApiResponse<PersonResponse>.ErrorResult(
                        ErrorCode.PersonNotFound,
                        "Persona no encontrada");
                }

                var response = MapToResponse(person);
                return ApiResponse<PersonResponse>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener persona: {PersonId}", query.PersonId);
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al obtener persona");
            }
        }

        private static PersonResponse MapToResponse(Entities.Models.PersonWithDisability person)
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
                // Perfil funcional
                AttentionLevel = person.AttentionLevel,
                CommunicationLevel = person.CommunicationLevel,
                UsesAAC = person.UsesAAC,
                UsesSignLanguage = person.UsesSignLanguage,
                MotorSkillLevel = person.MotorSkillLevel,
                // Preferencias
                InterestsAndMotivators = person.InterestsAndMotivators,
                LearningStyle = person.LearningStyle,
                AvailableResources = person.AvailableResources,
                AdditionalTherapies = person.AdditionalTherapies,
                // Accesibilidad
                RequiresLargeFont = person.RequiresLargeFont,
                RequiresHighContrast = person.RequiresHighContrast,
                VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                SoundSensitivity = person.SoundSensitivity,
                // Configuracion de acceso
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
                // Tipo de discapacidad
                DisabilityTypeId = person.DisabilityTypeId,
                DisabilityTypeName = person.DisabilityType?.Name,
                // Estado
                IsActive = person.User?.IsActive ?? false,
                CreatedAt = person.CreatedAt,
                UpdatedAt = person.UpdatedAt
            };
        }
    }
}
