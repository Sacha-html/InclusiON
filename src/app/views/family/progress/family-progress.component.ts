import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';
import { FamilyService, ToastService } from '@services';
import { ActivitiesService } from '@services/activities.service';
import {
  ActivityAssignmentResponse,
  ActivityAssignmentStatus,
  ActivityAttemptResponse,
  ActivityResponseResult,
} from '@models/responses/activity.response';
import { FamilyPersonSummaryResponse } from '../../../models';
import {
  BadgeComponent,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  ProgressBarComponent,
  ProgressComponent,
  RowComponent,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';

interface PersonStats {
  total:             number;
  completadas:       number;
  enProgreso:        number;
  pendientes:        number;
  avgScore:          number | null;
  totalAttempts:     number;
  totalEvaluations:  number;
  completedEvals:    number;
}

interface CompletionEntry {
  activityTitle: string;
  completedAt:   string;
  score:         number | null;
  result:        ActivityResponseResult | undefined;
}

@Component({
  selector: 'app-family-progress',
  standalone: true,
  imports: [
    ActorAvatarComponent,
    AlertComponent,
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ColComponent,
    ProgressBarComponent,
    ProgressComponent,
    RowComponent,
    SpinnerComponent,
  ],
  templateUrl: './family-progress.component.html',
  styleUrl: './family-progress.component.scss',
})
export class FamilyProgressComponent implements OnInit {
  private readonly familyService      = inject(FamilyService);
  private readonly activitiesService  = inject(ActivitiesService);
  private readonly toastService       = inject(ToastService);

  persons              = signal<FamilyPersonSummaryResponse[]>([]);
  selectedPersonId     = signal<string | null>(null);
  assignments          = signal<ActivityAssignmentResponse[]>([]);
  isLoading            = signal(true);
  isLoadingAssignments = signal(false);
  hasError             = signal(false);

  readonly selectedPerson = computed(() =>
    this.persons().find(p => p.personId === this.selectedPersonId())
  );

  readonly stats = computed((): PersonStats => {
    const all = this.assignments();
    const completadas  = all.filter(a => a.status === ActivityAssignmentStatus.Completada);
    const enProgreso   = all.filter(a => a.status === ActivityAssignmentStatus.EnProgreso);
    const pendientes   = all.filter(a => a.status === ActivityAssignmentStatus.Pendiente);
    const evals        = all.filter(a => a.isEvaluationActivity);
    const completedEvals = evals.filter(a => a.status === ActivityAssignmentStatus.Completada);

    // avg score: last attempt of each completada
    const scores = completadas
      .map(a => this.lastScore(a))
      .filter((s): s is number => s !== null);
    const avgScore = scores.length
      ? Math.round(scores.reduce((sum, s) => sum + s, 0) / scores.length)
      : null;

    const totalAttempts = all.reduce((sum, a) => sum + a.responses.length, 0);

    return {
      total:            all.length,
      completadas:      completadas.length,
      enProgreso:       enProgreso.length,
      pendientes:       pendientes.length,
      avgScore,
      totalAttempts,
      totalEvaluations: evals.length,
      completedEvals:   completedEvals.length,
    };
  });

  readonly completionPercent = computed(() => {
    const s = this.stats();
    return s.total ? Math.round((s.completadas / s.total) * 100) : 0;
  });

  readonly recentCompletions = computed((): CompletionEntry[] => {
    const entries: CompletionEntry[] = [];
    for (const a of this.assignments()) {
      for (const r of a.responses) {
        if (r.completedAt) {
          entries.push({
            activityTitle: a.activityTitle,
            completedAt:   r.completedAt,
            score:         r.successPercentage ?? null,
            result:        r.result,
          });
        }
      }
    }
    return entries
      .sort((x, y) => new Date(y.completedAt).getTime() - new Date(x.completedAt).getTime())
      .slice(0, 15);
  });

  readonly ActivityResponseResult = ActivityResponseResult;

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
    this.isLoadingAssignments.set(true);
    this.assignments.set([]);

    this.activitiesService.getPersonAssignments(personId).subscribe({
      next: (data) => {
        this.assignments.set(data);
        this.isLoadingAssignments.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar el progreso');
        this.isLoadingAssignments.set(false);
      },
    });
  }

  private lastScore(a: ActivityAssignmentResponse): number | null {
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

  resultLabel(result: ActivityResponseResult | undefined): string {
    switch (result) {
      case ActivityResponseResult.Exito:   return 'Éxito';
      case ActivityResponseResult.Parcial: return 'Parcial';
      case ActivityResponseResult.Fallido: return 'Fallido';
      default:                              return '—';
    }
  }

  resultColor(result: ActivityResponseResult | undefined): string {
    switch (result) {
      case ActivityResponseResult.Exito:   return 'success';
      case ActivityResponseResult.Parcial: return 'warning';
      case ActivityResponseResult.Fallido: return 'danger';
      default:                              return 'secondary';
    }
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }
}
