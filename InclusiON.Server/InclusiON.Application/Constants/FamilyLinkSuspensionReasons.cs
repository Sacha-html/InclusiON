namespace InclusiON.Application.Constants
{
    /// <summary>
    /// Marcadores usados en <see cref="Domain.Models.PersonRepresentative.UnlinkObservation"/>
    /// para distinguir una suspensión automática del sistema (reversible al reactivar al
    /// familiar) de una desvinculación explícita hecha por un administrador (definitiva).
    /// </summary>
    public static class FamilyLinkSuspensionReasons
    {
        public const string SystemSuspendedByFamilyDeactivation = "SYSTEM_SUSPENDED_BY_FAMILY_DEACTIVATION";
    }
}
