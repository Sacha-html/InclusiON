namespace InclusiON.Application.Constants
{
    /// <summary>
    /// Razones estandarizadas para la revocación de refresh tokens.
    /// </summary>
    public static class RevokeReasons
    {
        public const string NewLogin = "Nuevo inicio de sesión - sesiones previas invalidadas";
        public const string AdminPasswordReset = "Contraseña reseteada por administrador";
        public const string UserDeactivated = "Usuario desactivado por administrador";
        public const string ProfessionalDeactivated = "Profesional desactivado";
        public const string PersonDeactivated = "Persona desactivada";
        public const string FamilyDeactivated = "Familiar desactivado";
        public const string RolePermissionsUpdated = "Permisos del rol actualizados";
        public const string ManualRevoke = "Revocación manual";
    }
}
