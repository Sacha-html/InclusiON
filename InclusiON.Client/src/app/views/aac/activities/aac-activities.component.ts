import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ActivitiesService } from '@services/activities.service';
import {
  ActivityAssignmentResponse,
  ActivityAssignmentStatus,
  ActivityResponseResult,
  ActivityAttemptResponse
} from '@models';
import { AppRoutes } from '@shared/constants/app-routes';
import { VisualCardComponent } from '@shared/components/visual-card/visual-card.component';

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
      next: (data) => {
        // En "Mis Actividades", el alumno solo visualiza las actividades asignadas por su profesor a cargo (excluyendo plantillas del Roadmap)
        const teacherAssigned = (data ?? []).filter(
          (a) => !a.isTemplate && (a.roadmapOrder === null || a.roadmapOrder === undefined)
        );
        this.assignments.set(teacherAssigned);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      },
    });
  }

  openActivity(assignment: ActivityAssignmentResponse): void {
    if (!this.isPlayable(assignment)) return;
    this.router.navigate([AppRoutes.Aac.Activities, assignment.encryptedId]);
  }

  /** Retorna el último intento registrado para esta actividad */
  private getLatestAttempt(a: ActivityAssignmentResponse): ActivityAttemptResponse | null {
    if (!a.responses || a.responses.length === 0) return null;
    return [...a.responses].sort(
      (r1, r2) => new Date(r2.startedAt || 0).getTime() - new Date(r1.startedAt || 0).getTime()
    )[0];
  }

  /** Determina si la actividad se realizó pero tuvo errores o resultado parcial */
  hasErrors(a: ActivityAssignmentResponse): boolean {
    const latest = this.getLatestAttempt(a);
    if (!latest) return false;
    if (latest.result === ActivityResponseResult.Parcial || latest.result === ActivityResponseResult.Fallido) {
      return true;
    }
    if (latest.successPercentage !== undefined && latest.successPercentage !== null && latest.successPercentage < 80) {
      return true;
    }
    return false;
  }

  /** Determina si la actividad fue completada exitosamente sin errores */
  isSuccess(a: ActivityAssignmentResponse): boolean {
    if (a.status !== ActivityAssignmentStatus.Completada) return false;
    return !this.hasErrors(a);
  }

  /** Determina si la actividad fue iniciada pero no completada */
  isInProgress(a: ActivityAssignmentResponse): boolean {
    return a.status === ActivityAssignmentStatus.EnProgreso;
  }

  /** Variante de color:
   * - 'success': Verde cuando se realizó y está bien
   * - 'warning': Naranja cuando se realizó pero no se completó (incompleta) o tiene errores
   * - 'primary': Azul accesible cuando está recién asignada/pendiente
   * - 'muted': Gris cuando está cancelada
   */
  statusVariant(a: ActivityAssignmentResponse): 'success' | 'warning' | 'danger' | 'primary' | 'muted' {
    if (a.status === ActivityAssignmentStatus.Cancelada) return 'muted';
    if (this.isSuccess(a)) return 'success';
    if (this.isInProgress(a) || this.hasErrors(a)) return 'warning';
    return 'primary';
  }

  /** Texto de la insignia (badge) */
  statusLabel(a: ActivityAssignmentResponse): string {
    if (a.status === ActivityAssignmentStatus.Cancelada) return ActivityAssignmentStatus.Cancelada;
    if (this.isSuccess(a)) return '¡Completada!';
    if (this.hasErrors(a)) return 'Para repasar';
    if (this.isInProgress(a)) return 'Incompleta';
    return 'Por realizar';
  }

  /** Subtítulo descriptivo y de ayuda */
  getActivitySubtitle(a: ActivityAssignmentResponse): string {
    if (a.status === ActivityAssignmentStatus.Cancelada) return 'Actividad cancelada';
    const latest = this.getLatestAttempt(a);
    if (this.isSuccess(a)) {
      return latest?.successPercentage != null
        ? `¡Excelente trabajo! • ${latest.successPercentage}% de aciertos`
        : '¡Excelente trabajo! • Completada con éxito';
    }
    if (this.hasErrors(a)) {
      return latest?.successPercentage != null
        ? `${latest.successPercentage}% de aciertos • Tocá para reintentar`
        : 'Tiene respuestas para corregir • Tocá para reintentar';
    }
    if (this.isInProgress(a)) {
      return 'Empezada • Tocá para terminarla';
    }
    return 'Asignada por tu profesor • Tocá para comenzar';
  }

  /** Icono descriptivo */
  getActivityIcon(a: ActivityAssignmentResponse): string {
    if (a.status === ActivityAssignmentStatus.Cancelada) return 'cilBan';
    if (this.isSuccess(a)) return 'cilCheckCircle';
    if (this.hasErrors(a)) return 'cilWarning';
    if (this.isInProgress(a)) return 'cilReload';
    return 'cilTask';
  }

  /** Define si el alumno puede hacer clic para jugar/reintentar */
  isPlayable(a: ActivityAssignmentResponse): boolean {
    if (a.status === ActivityAssignmentStatus.Cancelada) return false;
    if (this.isSuccess(a)) return false;
    return true;
  }

  /** Marca si la actividad está completada con éxito para estilo finalizado */
  isCompleted(a: ActivityAssignmentResponse): boolean {
    return this.isSuccess(a);
  }
}
