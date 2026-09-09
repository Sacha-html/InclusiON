using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Mappers;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
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
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly ILogger<UpdatePersonCommandHandler> _logger;

        public UpdatePersonCommandHandler(
            IPersonsRepository repository,
            IUnitOfWork unitOfWork,
            IBackgroundJobRepository backgroundJobs,
            ILogger<UpdatePersonCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
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
            if (command.PhotoUrl != null) person.PhotoUrl = command.PhotoUrl;

            await _repository.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _backgroundJobs.CreateAsync(
                JobTypes.Embedding,
                BuildEmbeddingPayload(person),
                maxRetries: 3,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Persona actualizada: {PersonId}", command.PersonId);

            var response = PersonMapper.ToResponse(person);
            return ApiResponse<PersonResponse>.SuccessResult(response, SuccessMessages.PersonUpdated);
        }

        private static string BuildEmbeddingPayload(PersonWithDisability person) =>
            JsonSerializer.Serialize(new
            {
                entity_type  = "person",
                entity_id    = person.Id.ToString(),
                description  = string.Join(" ", new[] { person.InterestsAndMotivators, person.LearningStyle }
                                   .Where(s => !string.IsNullOrWhiteSpace(s))),
                instructions = string.Join(" ", new[] { person.AdditionalTherapies, person.AvailableResources }
                                   .Where(s => !string.IsNullOrWhiteSpace(s))),
                content_json = JsonSerializer.Serialize(new
                {
                    uses_aac            = person.UsesAAC,
                    uses_sign_language  = person.UsesSignLanguage,
                    attention_level     = person.AttentionLevel,
                    communication_level = person.CommunicationLevel,
                    motor_skill_level   = person.MotorSkillLevel,
                    autonomy_level_id   = person.AutonomyLevelId,
                    disability_type_id  = person.DisabilityTypeId,
                }),
            });
    }
}
