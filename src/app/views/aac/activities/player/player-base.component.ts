import { Directive, ElementRef, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import { ActivitiesService } from '@services/activities.service';
import { ActivityAssignmentResponse } from '@models/responses/activity.response';
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
  phase      = signal<PlayerPhase>('intro');
  isLoading  = signal(false);
  responseId = signal<string | null>(null);
  isCorrect  = signal<boolean | null>(null);

  private _startTime = 0;

  /** Segundos transcurridos desde que inició la fase playing. */
  get elapsedSeconds(): number {
    return Math.round((Date.now() - this._startTime) / 1000);
  }

  // ── Fase intro → playing ──────────────────────────────────────────────────
  startActivity(): void {
    this.isLoading.set(true);
    this.activitiesService.startResponse(this.assignment.encryptedId).subscribe({
      next: (updated) => {
        const responses = [...(updated.responses ?? [])];
        const latest    = responses.sort(
          (a, b) => new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
        )[0];
        this.responseId.set(latest?.encryptedId ?? null);
        this._startTime = Date.now();
        this.isLoading.set(false);
        this.phase.set('playing');
        setTimeout(() => {
          const heading = this.el.nativeElement.querySelector('.game-instruction, [role="heading"]') as HTMLElement;
          heading?.focus();
        }, 80);
      },
      error: () => this.isLoading.set(false),
    });
  }

  // ── Fase result → guardar y salir ─────────────────────────────────────────
  finishActivity(result: PlayerResult): void {
    const responseId = this.responseId();
    if (responseId === null) {
      this.completed.emit();
      return;
    }
    this.isLoading.set(true);
    this.activitiesService.completeResponse(this.assignment.encryptedId, responseId, {
      successPercentage: result.successPercentage,
      timeSpentSeconds:  result.timeSpentSeconds,
      requiredSupport:   result.requiredSupport ?? false,
      observations:      result.observations,
    }).subscribe({
      next:  () => { this.isLoading.set(false); this.completed.emit(); },
      error: () => { this.isLoading.set(false); this.completed.emit(); },
    });
  }

  // ── Reintentar ────────────────────────────────────────────────────────────
  retry(): void {
    this.isCorrect.set(null);
    this.phase.set('intro');
  }
}
