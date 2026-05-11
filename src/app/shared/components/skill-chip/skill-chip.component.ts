import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-skill-chip',
  standalone: true,
  templateUrl: './skill-chip.component.html',
  styleUrls: ['./skill-chip.component.scss'],
})
export class SkillChipComponent {
  @Input({ required: true }) name!: string;
  @Input() color = '#6c757d';
  @Input() icon?: string | null;
  /** Show × remove button */
  @Input() removable = false;
  @Output() remove = new EventEmitter<void>();
}
