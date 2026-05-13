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

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class AddSkillAreaCommandHandler
        : ICommandHandler<AddSkillAreaCommand, ApiResponse<PersonSkillProfileResponse>>
    {
        private readonly IPersonsRepository           _persons;
        private readonly IReadOnlyRepository<SkillArea> _skillAreas;
        private readonly IUnitOfWork                  _uow;

        public AddSkillAreaCommandHandler(
            IPersonsRepository persons,
            IReadOnlyRepository<SkillArea> skillAreas,
            IUnitOfWork uow)
        {
            _persons    = persons;
            _skillAreas = skillAreas;
            _uow        = uow;
        }

        public async Task<ApiResponse<PersonSkillProfileResponse>> HandleAsync(
            AddSkillAreaCommand command, CancellationToken cancellationToken)
        {
            // 1. Verificar que el área de habilidad existe
            var skillArea = await _skillAreas.GetByIdAsync(command.SkillAreaId, cancellationToken);
            if (skillArea is null)
                return ApiResponse<PersonSkillProfileResponse>.NotFound("Area de habilidad");

            // 2. Verificar si ya existe una entrada para esta persona+área
            var existing = await _persons.GetSkillProfileEntryAsync(
                command.PersonId, command.SkillAreaId, cancellationToken);

            if (existing is not null)
            {
                if (existing.IsActive)
                    return ApiResponse<PersonSkillProfileResponse>.Conflict(
                        ErrorCode.Conflict,
                        "El area de habilidad ya esta asignada y activa para esta persona.");

                // Reactivar entrada existente
                existing.IsActive   = true;
                existing.AssignedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync(cancellationToken);

                return ApiResponse<PersonSkillProfileResponse>.SuccessResult(
                    PersonMapper.ToSkillProfileResponse(existing, skillArea),
                    "Area de habilidad reactivada exitosamente.");
            }

            // 3. Crear nueva entrada
            var profile = new PersonSkillProfile
            {
                PersonId   = command.PersonId,
                SkillAreaId = command.SkillAreaId,
                AssignedAt = DateTime.UtcNow,
                IsActive   = true
            };

            await _persons.AddSkillProfileEntryAsync(profile, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<PersonSkillProfileResponse>.SuccessResult(
                PersonMapper.ToSkillProfileResponse(profile, skillArea),
                "Area de habilidad asignada exitosamente.");
        }
    }
}
