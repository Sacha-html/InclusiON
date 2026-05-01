import { Component, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { inject } from '@angular/core';
import { ActivitiesService } from '@services/activities.service';
import { ToastService } from '@services';
import { ActivityAssignmentResponse, ActivityAttemptResponse } from '@models/responses/activity.response';
import {
  BadgeComponent,
  ButtonDirective,
  SpinnerComponent,
} from '@coreui/angular';

type StatusColor = 'warning' | 'info' | 'success' | 'danger' | 'secondary';

@Component({
  selector: 'app-professional-activities-tab',
  standalone: true,
  imports: [
    CommonModule,
    BadgeComponent,
    ButtonDirective,
    SpinnerComponent,
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

  statusColor(status: string): StatusColor {
    switch (status) {
      case 'Completada':  return 'success';
      case 'EnProgreso':  return 'info';
      case 'Vencida':     return 'danger';
      default:            return 'warning';   // Pendiente
    }
  }

  statusLabel(status: string): string {
    switch (status) {
      case 'Completada': return 'Completada';
      case 'EnProgreso': return 'En progreso';
      case 'Vencida':    return 'Vencida';
      default:           return 'Pendiente';
    }
  }

  lastAttempt(a: ActivityAssignmentResponse): ActivityAttemptResponse | null {
    return a.responses.length > 0 ? a.responses[0] : null;
  }

  isOverdue(a: ActivityAssignmentResponse): boolean {
    if (!a.dueDate || a.status === 'Completada') return false;
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
