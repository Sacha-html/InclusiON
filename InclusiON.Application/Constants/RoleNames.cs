namespace InclusiON.Application.Constants
{
    /// <summary>
    /// Nombres de los roles del sistema tal como están registrados en ASP.NET Core Identity.
    /// Usar estas constantes en lugar de strings literales en handlers, servicios y tests.
    /// </summary>
    public static class RoleNames
    {
        public const string Admin                 = "Admin";
        public const string Professional          = "Professional";
        public const string PersonWithDisability  = "PersonWithDisability";
        public const string FamilyRepresentative  = "FamilyRepresentative";

        /// <summary>
        /// UserType string usado en el flujo de identificación y login asistido.
        /// Distinto de FamilyRepresentative (rol de Identity) — representa el tipo de usuario en la UI.
        /// </summary>
        public const string Family                = "Family";
    }
}
