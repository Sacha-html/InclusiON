import { Component, Input, OnInit, signal } from '@angular/core';
import { inject } from '@angular/core';
import { ActivitiesService } from '@services/activities.service';
import { ToastService } from '@services';
import { ActivityAssignmentResponse, ActivityAttemptResponse, ActivityAssignmentStatus } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  SpinnerComponent,
} from '@coreui/angular';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';

type StatusColor = 'warning' | 'info' | 'success' | 'danger' | 'secondary';

@Component({
  selector: 'app-professional-activities-tab',
  standalone: true,
  imports: [
    BadgeComponent,
    ButtonDirective,
    SpinnerComponent,
    EmptyStateComponent,
  ],
  templateUrl: './professional-activities-tab.component.html',
})
export class ProfessionalActivitiesTabComponent implements OnInit {
  @Input({ required: true }) personId!: string;

  private readonly activitiesService = inject(ActivitiesService);
  private readonly toastService      = inject(ToastService);

  assignments = signal<ActivityAssignmentResponse[]>([]);
  isLoading   = signal(true);
  hasError    = signal(false);
  expandedId  = signal<number | null>(null);

  ngOnInit(): void {
    this.activitiesService.getPersonAssignments(this.personId).subscribe({
      next:  (data) => { this.assignments.set(data); this.isLoading.set(false); },
      error: ()     => { this.hasError.set(true);    this.isLoading.set(false); },
    });
  }

  statusColor(status: ActivityAssignmentStatus): StatusColor {
    switch (status) {
      case ActivityAssignmentStatus.Completada:  return 'success';
      case ActivityAssignmentStatus.EnProgreso:  return 'info';
      case ActivityAssignmentStatus.Cancelada:   return 'secondary';
      default:                                   return 'warning';   // Pendiente
    }
  }

  statusLabel(status: ActivityAssignmentStatus): string {
    switch (status) {
      case ActivityAssignmentStatus.Completada: return ActivityAssignmentStatus.Completada;
      case ActivityAssignmentStatus.EnProgreso: return 'En progreso';
      case ActivityAssignmentStatus.Cancelada:  return ActivityAssignmentStatus.Cancelada;
      default:                                  return ActivityAssignmentStatus.Pendiente;
    }
  }

  lastAttempt(a: ActivityAssignmentResponse): ActivityAttemptResponse | null {
    return a.responses.length > 0 ? a.responses[0] : null;
  }

  isOverdue(a: ActivityAssignmentResponse): boolean {
    if (!a.dueDate || a.status === ActivityAssignmentStatus.Completada) return false;
    return new Date(a.dueDate) < new Date();
  }

  formatDate(date?: string): string {
    if (!date) return '—';
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }

  formatTime(seconds?: number): string {
    if (!seconds) return '—';
    if (seconds < 60) return `${seconds}s`;
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return s > 0 ? `${m}m ${s}s` : `${m}m`;
  }

  toggleExpand(id: number): void {
    this.expandedId.set(this.expandedId() === id ? null : id);
  }
}
