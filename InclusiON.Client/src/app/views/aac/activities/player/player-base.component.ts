import { Directive, ElementRef, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import { ActivitiesService } from '@services/activities.service';
import { ActivityAssignmentResponse } from '@models';
import { PlayerResult } from './player.models';

export type PlayerPhase = 'intro' | 'playing' | 'result';

/**
 * Base abstracta para todos los players de actividad.
 * Centraliza la lógica de startResponse / completeResponse / timer.
 * Cada player concreto hereda de aquí y solo implementa la fase "playing".
 */
@Directive()
export abstract class PlayerBaseComponent {
  @Input({ required: true }) assignment!: ActivityAssignmentResponse;
  @Output() completed = new EventEmitter<void>();

  protected readonly activitiesService = inject(ActivitiesService);
  private readonly el = inject(ElementRef<HTMLElement>);

  // Estado compartido
  phase       = signal<PlayerPhase>('intro');
  isLoading   = signal(false);
  hasError    = signal(false);
  errorMsg    = signal('');
  responseId  = signal<string | null>(null);
  isCorrect   = signal<boolean | null>(null);
  canRetry    = signal(true);

  private _startTime = 0;
  private lastResult: PlayerResult | null = null;
  private readonly completedResponseIds = new Set<string>();

  /** Segundos transcurridos desde que inició la fase playing. */
  get elapsedSeconds(): number {
    return Math.round((Date.now() - this._startTime) / 1000);
  }

  // ── Fase intro → playing ──────────────────────────────────────────────────
  startActivity(): void {
    if (!this.assignment?.encryptedId || this.assignment?.id === 0) {
      // Modo juego directo (plantilla del Roadmap sin asignación persistida)
      this._startTime = Date.now();
      this.phase.set('playing');
      setTimeout(() => {
        const heading = this.el.nativeElement.querySelector('.game-instruction, [role="heading"]') as HTMLElement;
        heading?.focus();
      }, 80);
      return;
    }

    this.isLoading.set(true);
    this.activitiesService.startResponse(this.assignment.encryptedId).subscribe({
      next: (updated) => {
        const responses = [...(updated.responses ?? [])];
        const latest    = responses.sort(
          (a, b) => new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
        )[0];
        this.responseId.set(latest?.encryptedId ?? null);
        this.canRetry.set(updated.canRetry);
        this._startTime = Date.now();
        this.isLoading.set(false);
        this.phase.set('playing');
        setTimeout(() => {
          const heading = this.el.nativeElement.querySelector('.game-instruction, [role="heading"]') as HTMLElement;
          heading?.focus();
        }, 80);
      },
      error: () => { this.isLoading.set(false); this.hasError.set(true); this.errorMsg.set('No se pudo iniciar la actividad. Volvé a intentar.'); },
    });
  }

  // ── Fase result → guardar y salir ─────────────────────────────────────────
  finishActivity(result: PlayerResult): void {
    this.lastResult = result;
    // Guardar progreso local para desbloqueo de niveles del Roadmap
    if (this.assignment?.activityId) {
      try {
        localStorage.setItem(
          'roadmap_progress_' + this.assignment.activityId,
          JSON.stringify({
            score: result.successPercentage,
            passed: result.successPercentage >= 60,
            completedAt: new Date().toISOString(),
          })
        );
      } catch {}
    }

    const responseId = this.responseId();
    if (responseId === null || this.assignment?.id === 0) {
      this.completed.emit();
      return;
    }
    this.completeCurrentResponse(result, () => this.completed.emit());
  }

  // ── Reintentar ────────────────────────────────────────────────────────────
  retry(): void {
    if (!this.canRetry()) {
      this.completed.emit();
      return;
    }

    if (!this.assignment?.encryptedId || this.assignment?.id === 0) {
      this.isCorrect.set(null);
      this.phase.set('intro');
      return;
    }

    const result = this.lastResult;
    if (!result || this.responseId() === null) {
      this.startNextResponse();
      return;
    }

    this.completeCurrentResponse(result, () => {
      if (this.canRetry()) this.startNextResponse();
      else this.completed.emit();
    });
  }

  private completeCurrentResponse(result: PlayerResult, afterCompletion: () => void): void {
    const responseId = this.responseId();
    if (!responseId || this.completedResponseIds.has(responseId)) {
      afterCompletion();
      return;
    }

    this.isLoading.set(true);
    this.activitiesService.completeResponse(this.assignment.encryptedId, responseId, {
      successPercentage: result.successPercentage,
      timeSpentSeconds: result.timeSpentSeconds,
      requiredSupport: result.requiredSupport ?? false,
      observations: result.observations,
    }).subscribe({
      next: (updated) => {
        this.completedResponseIds.add(responseId);
        this.canRetry.set(updated.canRetry);
        this.isLoading.set(false);
        afterCompletion();
      },
      error: () => {
        this.isLoading.set(false);
        this.hasError.set(true);
        this.errorMsg.set('No se pudo guardar tu progreso. Intentá de nuevo.');
      },
    });
  }

  private startNextResponse(): void {
    this.isLoading.set(true);
    this.activitiesService.startResponse(this.assignment.encryptedId).subscribe({
      next: (updated) => {
        const latest = [...(updated.responses ?? [])].sort(
          (a, b) => new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
        )[0];
        this.responseId.set(latest?.encryptedId ?? null);
        this.canRetry.set(updated.canRetry);
        this.isCorrect.set(null);
        this.hasError.set(false);
        this.isLoading.set(false);
        this._startTime = Date.now();
        this.phase.set('playing');
      },
      error: () => {
        this.isLoading.set(false);
        this.hasError.set(true);
        this.errorMsg.set('No se pudo iniciar el reintento. Volvé a intentar.');
      },
    });
  }
}
