import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-player-intro',
  standalone: true,
  templateUrl: './player-intro.component.html',
})
export class PlayerIntroComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) instruction!: string;
  @Input() hint?: string;
  @Input() icon = '🎯';
  @Input() loading = false;
  @Input() error = '';
  @Output() start = new EventEmitter<void>();
}
