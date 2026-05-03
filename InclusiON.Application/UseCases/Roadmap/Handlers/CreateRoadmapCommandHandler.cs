using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
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
        private readonly IRoadmapRepository   _roadmaps;
        private readonly IProfessionalsRepository _professionals;
        private readonly IUnitOfWork          _uow;

        public CreateRoadmapCommandHandler(
            IRoadmapRepository roadmaps,
            IProfessionalsRepository professionals,
            IUnitOfWork uow)
        {
            _roadmaps      = roadmaps;
            _professionals = professionals;
            _uow           = uow;
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
                CreatedByProfessional   = professional
            };

            await _roadmaps.CreateAsync(roadmap, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<RoadmapResponse>.SuccessResult(
                GetPersonRoadmapQueryHandler.Map(roadmap),
                "Roadmap creado exitosamente.");
        }
    }
}
