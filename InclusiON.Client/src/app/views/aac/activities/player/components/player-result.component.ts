import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-player-result',
  standalone: true,
  templateUrl: './player-result.component.html',
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
