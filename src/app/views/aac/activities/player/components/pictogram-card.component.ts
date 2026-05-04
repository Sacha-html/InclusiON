import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ArasaacService } from '@services/arasaac.service';

@Component({
  selector: 'app-pictogram-card',
  standalone: true,
  template: `
    <button
      class="picto-card"
      [class]="cardClass"
      [disabled]="disabled"
      (click)="cardClick.emit()"
      [attr.aria-label]="label"
      [attr.aria-pressed]="selected"
    >
      @if (pictogramId) {
        <img
          [src]="arasaac.getPictogramUrl(pictogramId)"
          [alt]="label"
          class="picto-img"
          loading="lazy"
        />
      }
      <span class="picto-label">{{ label }}</span>

      @if (badge) {
        <span class="picto-badge" aria-hidden="true">{{ badge }}</span>
      }
    </button>
  `,
  styles: [`
    .picto-card {
      position: relative;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: .5rem;
      padding: 1rem .75rem;
      border: 3px solid var(--a11y-border, #E0E0E0);
      border-radius: 20px;
      background: var(--a11y-surface, #fff);
      cursor: pointer;
      transition: border-color .15s, transform .1s, box-shadow .15s;
      box-shadow: 0 2px 8px rgba(0,0,0,.06);
      width: 100%;

      &:focus-visible { outline: 4px solid var(--a11y-primary, #2196F3); outline-offset: 2px; }
      &:not(:disabled):hover {
        border-color: var(--a11y-primary, #2196F3);
        transform: translateY(-3px);
        box-shadow: 0 6px 18px rgba(33,150,243,.2);
      }
      &:disabled { cursor: default; }
    }

    .picto-card--selected {
      border-color: var(--a11y-primary, #2196F3);
      background: rgba(33,150,243,.07);
      box-shadow: 0 0 0 2px var(--a11y-primary, #2196F3);
    }
    .picto-card--correct {
      border-color: var(--a11y-success, #4CAF50);
      background: rgba(76,175,80,.08);
      transform: scale(1.04);
      box-shadow: 0 4px 20px rgba(76,175,80,.3);
    }
    .picto-card--wrong {
      border-color: var(--a11y-danger, #F44336);
      background: rgba(244,67,54,.06);
    }
    .picto-card--reveal {
      border-color: var(--a11y-warning, #FF9800);
      background: rgba(255,152,0,.08);
    }
    .picto-card--dimmed { opacity: .45; }
    .picto-card--matched {
      border-color: var(--a11y-success, #4CAF50);
      background: rgba(76,175,80,.08);
      opacity: .75;
    }

    .picto-img { width: 90px; height: 90px; object-fit: contain; }

    .picto-label {
      font-size: 1rem;
      font-weight: 600;
      color: var(--a11y-text, #212121);
      text-align: center;
    }

    .picto-badge {
      position: absolute;
      top: -.5rem;
      right: -.5rem;
      font-size: 1.5rem;
    }
  `],
})
export class PictogramCardComponent {
  readonly arasaac = inject(ArasaacService);

  @Input({ required: true }) label!: string;
  @Input() pictogramId?: number;
  @Input() disabled = false;
  @Input() selected = false;
  @Input() state: 'none' | 'correct' | 'wrong' | 'reveal' | 'dimmed' | 'matched' = 'none';
  @Input() badge?: string;

  @Output() cardClick = new EventEmitter<void>();

  get cardClass(): string {
    const states: Record<string, boolean> = {
      'picto-card--selected': this.selected && this.state === 'none',
      'picto-card--correct':  this.state === 'correct',
      'picto-card--wrong':    this.state === 'wrong',
      'picto-card--reveal':   this.state === 'reveal',
      'picto-card--dimmed':   this.state === 'dimmed',
      'picto-card--matched':  this.state === 'matched',
    };
    return Object.entries(states).filter(([,v]) => v).map(([k]) => k).join(' ');
  }
}
