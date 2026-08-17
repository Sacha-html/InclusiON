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
        private readonly IUsersRepository            _users;
        private readonly IAssignmentsRepository      _assignments;
        private readonly IAdminInstitutionRepository _admins;
        private readonly IProfessionalsRepository    _professionals;
        private readonly IFamilyRepository           _families;
        private readonly IMessagesRepository         _messages;

        public GetMessageContactsQueryHandler(
            IUsersRepository users,
            IAssignmentsRepository assignments,
            IAdminInstitutionRepository admins,
            IProfessionalsRepository professionals,
            IFamilyRepository families,
            IMessagesRepository messages)
        {
            _users         = users;
            _assignments   = assignments;
            _admins        = admins;
            _professionals = professionals;
            _families      = families;
            _messages      = messages;
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

            var all = new List<MessageContactResponse>();

            if (user.Professional is not null)
            {
                var families = await _assignments.GetContactsForProfessionalAsync(query.UserId, cancellationToken);
                all.AddRange(families.Select(u => MessageMapper.ToContactResponse(u, RoleNames.FamilyRepresentative)));

                var admins = await _admins.GetAllAdminsWithInstitutionsAsync(cancellationToken);
                all.AddRange(admins.Where(a => a.IsActive && a.Id != query.UserId).Select(u => MessageMapper.ToContactResponse(u, RoleNames.Admin)));
            }
            else if (user.FamilyRepresentative is not null)
            {
                var professionals = await _assignments.GetContactsForFamilyAsync(query.UserId, cancellationToken);
                all.AddRange(professionals.Select(u => MessageMapper.ToContactResponse(u, RoleNames.Professional)));

                var admins = await _admins.GetAllAdminsWithInstitutionsAsync(cancellationToken);
                all.AddRange(admins.Where(a => a.IsActive && a.Id != query.UserId).Select(u => MessageMapper.ToContactResponse(u, RoleNames.Admin)));
            }
            else
            {
                // Administrador: Acceso global a todos los profesionales y familias registradas
                var professionals = await _professionals.GetAllActiveAsync(cancellationToken);
                all.AddRange(professionals.Where(p => p.User != null && p.User.IsActive && p.User.Id != query.UserId)
                                          .Select(p => MessageMapper.ToContactResponse(p.User!, RoleNames.Professional)));

                var families = await _families.GetAllActiveAsync(cancellationToken);
                all.AddRange(families.Where(f => f.User != null && f.User.IsActive && f.User.Id != query.UserId)
                                     .Select(f => MessageMapper.ToContactResponse(f.User!, RoleNames.FamilyRepresentative)));
            }

            // Eliminar duplicados
            var distinctContacts = all
                .GroupBy(c => c.UserId)
                .Select(g => g.First())
                .ToList();

            // Cargar estadísticas de la conversación (último mensaje y conteo de no leídos)
            var contactIds = distinctContacts.Select(c => c.UserId).ToList();
            var stats = await _messages.GetConversationStatsAsync(query.UserId, contactIds, cancellationToken);

            if (stats != null)
            {
                foreach (var contact in distinctContacts)
                {
                    if (stats.TryGetValue(contact.UserId, out var st))
                    {
                        contact.UltimoMensajeFecha = st.LastMessageDate;
                        contact.MensajesNoLeidos = st.UnreadCount;
                    }
                }
            }

            // Ordenamiento: primero los que tienen actividad reciente (DESC), luego por nombre (ASC)
            var sortedContacts = distinctContacts
                .OrderByDescending(c => c.UltimoMensajeFecha.HasValue)
                .ThenByDescending(c => c.UltimoMensajeFecha)
                .ThenBy(c => c.FullName)
                .ToList();

            var totalRecords = sortedContacts.Count;
            var totalPages   = (int)Math.Ceiling(totalRecords / (double)query.PageSize);
            var data         = sortedContacts.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

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
