import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent } from '@coreui/angular';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [RouterLink, CardComponent, CardBodyComponent],
  templateUrl: './stat-card.component.html',
})
export class StatCardComponent {
  @Input({ required: true }) label!: string;
  @Input({ required: true }) value!: number | string;
  @Input() color = 'primary';
  @Input() linkText?: string;
  @Input() linkUrl?: string;
}
