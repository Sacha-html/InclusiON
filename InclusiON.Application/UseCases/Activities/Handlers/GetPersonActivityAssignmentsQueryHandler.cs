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
        private readonly IFamilyRepository _familyRepository;

        public GetPersonActivityAssignmentsQueryHandler(
            IActivityAssignmentRepository repository,
            IEncryptionService encryption,
            IFamilyRepository familyRepository)
        {
            _repository = repository;
            _encryption = encryption;
            _familyRepository = familyRepository;
        }

        public async Task<ApiResponse<List<ActivityAssignmentResponse>>> HandleAsync(
            GetPersonActivityAssignmentsQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _repository.GetByPersonIdAsync(query.PersonId, cancellationToken);

            // Check if requester is an active family representative of this person
            var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(
                query.PersonId, cancellationToken);
            var isFamilyRep = representatives.Any(r => r.RepresentativeId == query.RequesterId && r.IsActive);

            // Security: allow person, assigned professionals, or active family representatives.
            // Student: RequesterId == PersonId  → sees all their own assignments.
            // Professional: RequesterId == AssignedByProfessionalId → sees only their assignments.
            // Family: active representative of the person → sees all assignments.
            var authorized = assignments
                .Where(a => a.PersonId == query.RequesterId ||
                            a.AssignedByProfessionalId == query.RequesterId ||
                            isFamilyRep)
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
