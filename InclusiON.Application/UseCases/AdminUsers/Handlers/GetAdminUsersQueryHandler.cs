using System.Data;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class GetAdminUsersQueryHandler : IQueryHandler<GetAdminUsersQuery, ApiResponse<PagedResponse<AdminUserListItemResponse>>>
    {
        private readonly IRawDbExecutor _db;

        public GetAdminUsersQueryHandler(IRawDbExecutor db)
        {
            _db = db;
        }

        public async Task<ApiResponse<PagedResponse<AdminUserListItemResponse>>> HandleAsync(
            GetAdminUsersQuery query, CancellationToken cancellationToken)
        {
            var whereClauses = new List<string>();
            var configureParams = BuildParameters(query, whereClauses);

            var whereClause = whereClauses.Count > 0
                ? "WHERE " + string.Join(" AND ", whereClauses)
                : "";

            var orderColumn = query.SortBy switch
            {
                SortField.Email => "u.Email",
                SortField.Name or SortField.FirstName => "FullName",
                SortField.CreatedAt => "u.CreatedAt",
                _ => "u.CreatedAt"
            };
            var orderDirection = query.SortDirection?.ToUpper() == "ASC" ? "ASC" : "DESC";
            var skip = (query.Page - 1) * query.PageSize;

            var baseSql = $@"
                FROM Users u
                INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                LEFT JOIN Professionals p ON u.Id = p.UserId
                LEFT JOIN PersonsWithDisability pwd ON u.Id = pwd.UserId
                LEFT JOIN FamilyRepresentatives fr ON u.Id = fr.UserId
                {whereClause}";

            // Count
            var countSql = $"SELECT COUNT(*) {baseSql}";
            var totalRecords = (int)(await _db.ExecuteScalarAsync<int>(countSql, configureParams, cancellationToken))!;

            // Data
            var dataSql = $@"
                SELECT u.Id AS UserId, u.Email, u.IsActive, u.LastLoginDate, u.CreatedAt, u.MustChangePassword,
                       r.Name AS Role,
                       LTRIM(RTRIM(
                           COALESCE(p.FirstName, pwd.FirstName, fr.FirstName, u.Name, '') + ' ' +
                           COALESCE(p.LastName, pwd.LastName, fr.LastName, u.Surname, '')
                       )) AS FullName
                {baseSql}
                ORDER BY {orderColumn} {orderDirection}
                OFFSET {skip} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

            var items = await _db.QueryAsync(dataSql, MapRow, configureParams, cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalRecords / query.PageSize);

            var response = new PagedResponse<AdminUserListItemResponse>
            {
                Data = items.ToList(),
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = query.Page,
                PageSize = query.PageSize,
                HasNextPage = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<AdminUserListItemResponse>>.SuccessResult(response);
        }

        private static Action<IDbCommand> BuildParameters(GetAdminUsersQuery query, List<string> whereClauses)
        {
            return cmd =>
            {
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    whereClauses.Add(@"(u.Email LIKE @Search
                        OR COALESCE(p.FirstName, pwd.FirstName, fr.FirstName, u.Name, '') LIKE @Search
                        OR COALESCE(p.LastName, pwd.LastName, fr.LastName, u.Surname, '') LIKE @Search)");
                    AddParam(cmd, "@Search", $"%{query.Search}%");
                }

                if (!string.IsNullOrWhiteSpace(query.Role))
                {
                    whereClauses.Add("r.Name = @Role");
                    AddParam(cmd, "@Role", query.Role);
                }

                if (query.IsActive.HasValue)
                {
                    whereClauses.Add("u.IsActive = @IsActive");
                    AddParam(cmd, "@IsActive", query.IsActive.Value);
                }

                if (query.InstitutionIds is not null && query.InstitutionIds.Count > 0)
                {
                    // Build IN clause with individual parameters
                    var paramNames = new List<string>();
                    for (int i = 0; i < query.InstitutionIds.Count; i++)
                    {
                        var paramName = $"@InstId{i}";
                        paramNames.Add(paramName);
                        AddParam(cmd, paramName, query.InstitutionIds[i]);
                    }
                    var inClause = string.Join(", ", paramNames);

                    whereClauses.Add($@"EXISTS (
                        SELECT 1 FROM ProfessionalInstitutions pi
                        WHERE pi.ProfessionalId = p.Id AND pi.InstitutionId IN ({inClause}) AND pi.IsActive = 1
                        UNION ALL
                        SELECT 1 FROM ProfessionalInstitutions pi2
                        INNER JOIN ProfessionalPersons pp2 ON pi2.ProfessionalId = pp2.ProfessionalId AND pp2.IsActive = 1
                        WHERE pp2.PersonId = pwd.Id AND pi2.InstitutionId IN ({inClause}) AND pi2.IsActive = 1
                        UNION ALL
                        SELECT 1 FROM AdminInstitutions ai
                        WHERE ai.AdminUserId = u.Id AND ai.InstitutionId IN ({inClause}) AND ai.IsActive = 1
                        UNION ALL
                        SELECT 1 FROM ProfessionalInstitutions pi3
                        INNER JOIN ProfessionalPersons pp3 ON pi3.ProfessionalId = pp3.ProfessionalId AND pp3.IsActive = 1
                        INNER JOIN PersonRepresentatives pr ON pp3.PersonId = pr.PersonId AND pr.IsActive = 1
                        WHERE pr.RepresentativeId = fr.Id AND pi3.InstitutionId IN ({inClause}) AND pi3.IsActive = 1
                    )");
                }
            };
        }

        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            cmd.Parameters.Add(param);
        }

        private static AdminUserListItemResponse MapRow(IDataReader reader)
        {
            return new AdminUserListItemResponse
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                MustChangePassword = reader.GetBoolean(reader.GetOrdinal("MustChangePassword")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                FullName = reader.GetString(reader.GetOrdinal("FullName"))
            };
        }
    }
}
