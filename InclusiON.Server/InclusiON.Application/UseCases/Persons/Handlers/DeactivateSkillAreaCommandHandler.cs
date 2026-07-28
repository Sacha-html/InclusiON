using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class DeactivateSkillAreaCommandHandler
        : ICommandHandler<DeactivateSkillAreaCommand, ApiResponse<PersonSkillProfileResponse>>
    {
        private readonly IPersonsRepository _persons;
        private readonly IUnitOfWork        _uow;

        public DeactivateSkillAreaCommandHandler(IPersonsRepository persons, IUnitOfWork uow)
        {
            _persons = persons;
            _uow     = uow;
        }

        public async Task<ApiResponse<PersonSkillProfileResponse>> HandleAsync(
            DeactivateSkillAreaCommand command, CancellationToken cancellationToken)
        {
            var profile = await _persons.GetSkillProfileEntryAsync(
                command.PersonId, command.SkillAreaId, cancellationToken);

            if (profile is null)
                return ApiResponse<PersonSkillProfileResponse>.NotFound("Perfil de habilidad");

            profile.IsActive = false;
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<PersonSkillProfileResponse>.SuccessResult(
                new PersonSkillProfileResponse
                {
                    SkillAreaId   = profile.SkillAreaId,
                    SkillAreaName = profile.SkillArea.Name,
                    Color         = profile.SkillArea.Color,
                    Icon          = profile.SkillArea.Icon,
                    IsActive      = profile.IsActive,
                    AssignedAt    = profile.AssignedAt
                },
                "Area de habilidad desactivada exitosamente.");
        }
    }
}
