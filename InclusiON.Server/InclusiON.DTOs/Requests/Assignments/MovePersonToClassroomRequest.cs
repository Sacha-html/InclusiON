namespace InclusiON.DTOs.Requests.Assignments
{
    /// <summary>
    /// Request para mover un alumno a un aula específica (o sacarlo de todas las aulas si ClassroomId es null).
    /// </summary>
    public class MovePersonToClassroomRequest
    {
        public Guid? ClassroomId { get; set; }
    }
}
