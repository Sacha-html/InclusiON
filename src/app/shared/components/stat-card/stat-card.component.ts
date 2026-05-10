import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent } from '@coreui/angular';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [RouterLink, CardComponent, CardBodyComponent],
  template: `
    <c-card class="h-100">
      <c-card-body class="text-center">
        <p class="text-body-secondary small mb-1">{{ label }}</p>
        <div class="display-6 fw-bold" [class]="'text-' + color">{{ value }}</div>
        @if (linkUrl && linkText) {
          <a [routerLink]="linkUrl" class="btn btn-sm btn-link p-0 mt-1">{{ linkText }}</a>
        }
      </c-card-body>
    </c-card>
  `,
})
export class StatCardComponent {
  @Input({ required: true }) label!: string;
  @Input({ required: true }) value!: number | string;
  @Input() color = 'primary';
  @Input() linkText?: string;
  @Input() linkUrl?: string;
}
