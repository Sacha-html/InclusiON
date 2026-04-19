import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-big-button',
  standalone: true,
  imports: [IconDirective],
  templateUrl: './big-button.component.html',
  styleUrl: './big-button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BigButtonComponent {
  @Input() label = '';
  @Input() icon?: string;
  @Input() image?: string;
  @Input() color = '#2196F3';
  @Input() bgColor = '#FFFFFF';
  @Input() disabled = false;
  @Input() ariaLabel?: string;

  @Output() buttonClick = new EventEmitter<void>();

  handleClick(): void {
    if (!this.disabled) {
      this.buttonClick.emit();
    }
  }
}
