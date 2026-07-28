namespace InclusiON.Application.UseCases.AdminUsers.Queries
{
    public record GetAdminDashboardQuery(
        bool          IsGlobalAdmin,
        List<int>     InstitutionIds);
}
