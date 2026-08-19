import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserRoles } from '@shared/constants/roles';
import { AuthService, ToastService, ProfessionalsService, AssignmentsService, FamilyService, CalendarService, CalendarEvent } from '@services';
import { switchMap } from 'rxjs';
import {
  CardBodyComponent, CardComponent,
  ButtonDirective, FormControlDirective, FormSelectDirective,
  ModalComponent, ModalHeaderComponent, ModalBodyComponent,
  ModalFooterComponent, ModalTitleDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [
    FormsModule,
    DatePipe,
    CardComponent, CardBodyComponent,
    ButtonDirective,
    FormControlDirective, FormSelectDirective,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent,
    ModalFooterComponent, ModalTitleDirective,
    IconDirective,
  ],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.scss',
})
export class CalendarComponent implements OnInit {
  private readonly authService         = inject(AuthService);
  private readonly toastService        = inject(ToastService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService   = inject(AssignmentsService);
  private readonly familyService        = inject(FamilyService);
  private readonly calendarService      = inject(CalendarService);

  // ── State ──────────────────────────────────────────────────────────────
  currentDate = new Date();
  daysGrid: { date: Date | null; isToday: boolean; events: CalendarEvent[] }[] = [];
  weekDays = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];

  events = signal<CalendarEvent[]>([]);
  isProfessional = false;
  assignedStudents = signal<any[]>([]);

  readonly todayDate = this.formatDateKey(new Date());

  // Month-Year Picker Popover State
  showMonthYearPicker = false;
  pickerYear = this.currentDate.getFullYear();
  monthsList = [
    { value: 0, abbr: 'Ene' },
    { value: 1, abbr: 'Feb' },
    { value: 2, abbr: 'Mar' },
    { value: 3, abbr: 'Abr' },
    { value: 4, abbr: 'May' },
    { value: 5, abbr: 'Jun' },
    { value: 6, abbr: 'Jul' },
    { value: 7, abbr: 'Ago' },
    { value: 8, abbr: 'Sep' },
    { value: 9, abbr: 'Oct' },
    { value: 10, abbr: 'Nov' },
    { value: 11, abbr: 'Dic' }
  ];

  // Modals
  showEventModal = false;
  showDetailModal = false;

  // Form State
  eventForm = {
    id: '',
    title: '',
    type: 'Tutoría' as 'Tutoría' | 'Clase' | 'Tarea',
    date: this.todayDate,
    time: '',
    description: '',
    targetScope: 'all' as 'all' | 'single',
    studentId: ''
  };

  selectedEvent = signal<CalendarEvent | null>(null);

  private readonly STORAGE_KEY = 'inclusion_calendar_events';

  ngOnInit(): void {
    const role = this.authService.getCurrentUser()?.role;
    this.isProfessional = role === UserRoles.Professional;
    this.loadEvents();

    if (this.isProfessional) {
      this.loadAssignedStudents();
    }
  }

