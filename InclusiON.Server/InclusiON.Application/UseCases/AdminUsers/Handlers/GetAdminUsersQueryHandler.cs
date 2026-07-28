using System.Data;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class
        GetAdminUsersQueryHandler : IQueryHandler<GetAdminUsersQuery,
        ApiResponse<PagedResponse<AdminUserListItemResponse>>>
    {
        private readonly IRawDbExecutor _db;

        public GetAdminUsersQueryHandler(IRawDbExecutor db)
        {
            _db = db;
        }

        public async Task<ApiResponse<PagedResponse<AdminUserListItemResponse>>> HandleAsync(
            GetAdminUsersQuery query, CancellationToken cancellationToken)
        {
            var (whereClause, sqlParams) = BuildFilters(query);

            var orderColumn = query.SortBy switch
            {
                SortField.Email    => "u.\"Email\"",
                SortField.Name or
                SortField.FirstName => "FullName",
                _                  => "u.\"CreatedAt\""
            };
            var orderDirection = query.SortDirection?.ToUpper() == "ASC" ? "ASC" : "DESC";
            var skip    = (query.Page - 1) * query.PageSize;

            var baseSql = $@"
                FROM ""Users"" u
                INNER JOIN ""AspNetUserRoles"" ur ON u.""Id"" = ur.""UserId""
                INNER JOIN ""AspNetRoles""     r  ON ur.""RoleId"" = r.""Id""
                LEFT JOIN  ""Professionals""          p   ON u.""Id"" = p.""UserId""
                LEFT JOIN  ""PersonsWithDisability""  pwd ON u.""Id"" = pwd.""UserId""
                LEFT JOIN  ""FamilyRepresentatives""  fr  ON u.""Id"" = fr.""UserId""
                {whereClause}";

            // Count
            var countSql = $"SELECT COUNT(*) {baseSql}";
            var totalRecords = await _db.ExecuteScalarAsync<int>(countSql, sqlParams, cancellationToken);

            // Data
            var dataSql = $@"
                SELECT
                    u.""Id""            AS UserId,
                    u.""Email""         AS Email,
                    u.""IsActive""      AS IsActive,
                    u.""LastLoginDate"" AS LastLoginDate,
                    u.""CreatedAt""     AS CreatedAt,
                    u.""MustChangePassword"" AS MustChangePassword,
                    r.""Name""          AS Role,
                    TRIM(
                        COALESCE(p.""FirstName"",   pwd.""FirstName"",   fr.""FirstName"",   u.""Name"",    '') || ' ' ||
                        COALESCE(p.""LastName"",    pwd.""LastName"",    fr.""LastName"",    u.""Surname"", '')
                    ) AS FullName
                {baseSql}
                ORDER BY {orderColumn} {orderDirection}
                LIMIT {query.PageSize} OFFSET {skip}";

            var items = await _db.QueryAsync(dataSql, AdminUserListItemResponse.FromReader, sqlParams,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalRecords / query.PageSize);

            var response = new PagedResponse<AdminUserListItemResponse>
            {
                Data            = items.ToList(),
                TotalRecords    = totalRecords,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<AdminUserListItemResponse>>.SuccessResult(response);
        }

        private static (string whereClause, Action<IDbCommand> configureParams) BuildFilters(GetAdminUsersQuery query)
        {
            var whereClauses = new List<string>();
            var parameters   = new List<(string Name, object Value)>();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                whereClauses.Add(@"(
                    u.""Email"" ILIKE @Search
                    OR COALESCE(p.""FirstName"",   pwd.""FirstName"",   fr.""FirstName"",   u.""Name"",    '') ILIKE @Search
                    OR COALESCE(p.""LastName"",    pwd.""LastName"",    fr.""LastName"",    u.""Surname"", '') ILIKE @Search
                )");
                parameters.Add(("@Search", $"%{query.Search}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                whereClauses.Add(@"r.""Name"" = @Role");
                parameters.Add(("@Role", query.Role));
            }

            if (query.IsActive.HasValue)
            {
                whereClauses.Add(@"u.""IsActive"" = @IsActive");
                parameters.Add(("@IsActive", query.IsActive.Value));
            }

            if (query.InstitutionIds is { Count: > 0 })
            {
                var paramNames = new List<string>();
                for (int i = 0; i < query.InstitutionIds.Count; i++)
                {
                    var paramName = $"@InstId{i}";
                    paramNames.Add(paramName);
                    parameters.Add((paramName, query.InstitutionIds[i]));
                }

                var inClause = string.Join(", ", paramNames);

                whereClauses.Add($@"EXISTS (
                    SELECT 1 FROM ""ProfessionalInstitutions"" pi
                    WHERE pi.""ProfessionalId"" = p.""Id"" AND pi.""InstitutionId"" IN ({inClause}) AND pi.""IsActive"" = true
                    UNION ALL
                    SELECT 1 FROM ""ProfessionalInstitutions"" pi2
                    INNER JOIN ""ProfessionalPersons"" pp2 ON pi2.""ProfessionalId"" = pp2.""ProfessionalId"" AND pp2.""IsActive"" = true
                    WHERE pp2.""PersonId"" = pwd.""Id"" AND pi2.""InstitutionId"" IN ({inClause}) AND pi2.""IsActive"" = true
                    UNION ALL
                    SELECT 1 FROM ""AdminInstitutions"" ai
                    WHERE ai.""AdminUserId"" = u.""Id"" AND ai.""InstitutionId"" IN ({inClause}) AND ai.""IsActive"" = true
                    UNION ALL
                    SELECT 1 FROM ""ProfessionalInstitutions"" pi3
                    INNER JOIN ""ProfessionalPersons"" pp3 ON pi3.""ProfessionalId"" = pp3.""ProfessionalId"" AND pp3.""IsActive"" = true
                    INNER JOIN ""PersonRepresentatives"" pr ON pp3.""PersonId"" = pr.""PersonId"" AND pr.""IsActive"" = true
                    WHERE pr.""RepresentativeId"" = fr.""Id"" AND pi3.""InstitutionId"" IN ({inClause}) AND pi3.""IsActive"" = true
                )");
            }

            var whereClause = whereClauses.Count > 0
                ? "WHERE " + string.Join(" AND ", whereClauses)
                : "";

            var configureParams = (Action<IDbCommand>)((IDbCommand cmd) =>
            {
                foreach (var (name, value) in parameters)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = name;
                    param.Value         = value;
                    cmd.Parameters.Add(param);
                }
            });

            return (whereClause, configureParams);
        }
    }
}
