using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para asignaciones profesional-persona y profesional-institucion.
    /// </summary>
    public interface IAssignmentsRepository
    {
        // Professional-Person
        Task<List<ProfessionalPerson>> GetPersonsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default);
        Task<List<ProfessionalPerson>> GetProfessionalsByPersonIdAsync(Guid personId, CancellationToken ct = default);
        Task<ProfessionalPerson?> GetAssignmentAsync(Guid professionalId, Guid personId, CancellationToken ct = default);
        Task<ProfessionalPerson> CreateAssignmentAsync(ProfessionalPerson assignment, CancellationToken ct = default);

        // Professional-Institution
        Task<List<ProfessionalInstitution>> GetInstitutionsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default);
        Task<ProfessionalInstitution?> GetInstitutionAssignmentAsync(Guid professionalId, int institutionId, CancellationToken ct = default);
        Task<ProfessionalInstitution> CreateInstitutionAssignmentAsync(ProfessionalInstitution assignment, CancellationToken ct = default);

        /// <summary>
        /// Indica si un profesional (por UserId) y un familiar (por UserId) comparten
        /// al menos una persona con discapacidad con asignaciones activas en ambos lados.
        /// </summary>
        Task<bool> HaveSharedPersonAsync(
            Guid professionalUserId,
            Guid familyUserId,
            CancellationToken ct = default);

        /// <summary>
        /// Devuelve los Users de los representantes familiares vinculados activamente
        /// a las personas asignadas al profesional indicado (por UserId).
        /// </summary>
        Task<List<User>> GetContactsForProfessionalAsync(
            Guid professionalUserId,
            CancellationToken ct = default);

        /// <summary>
        /// Devuelve los Users de los profesionales vinculados activamente
        /// a las personas que representa el familiar indicado (por UserId).
        /// </summary>
        Task<List<User>> GetContactsForFamilyAsync(
            Guid familyUserId,
            CancellationToken ct = default);
        Task CancelActiveAssignmentsForProfessionalAndPersonAsync(Guid professionalId, Guid personId, CancellationToken ct = default);
    }
}
