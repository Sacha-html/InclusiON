import { Component } from '@angular/core';
import { VisualCardComponent } from '@shared/components/visual-card/visual-card.component';

@Component({
  selector: 'app-aac-calendar',
  standalone: true,
  imports: [VisualCardComponent],
  templateUrl: './aac-calendar.component.html',
  styleUrl: './aac-calendar.component.scss'
})
export class AacCalendarComponent {
  get todayFormatted(): string {
    return new Date().toLocaleDateString('es-ES', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });
  }

  events = [
    {
      id: 1,
      title: 'Sesion con terapeuta',
      date: 'Manana, 10:00 AM',
      icon: 'cilMedicalCross',
      color: 'var(--a11y-primary, #2196F3)'
    },
    {
      id: 2,
      title: 'Actividad grupal',
      date: 'Miercoles, 3:00 PM',
      icon: 'cilPeople',
      color: 'var(--a11y-nav-talk, #9C27B0)'
    },
    {
      id: 3,
      title: 'Evaluacion mensual',
      date: 'Viernes, 11:00 AM',
      icon: 'cilClipboard',
      color: 'var(--a11y-warning, #FF9800)'
    }
  ];
}
