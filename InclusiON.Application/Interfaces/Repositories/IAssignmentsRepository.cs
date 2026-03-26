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
        Task<ProfessionalPerson?> GetAssignmentAsync(Guid professionalId, Guid personId, CancellationToken ct = default);
        Task<ProfessionalPerson> CreateAssignmentAsync(ProfessionalPerson assignment, CancellationToken ct = default);

        // Professional-Institution
        Task<List<ProfessionalInstitution>> GetInstitutionsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default);
        Task<ProfessionalInstitution?> GetInstitutionAssignmentAsync(Guid professionalId, int institutionId, CancellationToken ct = default);
        Task<ProfessionalInstitution> CreateInstitutionAssignmentAsync(ProfessionalInstitution assignment, CancellationToken ct = default);
    }
}
