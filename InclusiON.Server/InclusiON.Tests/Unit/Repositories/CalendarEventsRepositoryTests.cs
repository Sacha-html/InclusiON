using FluentAssertions;
using Xunit;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Calendar;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Tests.TestSupport;

namespace InclusiON.Tests.Unit.Repositories;

public class CalendarEventsRepositoryTests : DbContextTestBase
{
    private readonly CalendarEventsRepository _repository;

    public CalendarEventsRepositoryTests()
    {
        _repository = new CalendarEventsRepository(Db);
    }

    [Fact]
    public async Task GetByStudentIdsAsync_ReturnsSingleEventForMatchingStudent_AndFiltersAllByAssignedProfessional()
    {
        var assignedProf = new Professional { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Laura", LastName = "Gómez", IsActive = true };
        var unrelatedProf = new Professional { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Carlos", LastName = "Ruiz", IsActive = true };
        var joan = new PersonWithDisability { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "joan", LastName = "joan", BirthDate = new DateTime(2015, 1, 1), IsActive = true };
        var ricardo = new PersonWithDisability { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), FirstName = "Ricardo", LastName = "Jose", BirthDate = new DateTime(2015, 1, 1), IsActive = true };

        Db.Professionals.AddRange(assignedProf, unrelatedProf);
        Db.PersonsWithDisability.AddRange(joan, ricardo);
        Db.ProfessionalPersons.Add(new ProfessionalPerson { ProfessionalId = assignedProf.Id, PersonId = joan.Id, IsActive = true, AssignedAt = DateTime.UtcNow });
        Db.ProfessionalPersons.Add(new ProfessionalPerson { ProfessionalId = unrelatedProf.Id, PersonId = ricardo.Id, IsActive = true, AssignedAt = DateTime.UtcNow });

        var joanTutoria = new CalendarEvent
        {
            Title = "Tutoría de Lectura",
            Type = "Tutoría",
            Date = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc),
            Time = "10:00",
            TargetScope = "single",
            StudentId = joan.Id,
            StudentName = "joan joan",
            CreatedByProfessionalId = assignedProf.Id,
            IsActive = true
        };
        var ricardoTutoria = new CalendarEvent
        {
            Title = "Tutoría Ricardo",
            Type = "Tutoría",
            Date = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc),
            Time = "11:00",
            TargetScope = "single",
            StudentId = ricardo.Id,
            StudentName = "Ricardo Jose",
            CreatedByProfessionalId = unrelatedProf.Id,
            IsActive = true
        };
        var assignedProfAllEvent = new CalendarEvent
        {
            Title = "Clase General Laura",
            Type = "Clase",
            Date = new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc),
            Time = "09:00",
            TargetScope = "all",
            CreatedByProfessionalId = assignedProf.Id,
            IsActive = true
        };
        var unrelatedProfAllEvent = new CalendarEvent
        {
            Title = "Clase General Carlos (NO debe aparecer a Joan)",
            Type = "Clase",
            Date = new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc),
            Time = "14:00",
            TargetScope = "all",
            CreatedByProfessionalId = unrelatedProf.Id,
            IsActive = true
        };

        Db.CalendarEvents.AddRange(joanTutoria, ricardoTutoria, assignedProfAllEvent, unrelatedProfAllEvent);
        await Db.SaveChangesAsync();

        var results = await _repository.GetByStudentIdsAsync([joan.Id]);

        results.Should().HaveCount(2);
        results.Select(e => e.Title).Should().BeEquivalentTo(["Tutoría de Lectura", "Clase General Laura"]);

        var dto = CalendarEventResponse.From(results.First(e => e.Id == joanTutoria.Id));
        dto.StudentId.Should().Be(joan.Id);
        dto.CreatedBy.Should().Be("Laura Gómez");
    }
}
