using System.Data;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class GetAdminDashboardQueryHandler
        : IQueryHandler<GetAdminDashboardQuery, ApiResponse<AdminDashboardResponse>>
    {
        private readonly IRawDbExecutor _db;

        public GetAdminDashboardQueryHandler(IRawDbExecutor db)
        {
            _db = db;
        }

        public async Task<ApiResponse<AdminDashboardResponse>> HandleAsync(
            GetAdminDashboardQuery query, CancellationToken cancellationToken)
        {
            // AdminInstitucional with no assigned institutions → nothing visible
            if (!query.IsGlobalAdmin && query.InstitutionIds.Count == 0)
                return ApiResponse<AdminDashboardResponse>.SuccessResult(new AdminDashboardResponse());

            var thisMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var instParams     = BuildInstParams(query.InstitutionIds);
            var instIn         = instParams.inClause;
            var configureInst  = instParams.configure;

            // ── Professionals ────────────────────────────────────────────
            var profScope = query.IsGlobalAdmin
                ? string.Empty
                : $@"AND EXISTS (
                    SELECT 1 FROM ""ProfessionalInstitutions"" pi
                    WHERE pi.""ProfessionalId"" = p.""Id""
                      AND pi.""InstitutionId"" IN ({instIn})
                      AND pi.""IsActive"" = true)";

            var sqlTotalProf = $@"
                SELECT COUNT(*)
                FROM ""Professionals"" p
                INNER JOIN ""Users"" u ON u.""Id"" = p.""UserId""
                WHERE u.""IsActive"" = true {profScope}";

            var sqlPendingVal = $@"
                SELECT COUNT(*)
                FROM ""Professionals"" p
                INNER JOIN ""Users"" u ON u.""Id"" = p.""UserId""
                WHERE u.""IsActive"" = true
                  AND p.""Status"" = 0 {profScope}";   /* ProfessionalStatusEnum.Pending = 0 */

            // ── Families ─────────────────────────────────────────────────
            var familyScope = query.IsGlobalAdmin
                ? string.Empty
                : $@"AND EXISTS (
                    SELECT 1
                    FROM ""PersonRepresentatives"" pr
                    JOIN ""ProfessionalPersons""    pp ON pp.""PersonId"" = pr.""PersonId"" AND pp.""IsActive"" = true
                    JOIN ""ProfessionalInstitutions"" pi ON pi.""ProfessionalId"" = pp.""ProfessionalId""
                        AND pi.""InstitutionId"" IN ({instIn}) AND pi.""IsActive"" = true
                    WHERE pr.""RepresentativeId"" = fr.""Id"" AND pr.""IsActive"" = true)";

            var sqlFamilies = $@"
                SELECT COUNT(*)
                FROM ""FamilyRepresentatives"" fr
                INNER JOIN ""Users"" u ON u.""Id"" = fr.""UserId""
                WHERE u.""IsActive"" = true {familyScope}";

            // ── Persons ──────────────────────────────────────────────────
            var personScope = query.IsGlobalAdmin
                ? string.Empty
                : $@"AND EXISTS (
                    SELECT 1
                    FROM ""ProfessionalPersons"" pp
                    JOIN ""ProfessionalInstitutions"" pi ON pi.""ProfessionalId"" = pp.""ProfessionalId""
                        AND pi.""InstitutionId"" IN ({instIn}) AND pi.""IsActive"" = true
                    WHERE pp.""PersonId"" = pwd.""Id"" AND pp.""IsActive"" = true)";

            var sqlPersons = $@"
                SELECT COUNT(*)
                FROM ""PersonsWithDisability"" pwd
                WHERE pwd.""IsActive"" = true {personScope}";

            // ── Institutions (GlobalAdmin only) ───────────────────────────
            var sqlInstitutions = @"SELECT COUNT(*) FROM ""EducationalInstitutions"" WHERE ""IsActive"" = true";

            // ── Active assignments ────────────────────────────────────────
            var assignScope = query.IsGlobalAdmin
                ? string.Empty
                : $@"AND EXISTS (
                    SELECT 1
                    FROM ""ProfessionalPersons"" pp
                    JOIN ""ProfessionalInstitutions"" pi ON pi.""ProfessionalId"" = pp.""ProfessionalId""
                        AND pi.""InstitutionId"" IN ({instIn}) AND pi.""IsActive"" = true
                    WHERE pp.""PersonId"" = aa.""PersonId"" AND pp.""IsActive"" = true)";

            var sqlActiveAssignments = $@"
                SELECT COUNT(*)
                FROM ""ActivityAssignments"" aa
                WHERE aa.""IsActive"" = true {assignScope}";

            // ── Reports ──────────────────────────────────────────────────
            var reportScope = query.IsGlobalAdmin
                ? string.Empty
                : $@"AND EXISTS (
                    SELECT 1
                    FROM ""ProfessionalPersons"" pp
                    JOIN ""ProfessionalInstitutions"" pi ON pi.""ProfessionalId"" = pp.""ProfessionalId""
                        AND pi.""InstitutionId"" IN ({instIn}) AND pi.""IsActive"" = true
                    WHERE pp.""PersonId"" = rpt.""PersonId"" AND pp.""IsActive"" = true)";

            var sqlReportsPending = $@"
                SELECT COUNT(*)
                FROM ""Reports"" rpt
                WHERE rpt.""IsActive"" = true
                  AND rpt.""Status"" = 'Submitted' {reportScope}";

            var sqlReportsApproved = $@"
                SELECT COUNT(*)
                FROM ""Reports"" rpt
                WHERE rpt.""IsActive"" = true
                  AND rpt.""Status"" = 'Approved'
                  AND rpt.""ReportDate"" >= @ThisMonthStart {reportScope}";

            // ── Run all in parallel ───────────────────────────────────────
            var tProf     = _db.ExecuteScalarAsync<int>(sqlTotalProf,         configureInst, cancellationToken);
            var tPending  = _db.ExecuteScalarAsync<int>(sqlPendingVal,        configureInst, cancellationToken);
            var tFamilies = _db.ExecuteScalarAsync<int>(sqlFamilies,          configureInst, cancellationToken);
            var tPersons  = _db.ExecuteScalarAsync<int>(sqlPersons,           configureInst, cancellationToken);
            var tInst     = _db.ExecuteScalarAsync<int>(sqlInstitutions,      null,          cancellationToken);
            var tAssign   = _db.ExecuteScalarAsync<int>(sqlActiveAssignments, configureInst, cancellationToken);
            var tRptPend  = _db.ExecuteScalarAsync<int>(sqlReportsPending,    configureInst, cancellationToken);
            var tRptApprv = _db.ExecuteScalarAsync<int>(sqlReportsApproved,
                cmd =>
                {
                    configureInst?.Invoke(cmd);

                    var p = cmd.CreateParameter();
                    p.ParameterName = "@ThisMonthStart";
                    p.Value         = thisMonthStart;
                    cmd.Parameters.Add(p);
                },
                cancellationToken);

            // Always await all — tInst result only used for GlobalAdmin
            await Task.WhenAll(tProf, tPending, tFamilies, tPersons,
                               tInst, tAssign, tRptPend, tRptApprv);

            var dashboard = new AdminDashboardResponse
            {
                TotalProfessionals       = tProf.Result,
                PendingValidations       = tPending.Result,
                TotalFamilies            = tFamilies.Result,
                TotalPersons             = tPersons.Result,
                TotalInstitutions        = query.IsGlobalAdmin ? tInst.Result : null,
                ActiveAssignments        = tAssign.Result,
                ReportsPendingApproval   = tRptPend.Result,
                ReportsApprovedThisMonth = tRptApprv.Result
            };

            return ApiResponse<AdminDashboardResponse>.SuccessResult(dashboard);
        }

        // ──────────────────────────────────────────────────────────────────
        // Builds the IN (...) clause and the parameter-configuration delegate
        // for institution IDs. Returns empty when GlobalAdmin (no filtering).
        // ──────────────────────────────────────────────────────────────────
        private static (string inClause, Action<IDbCommand>? configure)
            BuildInstParams(List<int> ids)
        {
            if (ids is not { Count: > 0 })
                return (string.Empty, null);

            var names = ids.Select((_, i) => $"@InstId{i}").ToList();

            Action<IDbCommand> configure = cmd =>
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = names[i];
                    p.Value         = ids[i];
                    cmd.Parameters.Add(p);
                }
            };

            return (string.Join(", ", names), configure);
        }
    }
}
