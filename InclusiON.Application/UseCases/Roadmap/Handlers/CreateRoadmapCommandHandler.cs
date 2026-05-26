using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class CreateRoadmapCommandHandler
        : ICommandHandler<CreateRoadmapCommand, ApiResponse<RoadmapResponse>>
    {
        private readonly IRoadmapRepository      _roadmaps;
        private readonly IProfessionalsRepository _professionals;
        private readonly IUnitOfWork             _uow;
        private readonly IEncryptionService      _encryption;

        public CreateRoadmapCommandHandler(
            IRoadmapRepository roadmaps,
            IProfessionalsRepository professionals,
            IUnitOfWork uow,
            IEncryptionService encryption)
        {
            _roadmaps      = roadmaps;
            _professionals = professionals;
            _uow           = uow;
            _encryption    = encryption;
        }

        public async Task<ApiResponse<RoadmapResponse>> HandleAsync(
            CreateRoadmapCommand command, CancellationToken cancellationToken)
        {
            // 1. Verificar que no exista ya un roadmap para esta persona
            if (await _roadmaps.ExistsForPersonAsync(command.PersonId, cancellationToken))
                return ApiResponse<RoadmapResponse>.Conflict(
                    ErrorCode.Conflict,
                    "La persona ya tiene un roadmap creado.");

            // 2. Verificar que el profesional existe
            var professional = await _professionals.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional is null)
                return ApiResponse<RoadmapResponse>.NotFound("Profesional");

            // 3. Crear el roadmap
            var roadmap = new PersonRoadmap
            {
                PersonId                = command.PersonId,
                CreatedByProfessionalId = command.ProfessionalId,
                Notes                   = command.Notes,
                // No asignar la navigation property — viene de AsNoTracking y EF la marcaría Added
            };

            await _roadmaps.CreateAsync(roadmap, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Asignar navigation property DESPUÉS del save para que Map() pueda acceder al nombre completo
            roadmap.CreatedByProfessional = professional;

            var dto = RoadmapMapper.ToResponse(roadmap);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(roadmap.Id.ToString()));
            foreach (var area in dto.Areas)
            {
                area.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(area.Id.ToString()));
                foreach (var activity in area.Activities)
                    activity.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(activity.Id.ToString()));
            }
            return ApiResponse<RoadmapResponse>.SuccessResult(dto, "Roadmap creado exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
