using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetPersonActivityAssignmentsQueryHandler
        : IQueryHandler<GetPersonActivityAssignmentsQuery, ApiResponse<List<ActivityAssignmentResponse>>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IEncryptionService _encryption;

        public GetPersonActivityAssignmentsQueryHandler(IActivityAssignmentRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<List<ActivityAssignmentResponse>>> HandleAsync(
            GetPersonActivityAssignmentsQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _repository.GetByPersonIdAsync(query.PersonId, cancellationToken);

            // Security: only return assignments the requester owns.
            // Student: RequesterId == PersonId  → sees all their own assignments.
            // Professional: RequesterId == AssignedByProfessionalId → sees only their assignments.
            var authorized = assignments
                .Where(a => a.PersonId == query.RequesterId ||
                            a.AssignedByProfessionalId == query.RequesterId)
                .ToList();

            var response = authorized.Select(a =>
            {
                var dto = ActivityAssignmentResponse.From(a);
                dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(a.Id.ToString()));
                foreach (var attempt in dto.Responses)
                    attempt.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(attempt.Id.ToString()));
                return dto;
            }).ToList();

            return ApiResponse<List<ActivityAssignmentResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
