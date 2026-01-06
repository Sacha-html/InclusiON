import { Component } from '@angular/core';
import { VisualCardComponent } from '../../../shared/components/visual-card/visual-card.component';

@Component({
  selector: 'app-aac-activities',
  standalone: true,
  imports: [VisualCardComponent],
  template: `
    <div class="aac-activities">
      <h1 class="page-title">Mis Actividades</h1>

      <div class="activities-list">
        @for (activity of activities; track activity.id) {
          <app-visual-card
            [title]="activity.title"
            [subtitle]="activity.time"
            [icon]="activity.icon"
            [accentColor]="activity.color"
            [badge]="activity.status"
            [badgeColor]="activity.statusColor"
          />
        }

        @empty {
          <div class="empty-state">
            <p>No hay actividades programadas</p>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .aac-activities {
      padding: 8px;
    }

    .page-title {
      font-size: 28px;
      font-weight: 700;
      color: var(--aac-text, #1a1a1a);
      margin: 0 0 24px;
      text-align: center;
    }

    .activities-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .empty-state {
      text-align: center;
      padding: 48px 24px;
      background: white;
      border-radius: 24px;
      color: #666;
      font-size: 20px;
    }

    :host-context([data-profile="high-contrast"]) .page-title {
      color: #fff;
    }

    :host-context([data-color-mode="dark"]) {
      .page-title {
        color: #f5f5f5;
      }

      .empty-state {
        background: #2a2a3e;
        color: #aaa;
      }
    }
  `]
})
export class AacActivitiesComponent {
  activities = [
    {
      id: 1,
      title: 'Ejercicios de motricidad',
      time: '09:00 AM',
      icon: 'cilPuzzle',
      color: '#4CAF50',
      status: 'Completado',
      statusColor: '#4CAF50'
    },
    {
      id: 2,
      title: 'Lectura interactiva',
      time: '10:30 AM',
      icon: 'cilBook',
      color: '#2196F3',
      status: 'En progreso',
      statusColor: '#FF9800'
    },
    {
      id: 3,
      title: 'Juego de memoria',
      time: '02:00 PM',
      icon: 'cilLightbulb',
      color: '#9C27B0',
      status: 'Pendiente',
      statusColor: '#9E9E9E'
    }
  ];
}
