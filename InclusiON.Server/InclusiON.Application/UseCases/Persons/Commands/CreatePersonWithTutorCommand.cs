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

        // Tutor (nuevo o existente via ExistingTutorId)
        string? TutorFirstName,
        string? TutorLastName,
        string? TutorEmail,
        string? TutorDocumentNumber,
        string? TutorPhone,
        string TutorRelationship,

        // Asignación de Profesional y Aula
        Guid ProfessionalId,
        Guid? ClassroomId,

        // Vinculación explícita a un tutor ya registrado (ej. hermanos)
        Guid? ExistingTutorId = null
    );
}
