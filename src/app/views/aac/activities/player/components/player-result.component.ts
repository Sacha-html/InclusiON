import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-player-result',
  standalone: true,
  template: `
    <div class="player-result" role="region" aria-label="Resultado de la actividad" aria-live="assertive">
      @if (success) {
        <div class="result-icon result-icon--success" aria-hidden="true">🎉</div>
        <h2 class="result-title result-title--success">¡Muy bien!</h2>
      } @else {
        <div class="result-icon result-icon--fail" aria-hidden="true">💪</div>
        <h2 class="result-title result-title--fail">¡Casi!</h2>
      }

      @if (score !== null) {
        <p class="result-score" [class.result-score--success]="success" [class.result-score--fail]="!success">
          {{ score }}%
        </p>
      }

      @if (message) {
        <p class="result-msg">{{ message }}</p>
      }

      <div class="result-actions">
        <button
          class="action-btn action-btn--primary"
          [disabled]="loading"
          (click)="finish.emit()"
          aria-label="Finalizar y volver"
        >
          @if (loading) {
            <span class="btn-spinner" aria-hidden="true"></span>
            Guardando...
          } @else {
            ✓ Finalizar
          }
        </button>

        @if (canRetry) {
          <button
            class="action-btn action-btn--secondary"
            [disabled]="loading"
            (click)="retry.emit()"
            aria-label="Intentar de nuevo"
          >
            🔄 Intentar de nuevo
          </button>
        }
      </div>
    </div>
  `,
})
export class PlayerResultComponent {
  @Input({ required: true }) success!: boolean;
  @Input() score: number | null = null;
  @Input() message?: string;
  @Input() loading = false;
  @Input() canRetry = true;
  @Output() finish = new EventEmitter<void>();
  @Output() retry  = new EventEmitter<void>();
}
