using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class CalendarEventsRepository : ICalendarEventsRepository
    {
        private readonly AppDbContext _context;

        public CalendarEventsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CalendarEvent>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default)
        {
            return await _context.CalendarEvents
                .Include(c => c.Student)
                .Where(c => c.CreatedByProfessionalId == professionalId && c.IsActive)
                .OrderBy(c => c.Date)
                .ThenBy(c => c.Time)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CalendarEvent>> GetByStudentIdsAsync(List<Guid> studentIds, CancellationToken cancellationToken = default)
        {
            return await _context.CalendarEvents
                .Include(c => c.Student)
                .Where(c => c.IsActive && (c.TargetScope == "all" || (c.StudentId != null && studentIds.Contains(c.StudentId.Value))))
                .OrderBy(c => c.Date)
                .ThenBy(c => c.Time)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.CalendarEvents
                .Include(c => c.Student)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);
        }

        public async Task<CalendarEvent> CreateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
        {
            await _context.CalendarEvents.AddAsync(calendarEvent, cancellationToken);
            return calendarEvent;
        }

        public Task UpdateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
        {
            _context.CalendarEvents.Update(calendarEvent);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
        {
            calendarEvent.IsActive = false;
            _context.CalendarEvents.Update(calendarEvent);
            return Task.CompletedTask;
        }
    }
}
