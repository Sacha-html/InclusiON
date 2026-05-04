import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ActivitiesService } from '@services/activities.service';
import { ActivityAssignmentResponse, ActivityAssignmentStatus } from '@models/responses/activity.response';
import { AppRoutes } from '@shared/constants/app-routes';
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
    if (assignment.status === ActivityAssignmentStatus.Completada) return;
    this.router.navigate([AppRoutes.Aac.Activities, assignment.id]);
  }

  statusColor(status: ActivityAssignmentStatus): string {
    return status === ActivityAssignmentStatus.Completada ? 'var(--a11y-success, #4CAF50)'
         : status === ActivityAssignmentStatus.EnProgreso ? 'var(--a11y-warning, #FF9800)'
         : status === ActivityAssignmentStatus.Cancelada  ? 'var(--a11y-text-muted, #9E9E9E)'
         :                                                   'var(--a11y-primary, #2196F3)';
  }

  statusLabel(status: ActivityAssignmentStatus): string {
    return status === ActivityAssignmentStatus.Completada ? ActivityAssignmentStatus.Completada
         : status === ActivityAssignmentStatus.EnProgreso ? 'En progreso'
         : status === ActivityAssignmentStatus.Cancelada  ? ActivityAssignmentStatus.Cancelada
         :                                                  ActivityAssignmentStatus.Pendiente;
  }

  isPlayable(assignment: ActivityAssignmentResponse): boolean {
    return assignment.status !== ActivityAssignmentStatus.Completada && assignment.status !== ActivityAssignmentStatus.Cancelada;
  }
}
