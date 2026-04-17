import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { UserMatchSummary } from '@models';

/**
 * Lista visual de candidatos cuando el identifier matchea más de un usuario.
 * Cada card muestra el avatar (con su color asignado) + inicial + nombre + inicial apellido,
 * pensado para que la persona reconozca rápidamente cuál es ella.
 */
@Component({
  selector: 'app-identify-results-list',
  standalone: true,
  imports: [],
  template: `
    <div class="results-grid" role="listbox" [attr.aria-label]="ariaLabel">
      @for (m of matches; track m.userId) {
        <button
          type="button"
          class="match-card"
          role="option"
          [attr.aria-label]="'Soy ' + m.displayName + (m.lastNameInitial ? ' ' + m.lastNameInitial + '.' : '')"
          (click)="select.emit(m)"
          (keydown.enter)="select.emit(m)"
          (keydown.space)="select.emit(m); $event.preventDefault()">
          <div class="avatar" [style.background-color]="m.avatarColor" aria-hidden="true">
            {{ m.initial }}
          </div>
          <div class="name">
            <span class="first-name">{{ m.displayName }}</span>
            @if (m.lastNameInitial) {
              <span class="last-initial">{{ m.lastNameInitial }}.</span>
            }
          </div>
        </button>
      }
    </div>
  `,
  styles: [`
    .results-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
      gap: 16px;
      padding: 8px 0;
    }

    .match-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 20px 12px;
      border: 2px solid var(--a11y-border, #E0E0E0);
      border-radius: 16px;
      background: var(--a11y-bg, #ffffff);
      cursor: pointer;
      transition: transform 0.15s ease, box-shadow 0.15s ease, border-color 0.15s ease;
      min-height: 160px;
    }

    .match-card:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
      border-color: var(--a11y-primary, #0066CC);
    }

    .match-card:focus-visible {
      outline: 3px solid var(--a11y-focus-accent, #0D47A1);
      outline-offset: 3px;
    }

    @media (prefers-reduced-motion: reduce) {
      .match-card {
        transition: none;
      }
      .match-card:hover:not(:disabled) {
        transform: none;
      }
    }

    .avatar {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 28px;
      font-weight: 700;
      text-transform: uppercase;
      box-shadow: 0 2px 6px rgba(0, 0, 0, 0.18);
      user-select: none;
    }

    .name {
      display: flex;
      align-items: baseline;
      gap: 6px;
      color: var(--a11y-text, #212121);
      font-weight: 600;
      font-size: 18px;
    }

    .last-initial {
      color: var(--a11y-text-muted, #6c757d);
      font-weight: 500;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IdentifyResultsListComponent {
  @Input({ required: true }) matches: UserMatchSummary[] = [];
  @Input() ariaLabel = 'Tocá tu cara para entrar';

  @Output() select = new EventEmitter<UserMatchSummary>();
}
