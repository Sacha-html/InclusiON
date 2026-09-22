import { Directive, ElementRef, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
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
export abstract class PlayerBaseComponent implements OnInit {
  @Input({ required: true }) assignment!: ActivityAssignmentResponse;
  @Output() completed = new EventEmitter<void>();

  protected readonly activitiesService = inject(ActivitiesService);
  private readonly el = inject(ElementRef<HTMLElement>);

  // Estado compartido
  phase               = signal<PlayerPhase>('intro');
  isLoading           = signal(false);
  hasError            = signal(false);
  errorMsg            = signal('');
  responseId          = signal<string | null>(null);
  isCorrect           = signal<boolean | null>(null);
  consecutiveFailures = signal(0);
  hasSucceeded        = signal(false);

  private _startTime = 0;

  /** Segundos transcurridos desde que inició la fase playing. */
  get elapsedSeconds(): number {
    return Math.round((Date.now() - this._startTime) / 1000);
  }

  /**
   * Indica si se permite reintentar la actividad.
   * Se permite si el intento actual no fue superado (< 60%) y no se han agotado los 4 intentos (HU-21).
   */
  get canRetry(): boolean {
    return !this.isCorrect() && this.consecutiveFailures() < 3;
  }

  ngOnInit(): void {
    this.initializeAttemptsFromAssignment();
  }

  protected initializeAttemptsFromAssignment(): void {
    if (!this.assignment?.responses?.length) return;

    // Verificar si ya tiene algún intento previo aprobado (>= 60%)
    const anySuccess = this.assignment.responses.some(r => (r.successPercentage ?? 0) >= 60);
    if (anySuccess) {
      this.hasSucceeded.set(true);
    }

    // Calcular fallos consecutivos previos ordenados por fecha descendente
    const sorted = [...this.assignment.responses]
      .filter(r => r.completedAt)
      .sort((a, b) => new Date(b.completedAt!).getTime() - new Date(a.completedAt!).getTime());

    let failures = 0;
    for (const r of sorted) {
      if ((r.successPercentage ?? 0) < 60) {
        failures++;
      } else {
        break;
      }
    }
    this.consecutiveFailures.set(failures);
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
    // Actualizar estado de éxito / fallos
    if (result.successPercentage >= 60) {
      this.hasSucceeded.set(true);
      this.consecutiveFailures.set(0);
    } else {
      this.consecutiveFailures.update(c => c + 1);
    }

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
    this.isLoading.set(true);
    this.activitiesService.completeResponse(this.assignment.encryptedId, responseId, {
      successPercentage: result.successPercentage,
      timeSpentSeconds:  result.timeSpentSeconds,
      requiredSupport:   result.requiredSupport ?? false,
      observations:      result.observations,
    }).subscribe({
      next:  () => { this.isLoading.set(false); this.completed.emit(); },
      error: () => { this.isLoading.set(false); this.hasError.set(true); this.errorMsg.set('No se pudo guardar tu progreso. Intentá de nuevo.'); },
    });
  }

  // ── Reintentar ────────────────────────────────────────────────────────────
  retry(): void {
    if (!this.canRetry) {
      return;
    }

    const prevResponseId = this.responseId();
    this.consecutiveFailures.update(c => c + 1);
    this.responseId.set(null);
    this.isCorrect.set(null);

    // Persistir el intento fallido previo en backend antes de iniciar uno nuevo para asegurar registro en analítica
    if (prevResponseId && this.assignment?.encryptedId && this.assignment?.id !== 0) {
      this.isLoading.set(true);
      this.activitiesService.completeResponse(this.assignment.encryptedId, prevResponseId, {
        successPercentage: 0,
        timeSpentSeconds: this.elapsedSeconds,
        requiredSupport: false,
        observations: 'Intento fallido registrado al solicitar reintento',
      }).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.startActivity();
        },
        error: (err) => {
          console.warn('No se pudo registrar intento fallido previo:', err);
          this.isLoading.set(false);
          this.startActivity();
        },
      });
    } else {
      this.startActivity();
    }
  }
}

