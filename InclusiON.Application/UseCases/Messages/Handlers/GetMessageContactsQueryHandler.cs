using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class GetMessageContactsQueryHandler
        : IQueryHandler<GetMessageContactsQuery, ApiResponse<List<MessageContactResponse>>>
    {
        private readonly IUsersRepository       _users;
        private readonly IAssignmentsRepository _assignments;

        public GetMessageContactsQueryHandler(
            IUsersRepository users,
            IAssignmentsRepository assignments)
        {
            _users       = users;
            _assignments = assignments;
        }

        public async Task<ApiResponse<List<MessageContactResponse>>> HandleAsync(
            GetMessageContactsQuery query, CancellationToken cancellationToken)
        {
            var user = await _users.GetByIdWithProfileAsync(query.UserId, cancellationToken);

            if (user is null)
                return ApiResponse<List<MessageContactResponse>>.NotFound("Usuario");

            // PwD no participa en mensajería
            if (user.PersonWithDisability is not null)
                return ApiResponse<List<MessageContactResponse>>.SuccessResult(
                    new List<MessageContactResponse>());

            List<MessageContactResponse> contacts;

            if (user.Professional is not null)
            {
                var families = await _assignments.GetContactsForProfessionalAsync(
                    query.UserId, cancellationToken);

                contacts = families
                    .Select(u => new MessageContactResponse
                    {
                        UserId   = u.Id,
                        FullName = MessageMapper.FullName(u),
                        Email    = u.Email ?? string.Empty,
                        UserType = RoleNames.FamilyRepresentative
                    })
                    .ToList();
            }
            else
            {
                // FamilyRepresentative
                var professionals = await _assignments.GetContactsForFamilyAsync(
                    query.UserId, cancellationToken);

                contacts = professionals
                    .Select(u => new MessageContactResponse
                    {
                        UserId   = u.Id,
                        FullName = MessageMapper.FullName(u),
                        Email    = u.Email ?? string.Empty,
                        UserType = RoleNames.Professional
                    })
                    .ToList();
            }

            return ApiResponse<List<MessageContactResponse>>.SuccessResult(contacts);
        }
    }
}
