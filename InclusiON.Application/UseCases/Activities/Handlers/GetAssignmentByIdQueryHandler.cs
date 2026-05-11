using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetAssignmentByIdQueryHandler
        : IQueryHandler<GetAssignmentByIdQuery, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IEncryptionService _encryption;

        public GetAssignmentByIdQueryHandler(IActivityAssignmentRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            GetAssignmentByIdQuery query, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetByIdAsync(query.AssignmentId, cancellationToken);

            if (assignment is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Asignación");

            // Solo puede acceder la persona asignada o el profesional que la asignó
            if (assignment.PersonId != query.RequesterId &&
                assignment.AssignedByProfessionalId != query.RequesterId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            var dto = ActivityAssignmentResponse.From(assignment);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(assignment.Id.ToString()));
            foreach (var attempt in dto.Responses)
                attempt.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(attempt.Id.ToString()));
            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(dto);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
