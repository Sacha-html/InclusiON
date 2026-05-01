import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ActivitiesService } from '@services/activities.service';
import { ActivityAssignmentResponse } from '@models/responses/activity.response';
import { VisualCardComponent } from '../../../shared/components/visual-card/visual-card.component';

@Component({
  selector: 'app-aac-activities',
  standalone: true,
  imports: [VisualCardComponent],
  templateUrl: './aac-activities.component.html',
  styleUrl: './aac-activities.component.scss',
})
export class AacActivitiesComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly router            = inject(Router);

  assignments = signal<ActivityAssignmentResponse[]>([]);
  isLoading   = signal(true);
  hasError    = signal(false);

  ngOnInit(): void {
    this.activitiesService.getMyAssignments().subscribe({
      next:  (data) => { this.assignments.set(data); this.isLoading.set(false); },
      error: ()     => { this.hasError.set(true);    this.isLoading.set(false); },
    });
  }

  openActivity(assignment: ActivityAssignmentResponse): void {
    if (assignment.status === 'Completada') return;
    this.router.navigate(['/app/activities', assignment.id]);
  }

  statusColor(status: string): string {
    return status === 'Completada'  ? 'var(--a11y-success, #4CAF50)'
         : status === 'EnProgreso'  ? 'var(--a11y-warning, #FF9800)'
         :                            'var(--a11y-primary, #2196F3)';
  }

  statusLabel(status: string): string {
    return status === 'Completada' ? 'Completada'
         : status === 'EnProgreso' ? 'En progreso'
         :                           'Pendiente';
  }

  isPlayable(assignment: ActivityAssignmentResponse): boolean {
    return assignment.status !== 'Completada' && assignment.status !== 'Cancelada';
  }
}
