namespace InclusiON.DTOs.Requests.Roadmap
{
    public class ReorderRoadmapActivitiesRequest
    {
        /// <summary>Lista de actividades con su nuevo orden secuencial.</summary>
        public List<ReorderActivityItem> Activities { get; set; } = new();
    }

    public class ReorderActivityItem
    {
        /// <summary>ID de la entrada en el roadmap (PersonRoadmapActivity.Id).</summary>
        public int Id { get; set; }

        /// <summary>Nuevo orden secuencial (1-based).</summary>
        public int SequenceOrder { get; set; }
    }
}
