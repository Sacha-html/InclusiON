import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ArasaacService } from '@services/arasaac.service';

@Component({
  selector: 'app-pictogram-card',
  standalone: true,
  templateUrl: './pictogram-card.component.html',
  styleUrls: ['./pictogram-card.component.scss'],
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
