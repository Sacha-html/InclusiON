namespace InclusiON.DTOs.Responses.Roadmap
{
    /// <summary>
    /// Area de habilidad dentro del roadmap de una persona.
    /// </summary>
    public class RoadmapAreaResponse
    {
        /// <summary>ID de la entrada PersonRoadmapArea.</summary>
        public int Id { get; set; }

        /// <summary>ID del area de habilidad.</summary>
        public int SkillAreaId { get; set; }

        /// <summary>Nombre del area de habilidad.</summary>
        public string SkillAreaName { get; set; } = string.Empty;

        /// <summary>Color del area de habilidad.</summary>
        public string? Color { get; set; }

        /// <summary>Icono del area de habilidad.</summary>
        public string? Icon { get; set; }

        /// <summary>Orden de presentacion dentro del roadmap.</summary>
        public int DisplayOrder { get; set; }

        /// <summary>Actividades asignadas en esta area, ordenadas por SequenceOrder.</summary>
        public List<RoadmapActivityResponse> Activities { get; set; } = new();
    }
}