  loadAssignedStudents(): void {
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
    ).subscribe({
      next: (persons) => {
        this.assignedStudents.set(persons.filter(p => p.isActive));
      },
      error: () => {
        this.toastService.error('Error al cargar alumnos asignados');
      }
    });
  }

  // ── Grid Generator ─────────────────────────────────────────────────────
  generateCalendarGrid(): void {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();

    const firstDayIndex = new Date(year, month, 1).getDay();
    const adjustedFirstDay = firstDayIndex === 0 ? 6 : firstDayIndex - 1;
    const totalDays = new Date(year, month + 1, 0).getDate();

    const today = new Date();
    const grid: typeof this.daysGrid = [];

    for (let i = 0; i < adjustedFirstDay; i++) {
      grid.push({ date: null, isToday: false, events: [] });
    }

    const allEvents = this.events();
    for (let day = 1; day <= totalDays; day++) {
      const date = new Date(year, month, day);
      const isToday = today.getDate() === day && today.getMonth() === month && today.getFullYear() === year;

      const dateStr = this.formatDateKey(date);
      const dayEvents = allEvents.filter(e => e.date === dateStr);

      grid.push({
        date,
        isToday,
        events: dayEvents.sort((a, b) => a.time.localeCompare(b.time))
      });
    }

    this.daysGrid = grid;
  }

  formatDateKey(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  // ── Navigation ─────────────────────────────────────────────────────────
  prevMonth(): void {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() - 1, 1);
    this.generateCalendarGrid();
  }

  nextMonth(): void {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 1);
    this.generateCalendarGrid();
  }

  toggleMonthYearPicker(): void {
    this.showMonthYearPicker = !this.showMonthYearPicker;
    this.pickerYear = this.currentDate.getFullYear();
  }

  selectPickerMonth(monthIndex: number): void {
    this.currentDate = new Date(this.pickerYear, monthIndex, 1);
    this.showMonthYearPicker = false;
    this.generateCalendarGrid();
  }

  applyMonthYearPicker(): void {
    this.currentDate = new Date(this.pickerYear, this.currentDate.getMonth(), 1);
    this.generateCalendarGrid();
  }

  get monthLabel(): string {
    const months = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    return `${months[this.currentDate.getMonth()]} ${this.currentDate.getFullYear()}`;
  }

  // ── Event Operations ───────────────────────────────────────────────────
  loadEvents(): void {
    this.calendarService.getEvents().subscribe({
      next: (eventsList) => {
        this.events.set(eventsList);
        // Cache to localStorage for offline robustness
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(eventsList));
        this.generateCalendarGrid();
      },
      error: () => {
        this.toastService.error('Error al cargar eventos del servidor. Usando datos locales.');
        const stored = localStorage.getItem(this.STORAGE_KEY);
        if (stored) {
          this.events.set(JSON.parse(stored));
        }
        this.generateCalendarGrid();
      }
    });
  }

  isDateInvalid(): boolean {
    if (!this.eventForm.date) return true;
    return this.eventForm.date < this.todayDate;
  }

  isFormInvalid(): boolean {
    if (!this.eventForm.title.trim() || !this.eventForm.date || !this.eventForm.time) return true;
    if (this.isDateInvalid()) return true;
    if (this.eventForm.targetScope === 'single' && !this.eventForm.studentId) return true;
    return false;
  }

  openCreateModal(date?: Date): void {
    if (!this.isProfessional) return;

    const chosenDateStr = date ? this.formatDateKey(date) : this.todayDate;
    const dateStr = chosenDateStr >= this.todayDate ? chosenDateStr : this.todayDate;

    this.eventForm = {
      id: '',
      title: '',
      type: 'Tutoría',
      date: dateStr,
      time: '12:00',
      description: '',
      targetScope: 'all',
      studentId: ''
    };
    this.showEventModal = true;
  }

  closeEventModal(): void {
    this.showEventModal = false;
  }

  saveEventSubmit(): void {
    if (!this.eventForm.title.trim() || !this.eventForm.date || !this.eventForm.time) {
      this.toastService.error('Por favor, completa los campos obligatorios.');
      return;
    }

    if (this.isDateInvalid()) {
      this.toastService.error('No se pueden agendar eventos en fechas pasadas.');
      return;
    }

    if (this.eventForm.targetScope === 'single' && !this.eventForm.studentId) {
      this.toastService.error('Por favor, selecciona un alumno para la actividad.');
      return;
    }

    const payload = {
      id: this.eventForm.id || null,
      title: this.eventForm.title.trim(),
      type: this.eventForm.type,
      date: this.eventForm.date,
      time: this.eventForm.time,
      description: this.eventForm.description.trim(),
      targetScope: this.eventForm.targetScope,
      studentId: this.eventForm.targetScope === 'single' ? this.eventForm.studentId : null
    };

    this.calendarService.saveEvent(payload).subscribe({
      next: (_savedEvent) => {
        this.toastService.success(this.eventForm.id ? 'Evento actualizado exitosamente.' : 'Evento agendado exitosamente.');
        this.loadEvents();
        this.showEventModal = false;
      },
      error: (err) => {
        const errorMsg = err?.error?.message || 'Error al guardar el evento en el servidor.';
        this.toastService.error(errorMsg);
      }
    });
  }

  openDetail(event: CalendarEvent, clickEvent: MouseEvent): void {
    clickEvent.stopPropagation();
    this.selectedEvent.set(event);
    this.showDetailModal = true;
  }

  closeDetailModal(): void {
    this.showDetailModal = false;
  }

  editSelectedEvent(): void {
    const ev = this.selectedEvent();
    if (!ev || !this.isProfessional) return;

    this.eventForm = {
      id: ev.id,
      title: ev.title,
      type: ev.type,
      date: ev.date,
      time: ev.time,
      description: ev.description || '',
      targetScope: ev.targetScope || 'all',
      studentId: ev.studentId || ''
    };
    this.showDetailModal = false;
    this.showEventModal = true;
  }

  deleteSelectedEvent(): void {
    const ev = this.selectedEvent();
    if (!ev || !this.isProfessional) return;

    this.calendarService.deleteEvent(ev.id).subscribe({
      next: () => {
        this.toastService.success('Evento eliminado.');
        this.loadEvents();
        this.showDetailModal = false;
      },
      error: () => {
        this.toastService.error('Error al eliminar el evento en el servidor.');
      }
    });
  }

  getEventTypeBadgeColor(type: string): string {
    switch (type) {
      case 'Tutoría': return 'primary';
      case 'Clase': return 'success';
      case 'Tarea': return 'warning';
      default: return 'secondary';
    }
  }
}
