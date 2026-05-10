import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';
import { FamilyService, ToastService } from '@services';
import { ActivitiesService } from '@services/activities.service';
import { ActivityAssignmentResponse, ActivityAssignmentStatus } from '@models/responses/activity.response';
import { FamilyPersonSummaryResponse } from '../../../models';
import {
  BadgeComponent,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-family-activities',
  standalone: true,
  imports: [
    ActorAvatarComponent,
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    SpinnerComponent,
    AlertComponent,
  ],
  templateUrl: './family-activities.component.html',
  styleUrl: './family-activities.component.scss',
})
export class FamilyActivitiesComponent implements OnInit {
  private readonly familyService      = inject(FamilyService);
  private readonly activitiesService  = inject(ActivitiesService);
  private readonly toastService       = inject(ToastService);

  persons              = signal<FamilyPersonSummaryResponse[]>([]);
  selectedPersonId     = signal<string | null>(null);
  assignments          = signal<ActivityAssignmentResponse[]>([]);
  isLoading            = signal(true);
  isLoadingAssignments = signal(false);
  hasError             = signal(false);

  // Filtro por estado
  statusFilter = signal<ActivityAssignmentStatus | 'all'>('all');

  readonly filteredAssignments = computed(() => {
    const filter = this.statusFilter();
    const all    = this.assignments();
    if (filter === 'all') return all;
    return all.filter(a => a.status === filter);
  });

  readonly selectedPerson = computed(() =>
    this.persons().find(p => p.personId === this.selectedPersonId())
  );

  readonly counts = computed(() => {
    const all = this.assignments();
    return {
      all:        all.length,
      pendiente:  all.filter(a => a.status === ActivityAssignmentStatus.Pendiente).length,
      enProgreso: all.filter(a => a.status === ActivityAssignmentStatus.EnProgreso).length,
      completada: all.filter(a => a.status === ActivityAssignmentStatus.Completada).length,
      cancelada:  all.filter(a => a.status === ActivityAssignmentStatus.Cancelada).length,
    };
  });

  readonly ActivityAssignmentStatus = ActivityAssignmentStatus;

  ngOnInit(): void {
    this.familyService.getDashboard().subscribe({
      next: (dashboard) => {
        this.persons.set(dashboard.persons);
        this.isLoading.set(false);
        if (dashboard.persons.length === 1) {
          this.selectPerson(dashboard.persons[0].personId);
        }
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      },
    });
  }

  selectPerson(personId: string): void {
    if (this.selectedPersonId() === personId) return;
    this.selectedPersonId.set(personId);
    this.statusFilter.set('all');
    this.isLoadingAssignments.set(true);

    this.activitiesService.getPersonAssignments(personId).subscribe({
      next: (data) => {
        // Orden: Pendiente → EnProgreso → Completada → Cancelada
        const order: Record<ActivityAssignmentStatus, number> = {
          [ActivityAssignmentStatus.Pendiente]:  0,
          [ActivityAssignmentStatus.EnProgreso]: 1,
          [ActivityAssignmentStatus.Completada]: 2,
          [ActivityAssignmentStatus.Cancelada]:  3,
        };
        const sorted = [...data].sort((a, b) => order[a.status] - order[b.status]);
        this.assignments.set(sorted);
        this.isLoadingAssignments.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar las actividades');
        this.isLoadingAssignments.set(false);
      },
    });
  }

  setFilter(filter: ActivityAssignmentStatus | 'all'): void {
    this.statusFilter.set(filter);
  }

  statusColor(status: ActivityAssignmentStatus): string {
    switch (status) {
      case ActivityAssignmentStatus.Completada:  return 'success';
      case ActivityAssignmentStatus.EnProgreso:  return 'info';
      case ActivityAssignmentStatus.Cancelada:   return 'secondary';
      default:                                    return 'warning';
    }
  }

  statusLabel(status: ActivityAssignmentStatus): string {
    switch (status) {
      case ActivityAssignmentStatus.Completada:  return ActivityAssignmentStatus.Completada;
      case ActivityAssignmentStatus.EnProgreso:  return 'En progreso';
      case ActivityAssignmentStatus.Cancelada:   return ActivityAssignmentStatus.Cancelada;
      default:                                    return ActivityAssignmentStatus.Pendiente;
    }
  }

  lastScore(a: ActivityAssignmentResponse): number | null {
    if (!a.responses.length) return null;
    const sorted = [...a.responses].sort(
      (x, y) => new Date(y.completedAt ?? 0).getTime() - new Date(x.completedAt ?? 0).getTime()
    );
    return sorted[0].successPercentage ?? null;
  }

  scoreColor(score: number): string {
    if (score >= 80) return 'success';
    if (score >= 50) return 'warning';
    return 'danger';
  }

  formatDate(date?: string): string {
    if (!date) return '—';
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }
}
