using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class UpdatePersonAccessibilityCommandHandler
        : ICommandHandler<UpdatePersonAccessibilityCommand, ApiResponse<PersonAccessibilityResponse>>
    {
        private readonly IPersonsRepository _persons;
        private readonly IUnitOfWork        _uow;

        public UpdatePersonAccessibilityCommandHandler(IPersonsRepository persons, IUnitOfWork uow)
        {
            _persons = persons;
            _uow     = uow;
        }

        public async Task<ApiResponse<PersonAccessibilityResponse>> HandleAsync(
            UpdatePersonAccessibilityCommand command, CancellationToken cancellationToken)
        {
            var person = await _persons.GetByIdAsync(command.PersonId, cancellationToken);
            if (person is null)
                return ApiResponse<PersonAccessibilityResponse>.NotFound("Persona");

            person.RequiresLargeFont      = command.RequiresLargeFont;
            person.RequiresHighContrast   = command.RequiresHighContrast;
            person.VisualNoiseSensitivity = command.VisualNoiseSensitivity;
            person.SoundSensitivity       = command.SoundSensitivity;
            person.ColorBlindnessType     = string.IsNullOrEmpty(command.ColorBlindnessType)
                                                ? null
                                                : command.ColorBlindnessType;

            await _persons.UpdateAsync(person, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = new PersonAccessibilityResponse
            {
                RequiresLargeFont      = person.RequiresLargeFont,
                RequiresHighContrast   = person.RequiresHighContrast,
                VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                SoundSensitivity       = person.SoundSensitivity,
                ColorBlindnessType     = person.ColorBlindnessType,
            };

            return ApiResponse<PersonAccessibilityResponse>.SuccessResult(dto,
                "Configuración de accesibilidad actualizada.");
        }
    }
}
