import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-visual-card',
  standalone: true,
  imports: [IconDirective],
  templateUrl: './visual-card.component.html',
  styleUrl: './visual-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VisualCardComponent {
  @Input() title = '';
  @Input() subtitle?: string;
  @Input() icon?: string;
  @Input() image?: string;
  @Input() accentColor = 'var(--a11y-primary, #2196F3)';
  @Input() badge?: string;
  @Input() badgeColor = 'var(--a11y-danger, #F44336)';
  @Input() interactive = true;
  @Input() ariaLabel?: string;

  @Output() cardClick = new EventEmitter<void>();

  handleClick(): void {
    if (this.interactive) {
      this.cardClick.emit();
    }
  }

  handleSpaceKey(event: Event): void {
    event.preventDefault();
    this.handleClick();
  }
}
