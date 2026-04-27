import { Component } from '@angular/core';
import { VisualCardComponent } from '../../../shared/components/visual-card/visual-card.component';

@Component({
  selector: 'app-aac-activities',
  standalone: true,
  imports: [VisualCardComponent],
  templateUrl: './aac-activities.component.html',
  styleUrl: './aac-activities.component.scss'
})
export class AacActivitiesComponent {
  activities = [
    {
      id: 1,
      title: 'Ejercicios de motricidad',
      time: '09:00 AM',
      icon: 'cilPuzzle',
      color: 'var(--a11y-success, #4CAF50)',
      status: 'Completado',
      statusColor: 'var(--a11y-success, #4CAF50)'
    },
    {
      id: 2,
      title: 'Lectura interactiva',
      time: '10:30 AM',
      icon: 'cilBook',
      color: 'var(--a11y-primary, #2196F3)',
      status: 'En progreso',
      statusColor: 'var(--a11y-warning, #FF9800)'
    },
    {
      id: 3,
      title: 'Juego de memoria',
      time: '02:00 PM',
      icon: 'cilLightbulb',
      color: 'var(--a11y-nav-talk, #9C27B0)',
      status: 'Pendiente',
      statusColor: 'var(--a11y-text-muted, #9E9E9E)'
    }
  ];
}
