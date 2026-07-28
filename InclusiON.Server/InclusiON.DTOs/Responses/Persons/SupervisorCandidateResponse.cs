namespace InclusiON.DTOs.Responses.Persons
{
    /// <summary>
    /// Candidato a supervisor de login asistido para una persona con discapacidad.
    /// Combina profesionales asignados (con CanSuperviseLogin) y familiares vinculados activos.
    /// </summary>
    public class SupervisorCandidateResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public string? AvatarColor { get; set; }
    }
}
