import { Component } from '@angular/core';
import { VisualCardComponent } from '../../../shared/components/visual-card/visual-card.component';

@Component({
  selector: 'app-aac-calendar',
  standalone: true,
  imports: [VisualCardComponent],
  template: `
    <div class="aac-calendar">
      <h1 class="page-title">Mi Calendario</h1>

      <div class="today-banner">
        <span class="today-label">Hoy es</span>
        <span class="today-date">{{ todayFormatted }}</span>
      </div>

      <section class="upcoming">
        <h2 class="section-title">Proximos eventos</h2>
        <div class="events-list">
          @for (event of events; track event.id) {
            <app-visual-card
              [title]="event.title"
              [subtitle]="event.date"
              [icon]="event.icon"
              [accentColor]="event.color"
              [interactive]="false"
            />
          }
        </div>
      </section>
    </div>
  `,
  styles: [`
    .aac-calendar {
      padding: 8px;
    }

    .page-title {
      font-size: 28px;
      font-weight: 700;
      color: var(--aac-text, #1a1a1a);
      margin: 0 0 24px;
      text-align: center;
    }

    .today-banner {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 24px;
      background: linear-gradient(135deg, #4CAF50, #2E7D32);
      border-radius: 24px;
      color: white;
      margin-bottom: 24px;
    }

    .today-label {
      font-size: 18px;
      opacity: 0.9;
    }

    .today-date {
      font-size: 28px;
      font-weight: 700;
      margin-top: 4px;
    }

    .section-title {
      font-size: 22px;
      font-weight: 600;
      color: var(--aac-text, #1a1a1a);
      margin: 0 0 16px;
    }

    .events-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    :host-context([data-profile="high-contrast"]) {
      .page-title,
      .section-title {
        color: #fff;
      }

      .today-banner {
        border: 3px solid #fff;
      }
    }

    :host-context([data-color-mode="dark"]) {
      .page-title,
      .section-title {
        color: #f5f5f5;
      }
    }
  `]
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
      color: '#2196F3'
    },
    {
      id: 2,
      title: 'Actividad grupal',
      date: 'Miercoles, 3:00 PM',
      icon: 'cilPeople',
      color: '#9C27B0'
    },
    {
      id: 3,
      title: 'Evaluacion mensual',
      date: 'Viernes, 11:00 AM',
      icon: 'cilClipboard',
      color: '#FF9800'
    }
  ];
}
