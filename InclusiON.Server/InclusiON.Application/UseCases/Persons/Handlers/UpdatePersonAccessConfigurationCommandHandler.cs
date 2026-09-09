using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Constants;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class UpdatePersonAccessConfigurationCommandHandler
        : ICommandHandler<UpdatePersonAccessConfigurationCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _persons;
        private readonly IVisualLoginRepository _visualLogin;
        private readonly IReadOnlyRepository<AutonomyLevel> _autonomyLevels;
        private readonly IPinHasher _pinHasher;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePersonAccessConfigurationCommandHandler(
            IPersonsRepository persons, IVisualLoginRepository visualLogin,
            IReadOnlyRepository<AutonomyLevel> autonomyLevels, IPinHasher pinHasher,
            IUnitOfWork unitOfWork)
        {
            _persons = persons;
            _visualLogin = visualLogin;
            _autonomyLevels = autonomyLevels;
            _pinHasher = pinHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(
            UpdatePersonAccessConfigurationCommand command, CancellationToken cancellationToken)
        {
            var person = await _persons.GetByIdAsync(command.PersonId, cancellationToken);
            if (person is null) return ApiResponse<PersonResponse>.NotFound("Persona");

            var autonomy = await _autonomyLevels.GetByIdAsync(command.AutonomyLevelId, cancellationToken);
            if (autonomy is null || !autonomy.IsActive)
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.ResourceNotFound, "El nivel de autonomía no está disponible.");

            var loginMethod = await _visualLogin.GetLoginMethodByIdAsync(command.LoginMethodId, cancellationToken);
            if (loginMethod is null)
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.ResourceNotFound, ErrorMessages.LoginMethodNotFound);
            if (!loginMethod.IsActive)
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.LoginMethodNotAllowed, ErrorMessages.LoginMethodNotAvailable);
            if (loginMethod.Id == 1)
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.LoginMethodNotAllowed,
                    "El método de inicio de sesión por email y contraseña ya no está disponible para alumnos.");

            string? pinHash = person.LoginMethodId == command.LoginMethodId ? person.PinCodeHash : null;
            Guid? supervisorUserId = person.LoginMethodId == command.LoginMethodId ? person.SupervisorUserId : null;

            if (loginMethod.Id == 2)
            {
                if (!string.IsNullOrEmpty(command.Pin))
                {
                    if (command.Pin.Length < 4 || command.Pin.Length > 6 || !command.Pin.All(char.IsDigit))
                        return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.InvalidFormat, ErrorMessages.PinInvalidFormat);
                    pinHash = _pinHasher.Hash(command.Pin);
                }
                else if (string.IsNullOrEmpty(pinHash))
                    return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.RequiredField, ErrorMessages.PinRequiredForMethod);
                supervisorUserId = null;
            }
            else if (loginMethod.Id == 3)
            {
                var requestedSupervisor = command.SupervisorUserId ?? supervisorUserId;
                if (!requestedSupervisor.HasValue)
                    return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.RequiredField, ErrorMessages.SupervisorRequiredForAssisted);
                var supervisor = await _visualLogin.GetProfessionalByUserIdAsync(requestedSupervisor.Value, cancellationToken);
                var family = await _visualLogin.GetFamilyByUserIdAsync(requestedSupervisor.Value, cancellationToken);
                if (supervisor is null && family is null)
                    return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.SupervisorNotAuthorized, ErrorMessages.SupervisorMustBeProfessionalOrFamily);
                supervisorUserId = requestedSupervisor;
                pinHash = null;
            }
            else
            {
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.LoginMethodNotAllowed, ErrorMessages.LoginMethodNotSupported);
            }

            var avatarColor = AvatarColors.Items.FirstOrDefault(c =>
                string.Equals(c.Hex, command.AvatarColor, StringComparison.OrdinalIgnoreCase));
            if (avatarColor is null)
                return ApiResponse<PersonResponse>.ErrorResult(ErrorCode.InvalidInput, "El color de avatar no está permitido.");

            person.AutonomyLevelId = autonomy.Id;
            person.LoginMethodId = loginMethod.Id;
            person.PinCodeHash = pinHash;
            person.SupervisorUserId = supervisorUserId;
            person.AvatarColor = avatarColor.Hex;
            await _persons.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedPerson = await _persons.GetByIdAsync(command.PersonId, cancellationToken);
            return updatedPerson is null
                ? ApiResponse<PersonResponse>.NotFound("Persona")
                : ApiResponse<PersonResponse>.SuccessResult(PersonMapper.ToResponse(updatedPerson), "Configuración de acceso actualizada.");
        }
    }
}
