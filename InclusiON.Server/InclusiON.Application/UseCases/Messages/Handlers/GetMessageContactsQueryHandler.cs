using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class GetMessageContactsQueryHandler
        : IQueryHandler<GetMessageContactsQuery, ApiResponse<PagedResponse<MessageContactResponse>>>
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

        public async Task<ApiResponse<PagedResponse<MessageContactResponse>>> HandleAsync(
            GetMessageContactsQuery query, CancellationToken cancellationToken)
        {
            var user = await _users.GetByIdWithProfileAsync(query.UserId, cancellationToken);

            if (user is null)
                return ApiResponse<PagedResponse<MessageContactResponse>>.NotFound("Usuario");

            if (user.PersonWithDisability is not null)
                return ApiResponse<PagedResponse<MessageContactResponse>>.SuccessResult(
                    new PagedResponse<MessageContactResponse> { Data = new(), TotalRecords = 0, TotalPages = 0, CurrentPage = query.Page, PageSize = query.PageSize });

            List<MessageContactResponse> all;

            if (user.Professional is not null)
            {
                var families = await _assignments.GetContactsForProfessionalAsync(query.UserId, cancellationToken);
                all = families.Select(u => MessageMapper.ToContactResponse(u, RoleNames.FamilyRepresentative)).ToList();
            }
            else
            {
                var professionals = await _assignments.GetContactsForFamilyAsync(query.UserId, cancellationToken);
                all = professionals.Select(u => MessageMapper.ToContactResponse(u, RoleNames.Professional)).ToList();
            }

            var totalRecords = all.Count;
            var totalPages   = (int)Math.Ceiling(totalRecords / (double)query.PageSize);
            var data         = all.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

            var response = new PagedResponse<MessageContactResponse>
            {
                Data            = data,
                TotalRecords    = totalRecords,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<MessageContactResponse>>.SuccessResult(response);
        }
    }
}
