using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface ICalendarEventsRepository
    {
        Task<List<CalendarEvent>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default);
        Task<List<CalendarEvent>> GetByStudentIdsAsync(List<Guid> studentIds, CancellationToken cancellationToken = default);
        Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CalendarEvent> CreateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
        Task UpdateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
        Task DeleteAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
    }
}
