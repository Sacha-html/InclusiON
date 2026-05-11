using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class AddRoadmapAreaCommandHandler
        : ICommandHandler<AddRoadmapAreaCommand, ApiResponse<RoadmapAreaResponse>>
    {
        private readonly IRoadmapRepository             _roadmaps;
        private readonly IReadOnlyRepository<SkillArea> _skillAreas;
        private readonly IUnitOfWork                    _uow;
        private readonly IEncryptionService             _encryption;

        public AddRoadmapAreaCommandHandler(
            IRoadmapRepository roadmaps,
            IReadOnlyRepository<SkillArea> skillAreas,
            IUnitOfWork uow,
            IEncryptionService encryption)
        {
            _roadmaps   = roadmaps;
            _skillAreas = skillAreas;
            _uow        = uow;
            _encryption = encryption;
        }

        public async Task<ApiResponse<RoadmapAreaResponse>> HandleAsync(
            AddRoadmapAreaCommand command, CancellationToken cancellationToken)
        {
            // 1. Verificar que el roadmap existe
            var roadmap = await _roadmaps.GetByPersonIdAsync(command.PersonId, cancellationToken);
            if (roadmap is null)
                return ApiResponse<RoadmapAreaResponse>.NotFound("Roadmap");

            // 2. Verificar que el area de habilidad existe
            var skillArea = await _skillAreas.GetByIdAsync(command.SkillAreaId, cancellationToken);
            if (skillArea is null)
                return ApiResponse<RoadmapAreaResponse>.NotFound("Area de habilidad");

            // 3. Verificar que el area no este ya en el roadmap
            if (await _roadmaps.AreaExistsInRoadmapAsync(roadmap.Id, command.SkillAreaId, cancellationToken))
                return ApiResponse<RoadmapAreaResponse>.Conflict(
                    ErrorCode.Conflict,
                    "El area de habilidad ya forma parte del roadmap.");

            // 4. Crear el area
            // Note: do NOT assign the SkillArea navigation property here.
            // skillArea was loaded via ReadOnlyRepository (AsNoTracking), so it is
            // detached. Assigning it to the new entity would make EF try to INSERT it,
            // causing a PK_SkillAreas duplicate-key violation on SaveChangesAsync.
            // Setting only the FK (SkillAreaId) is sufficient.
            var area = new PersonRoadmapArea
            {
                PersonRoadmapId = roadmap.Id,
                SkillAreaId     = command.SkillAreaId,
                DisplayOrder    = command.DisplayOrder,
            };

            await _roadmaps.AddAreaAsync(area, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var areaDto = new RoadmapAreaResponse
            {
                Id            = area.Id,
                EncryptedId   = ToUrlSafeBase64(_encryption.Encrypt(area.Id.ToString())),
                SkillAreaId   = area.SkillAreaId,
                SkillAreaName = skillArea.Name,
                Color         = skillArea.Color,
                Icon          = skillArea.Icon,
                DisplayOrder  = area.DisplayOrder,
                Activities    = new List<RoadmapActivityResponse>()
            };
            return ApiResponse<RoadmapAreaResponse>.SuccessResult(areaDto,
                "Area de habilidad agregada al roadmap exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
