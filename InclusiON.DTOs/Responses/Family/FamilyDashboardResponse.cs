namespace InclusiON.DTOs.Responses.Family
{
    /// <summary>
    /// Dashboard del familiar: resumen de personas vinculadas, mensajes y actividad reciente.
    /// </summary>
    public class FamilyDashboardResponse
    {
        /// <summary>Personas activamente vinculadas al familiar.</summary>
        public List<FamilyPersonSummaryResponse> Persons { get; set; } = new();

        /// <summary>Cantidad de mensajes no leídos del familiar.</summary>
        public int UnreadMessages { get; set; }
    }
}
