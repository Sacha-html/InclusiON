using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Assignments
{
    /// <summary>
    /// Response con los datos de un aula.
    /// </summary>
    public class ClassroomResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ProfessionalId { get; set; }
        public bool IsActive { get; set; }
        /// <summary>
        /// Cantidad de alumnos activos asignados al aula.
        /// </summary>
        public int StudentCount { get; set; }

        public static ClassroomResponse MapToResponse(Classroom classroom)
        {
            return new ClassroomResponse
            {
                Id = classroom.Id,
                Name = classroom.Name,
                ProfessionalId = classroom.ProfessionalId,
                IsActive = classroom.IsActive,
                StudentCount = classroom.ProfessionalPersons?.Count(pp => pp.IsActive) ?? 0
            };
        }
    }
}
