namespace InclusiON.DTOs.Responses.Admin
{
    public class AdminDashboardResponse
    {
        // Usuarios y personal
        public int TotalProfessionals         { get; set; }
        public int PendingValidations         { get; set; }   // profesionales sin validar
        public int TotalFamilies              { get; set; }

        // Personas con discapacidad
        public int TotalPersons               { get; set; }

        // Instituciones (solo GlobalAdmin)
        public int? TotalInstitutions         { get; set; }

        // Actividades
        public int ActiveAssignments          { get; set; }

        // Reportes
        public int ReportsPendingApproval     { get; set; }
        public int ReportsApprovedThisMonth   { get; set; }
    }
}
