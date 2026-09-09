using System.Text.Json;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class UpdatePersonFunctionalProfileCommandHandler
        : ICommandHandler<UpdatePersonFunctionalProfileCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _persons;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobRepository _backgroundJobs;

        public UpdatePersonFunctionalProfileCommandHandler(
            IPersonsRepository persons,
            IUnitOfWork unitOfWork,
            IBackgroundJobRepository backgroundJobs)
        {
            _persons = persons;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(
            UpdatePersonFunctionalProfileCommand command, CancellationToken cancellationToken)
        {
            var person = await _persons.GetByIdAsync(command.PersonId, cancellationToken);
            if (person is null)
                return ApiResponse<PersonResponse>.NotFound("Persona");

            person.AttentionLevel = command.AttentionLevel;
            person.CommunicationLevel = command.CommunicationLevel;
            person.UsesAAC = command.UsesAAC;
            person.UsesSignLanguage = command.UsesSignLanguage;
            person.MotorSkillLevel = command.MotorSkillLevel;
            person.InterestsAndMotivators = command.InterestsAndMotivators;
            person.LearningStyle = command.LearningStyle;
            person.AvailableResources = command.AvailableResources;
            person.AdditionalTherapies = command.AdditionalTherapies;
            person.RequiresLargeFont = command.RequiresLargeFont;
            person.RequiresHighContrast = command.RequiresHighContrast;
            person.VisualNoiseSensitivity = command.VisualNoiseSensitivity;
            person.SoundSensitivity = command.SoundSensitivity;
            person.ColorBlindnessType = string.IsNullOrWhiteSpace(command.ColorBlindnessType)
                ? null
                : command.ColorBlindnessType;

            await _persons.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _backgroundJobs.CreateAsync(
                JobTypes.Embedding,
                BuildEmbeddingPayload(person),
                maxRetries: 3,
                cancellationToken: cancellationToken);

            return ApiResponse<PersonResponse>.SuccessResult(
                PersonMapper.ToResponse(person), "Perfil funcional actualizado.");
        }

        private static string BuildEmbeddingPayload(PersonWithDisability person) => JsonSerializer.Serialize(new
        {
            entity_type = "person",
            entity_id = person.Id.ToString(),
            description = string.Join(" ", new[] { person.InterestsAndMotivators, person.LearningStyle }.Where(s => !string.IsNullOrWhiteSpace(s))),
            instructions = string.Join(" ", new[] { person.AdditionalTherapies, person.AvailableResources }.Where(s => !string.IsNullOrWhiteSpace(s))),
        });
    }
}
