import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-visual-card',
  standalone: true,
  imports: [IconDirective, NgTemplateOutlet],
  templateUrl: './visual-card.component.html',
  styleUrl: './visual-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VisualCardComponent {
  @Input() title = '';
  @Input() subtitle?: string;
  @Input() icon?: string;
  @Input() image?: string;
  /** Semantic variant — sets icon bg + badge bg/text via CSS classes. */
  @Input() variant?: 'primary' | 'success' | 'warning' | 'danger' | 'muted';
  /** Custom color fallback (used when variant is not set, e.g. calendar event colors). */
  @Input() accentColor = 'var(--a11y-primary, #2196F3)';
  @Input() badge?: string;
  @Input() badgeColor = 'var(--a11y-primary, #2196F3)';
  @Input() interactive = true;
  @Input() ariaLabel?: string;

  @Output() cardClick = new EventEmitter<void>();

  handleClick(): void {
    if (this.interactive) {
      this.cardClick.emit();
    }
  }
}
