import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { ProfessionalsService, AssignmentsService, ActivitiesService, ToastService } from '@services';
import { ProfessionalPersonResponse, ActivityAssignmentResponse, ActivityAttemptResponse, ActivityAssignmentStatus, ActivityResponseResult } from '@models';
import { switchMap } from 'rxjs';
import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  SpinnerComponent,
  BadgeComponent,
  TableDirective,
  ButtonDirective,
  ProgressComponent,
  ProgressBarComponent
} from '@coreui/angular';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';

@Component({
  selector: 'app-evaluations',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    DecimalPipe,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ColComponent,
    RowComponent,
    SpinnerComponent,
    BadgeComponent,
    TableDirective,
    ButtonDirective,
    ProgressComponent,
    ProgressBarComponent,
    ActorAvatarComponent
  ],
  templateUrl: './evaluations.component.html',
  styleUrl: './evaluations.component.scss'
})
export class EvaluationsComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly toastService = inject(ToastService);

  persons = signal<ProfessionalPersonResponse[]>([]);
  selectedPerson = signal<ProfessionalPersonResponse | null>(null);
  assignments = signal<ActivityAssignmentResponse[]>([]);
  
  isLoadingPersons = signal<boolean>(true);
  isLoadingAssignments = signal<boolean>(false);

  // Expanded attempts mapping
  expandedAssignments = signal<Set<number>>(new Set());

  // Computed metrics
  completedCount = signal<number>(0);
  inProgressCount = signal<number>(0);
  pendingCount = signal<number>(0);
  averageSuccessRate = signal<number>(0);
  averageTimeSpent = signal<number>(0);
  totalAttempts = signal<number>(0);

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.isLoadingPersons.set(true);
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
    ).subscribe({
      next: (data) => {
        this.persons.set(data.filter(p => p.isActive));
        this.isLoadingPersons.set(false);
      },
      error: () => {
        this.isLoadingPersons.set(false);
        this.toastService.error('Error al cargar la lista de alumnos');
      }
    });
  }

  selectPerson(person: ProfessionalPersonResponse): void {
    this.selectedPerson.set(person);
    this.expandedAssignments.set(new Set());
    this.loadAssignments(person.personId);
  }

  loadAssignments(personId: string): void {
    this.isLoadingAssignments.set(true);
    this.activitiesService.getPersonAssignments(personId).subscribe({
      next: (data) => {
        this.assignments.set(data);
        this.calculateMetrics(data);
        this.isLoadingAssignments.set(false);
      },
      error: () => {
        this.isLoadingAssignments.set(false);
        this.toastService.error('Error al cargar las evaluaciones del alumno');
      }
    });
  }

  calculateMetrics(data: ActivityAssignmentResponse[]): void {
    let completed = 0;
    let inProgress = 0;
    let pending = 0;
    let totalSuccess = 0;
    let totalTime = 0;
    let responseCount = 0;

    data.forEach(a => {
      if (a.status === 'Completada') completed++;
      else if (a.status === 'EnProgreso') inProgress++;
      else pending++;

      if (a.responses) {
        a.responses.forEach(r => {
          responseCount++;
          if (r.successPercentage !== undefined && r.successPercentage !== null) {
            totalSuccess += Number(r.successPercentage);
          }
          if (r.timeSpentSeconds) {
            totalTime += r.timeSpentSeconds;
          }
        });
      }
    });

    this.completedCount.set(completed);
    this.inProgressCount.set(inProgress);
    this.pendingCount.set(pending);
    this.totalAttempts.set(responseCount);
    this.averageSuccessRate.set(responseCount > 0 ? (totalSuccess / responseCount) : 0);
    this.averageTimeSpent.set(responseCount > 0 ? (totalTime / responseCount) : 0);
  }

  formatTime(seconds: number | undefined): string {
    if (!seconds) return '—';
    if (seconds < 60) return `${seconds}s`;
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return secs > 0 ? `${mins}m ${secs}s` : `${mins}m`;
  }

  getMaxSuccess(responses: ActivityAttemptResponse[] | undefined): number {
    if (!responses || responses.length === 0) return 0;
    return Math.max(...responses.map(r => r.successPercentage !== null && r.successPercentage !== undefined ? Number(r.successPercentage) : 0));
  }

  isExpanded(assignmentId: number): boolean {
    return this.expandedAssignments().has(assignmentId);
  }

  toggleExpanded(assignmentId: number): void {
    const current = new Set(this.expandedAssignments());
    if (current.has(assignmentId)) {
      current.delete(assignmentId);
    } else {
      current.add(assignmentId);
    }
    this.expandedAssignments.set(current);
  }

  getResultBadgeColor(result: ActivityResponseResult | string | undefined): string {
    if (!result) return 'secondary';
    switch (result) {
      case 'Exito':
      case ActivityResponseResult.Exito:
        return 'success';
      case 'Parcial':
      case ActivityResponseResult.Parcial:
        return 'warning';
      case 'Fallido':
      case ActivityResponseResult.Fallido:
        return 'danger';
      default:
        return 'secondary';
    }
  }

  getResultLabel(result: ActivityResponseResult | string | undefined): string {
    if (!result) return 'Pendiente';
    switch (result) {
      case 'Exito':
      case ActivityResponseResult.Exito:
        return 'Éxito';
      case 'Parcial':
      case ActivityResponseResult.Parcial:
        return 'Parcial';
      case 'Fallido':
      case ActivityResponseResult.Fallido:
        return 'Fallido';
      default:
        return result.toString();
    }
  }

  getAssignmentStatusColor(status: ActivityAssignmentStatus | string): string {
    switch (status) {
      case 'Completada': return 'success';
      case 'EnProgreso': return 'warning';
      case 'Pendiente': return 'secondary';
      default: return 'info';
    }
  }

  getAssignmentStatusLabel(status: ActivityAssignmentStatus | string): string {
    switch (status) {
      case 'Completada': return 'Completada';
      case 'EnProgreso': return 'En progreso';
      case 'Pendiente': return 'Pendiente';
      default: return status.toString();
    }
  }

  getFrustrationEmoji(level: number | undefined): string {
    if (!level) return '—';
    if (level <= 1) return '😊 (Muy bajo)';
    if (level === 2) return '🙂 (Bajo)';
    if (level === 3) return '😐 (Moderado)';
    if (level === 4) return '🙁 (Alto)';
    return '😫 (Muy alto)';
  }
}
