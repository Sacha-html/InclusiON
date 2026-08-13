using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.Domain.Enums;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class AssignmentsRepository : IAssignmentsRepository
    {
        private readonly AppDbContext _context;

        public AssignmentsRepository(AppDbContext context)
        {
            _context = context;
        }

        // Professional-Person

        public async Task<List<ProfessionalPerson>> GetPersonsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default)
        {
            return await _context.ProfessionalPersons
                .Include(pp => pp.Person)
                    .ThenInclude(p => p.DisabilityType)
                .Include(pp => pp.Classroom)
                .AsNoTracking()
                .Where(pp => pp.ProfessionalId == professionalId)
                .OrderByDescending(pp => pp.AssignedAt)
                .ToListAsync(ct);
        }

        public async Task<List<ProfessionalPerson>> GetProfessionalsByPersonIdAsync(Guid personId, CancellationToken ct = default)
        {
            return await _context.ProfessionalPersons
                .Include(pp => pp.Professional)
                    .ThenInclude(p => p.User)
                .AsNoTracking()
                .Where(pp => pp.PersonId == personId && pp.IsActive)
                .OrderByDescending(pp => pp.IsPrimaryProfessional)
                .ThenByDescending(pp => pp.AssignedAt)
                .ToListAsync(ct);
        }

        public async Task<ProfessionalPerson?> GetAssignmentAsync(Guid professionalId, Guid personId, CancellationToken ct = default)
        {
            return await _context.ProfessionalPersons
                .Include(pp => pp.Person)
                .AsNoTracking()
                .FirstOrDefaultAsync(pp => pp.ProfessionalId == professionalId && pp.PersonId == personId, ct);
        }

        public async Task<ProfessionalPerson> CreateAssignmentAsync(ProfessionalPerson assignment, CancellationToken ct = default)
        {
            await _context.ProfessionalPersons.AddAsync(assignment, ct);
            return assignment;
        }

        public async Task<ProfessionalPerson?> MovePersonToClassroomAsync(Guid professionalId, Guid personId, Guid? classroomId, CancellationToken ct = default)
        {
            var assignment = await _context.ProfessionalPersons
                .Include(pp => pp.Person)
                .Include(pp => pp.Classroom)
                .FirstOrDefaultAsync(pp => pp.ProfessionalId == professionalId && pp.PersonId == personId && pp.IsActive, ct);
            if (assignment == null) return null;

            assignment.ClassroomId = classroomId;
            if (classroomId.HasValue)
            {
                assignment.Classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classroomId.Value, ct);
            }
            else
            {
                assignment.Classroom = null;
            }
            return assignment;
        }

        // Professional-Institution

        public async Task<List<ProfessionalInstitution>> GetInstitutionsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default)
        {
            return await _context.ProfessionalInstitutions
                .Include(pi => pi.Institution)
                .AsNoTracking()
                .Where(pi => pi.ProfessionalId == professionalId)
                .OrderByDescending(pi => pi.AssignedAt)
                .ToListAsync(ct);
        }

        public async Task<ProfessionalInstitution?> GetInstitutionAssignmentAsync(Guid professionalId, int institutionId, CancellationToken ct = default)
        {
            return await _context.ProfessionalInstitutions
                .Include(pi => pi.Institution)
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.ProfessionalId == professionalId && pi.InstitutionId == institutionId, ct);
        }

        public async Task<ProfessionalInstitution> CreateInstitutionAssignmentAsync(ProfessionalInstitution assignment, CancellationToken ct = default)
        {
            await _context.ProfessionalInstitutions.AddAsync(assignment, ct);
            return assignment;
        }

        public async Task<bool> HaveSharedPersonAsync(
            Guid professionalUserId,
            Guid familyUserId,
            CancellationToken ct = default)
        {
            return await (
                from pp   in _context.ProfessionalPersons
                join prof in _context.Professionals         on pp.ProfessionalId    equals prof.Id
                join pr   in _context.PersonRepresentatives on pp.PersonId          equals pr.PersonId
                join fam  in _context.FamilyRepresentatives on pr.RepresentativeId  equals fam.Id
                where pp.IsActive  && pr.IsActive
                   && prof.UserId  == professionalUserId
                   && fam.UserId   == familyUserId
                select 1
            ).AnyAsync(ct);
        }

        public async Task<List<User>> GetContactsForProfessionalAsync(
            Guid professionalUserId,
            CancellationToken ct = default)
        {
            return await (
                from prof in _context.Professionals
                join pp   in _context.ProfessionalPersons   on prof.Id            equals pp.ProfessionalId
                join pr   in _context.PersonRepresentatives on pp.PersonId         equals pr.PersonId
                join fam  in _context.FamilyRepresentatives on pr.RepresentativeId equals fam.Id
                join u    in _context.Users                 on fam.UserId          equals u.Id
                where pp.IsActive && pr.IsActive && u.IsActive
                   && prof.UserId == professionalUserId
                select u
            ).Distinct().AsNoTracking().ToListAsync(ct);
        }

        public async Task<List<User>> GetContactsForFamilyAsync(
            Guid familyUserId,
            CancellationToken ct = default)
        {
            return await (
                from fam  in _context.FamilyRepresentatives
                join pr   in _context.PersonRepresentatives on fam.Id              equals pr.RepresentativeId
                join pp   in _context.ProfessionalPersons   on pr.PersonId         equals pp.PersonId
                join prof in _context.Professionals         on pp.ProfessionalId   equals prof.Id
                join u    in _context.Users                 on prof.UserId         equals u.Id
                where pr.IsActive && pp.IsActive && u.IsActive
                   && fam.UserId == familyUserId
                select u
            ).Distinct().AsNoTracking().ToListAsync(ct);
        }

        public async Task CancelActiveAssignmentsForProfessionalAndPersonAsync(Guid professionalId, Guid personId, CancellationToken ct = default)
        {
            var activeActivityAssignments = await _context.ActivityAssignments
                .Where(aa => aa.PersonId == personId && 
                             aa.AssignedByProfessionalId == professionalId &&
                             (aa.StatusId == AssignmentStatuses.Pendiente || aa.StatusId == AssignmentStatuses.EnProgreso))
                .ToListAsync(ct);

            foreach (var aa in activeActivityAssignments)
            {
                aa.StatusId = AssignmentStatuses.Cancelada;
            }
        }

        // Classrooms

        public async Task<Classroom> CreateClassroomAsync(Classroom classroom, CancellationToken ct = default)
        {
            await _context.Classrooms.AddAsync(classroom, ct);
            return classroom;
        }

        public async Task<List<Classroom>> GetClassroomsByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default)
        {
            return await _context.Classrooms
                .Include(c => c.ProfessionalPersons)
                .AsNoTracking()
                .Where(c => c.ProfessionalId == professionalId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);
        }

        public async Task<Classroom?> GetClassroomByIdAsync(Guid classroomId, CancellationToken ct = default)
        {
            return await _context.Classrooms
                .Include(c => c.ProfessionalPersons)
                .FirstOrDefaultAsync(c => c.Id == classroomId, ct);
        }

        public async Task<Classroom?> UpdateClassroomAsync(Guid classroomId, string name, CancellationToken ct = default)
        {
            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classroomId, ct);
            if (classroom == null) return null;
            classroom.Name = name.Trim();
            return classroom;
        }

        public async Task<Classroom?> DeactivateClassroomAsync(Guid classroomId, CancellationToken ct = default)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.ProfessionalPersons)
                .FirstOrDefaultAsync(c => c.Id == classroomId, ct);
            if (classroom == null) return null;

            // Desvincular alumnos del aula (ClassroomId = null) sin desactivar la asignación
            foreach (var pp in classroom.ProfessionalPersons.Where(pp => pp.IsActive))
            {
                pp.ClassroomId = null;
            }

            classroom.IsActive = false;
            return classroom;
        }

        public async Task<(bool success, string error)> DeleteClassroomAsync(Guid classroomId, CancellationToken ct = default)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.ProfessionalPersons)
                .FirstOrDefaultAsync(c => c.Id == classroomId, ct);
            if (classroom == null) return (false, "not_found");

            var activeStudents = classroom.ProfessionalPersons.Count(pp => pp.IsActive);
            if (activeStudents > 0) return (false, "has_students");

            _context.Classrooms.Remove(classroom);
            return (true, string.Empty);
        }
    }
}
