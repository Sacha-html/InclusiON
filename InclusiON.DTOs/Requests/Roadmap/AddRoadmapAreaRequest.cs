namespace InclusiON.DTOs.Requests.Roadmap
{
    public class AddRoadmapAreaRequest
    {
        /// <summary>ID del area de habilidad a agregar al roadmap.</summary>
        public int SkillAreaId { get; set; }

        /// <summary>Orden de presentacion dentro del roadmap.</summary>
        public int DisplayOrder { get; set; }
    }
}
