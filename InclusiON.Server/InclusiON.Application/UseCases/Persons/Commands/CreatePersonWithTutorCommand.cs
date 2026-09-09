using System;

namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record CreatePersonWithTutorCommand(
        // Alumno
        string FirstName,
        string LastName,
        string? DocumentNumber,
        DateTime BirthDate,
        string? PhotoUrl,

        // Tutor
        string TutorFirstName,
        string TutorLastName,
        string TutorEmail,
        string? TutorDocumentNumber,
        string? TutorPhone,
        string TutorRelationship,

        // Asignación de Aula
        Guid? ClassroomId
    );
}
