import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-player-result',
  standalone: true,
  templateUrl: './player-result.component.html',
  styleUrl: './player-result.component.scss',
})
export class PlayerResultComponent {
  @Input({ required: true }) success!: boolean;
  @Input() score: number | null = null;
  @Input() message?: string;
  @Input() loading = false;
  @Input() canRetry = true;
  @Output() finish = new EventEmitter<void>();
  @Output() retry = new EventEmitter<void>();

  // 10 piezas de confetti
  readonly confettiItems = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
  readonly confettiColors = ['#FFD700', '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7', '#DDA0DD', '#98D8C8'];
}
