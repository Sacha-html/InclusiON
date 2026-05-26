import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { PersonResponse } from '@models';
import { AppRoutes } from '@shared/constants/app-routes';
import { formatDate, formatDateTime } from '@shared/utils';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-person-basic-info',
  standalone: true,
  imports: [
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ButtonDirective,
  ],
  templateUrl: './person-basic-info.component.html',
})
export class PersonBasicInfoComponent {
  @Input({ required: true }) person!: PersonResponse;
  @Output() deactivate = new EventEmitter<void>();

  readonly formatDate = formatDate;
  readonly formatDateTime = formatDateTime;

  constructor(private router: Router) {}

  formatLevel(level: number | null | undefined): string {
    return level != null ? `${level} / 5` : 'Sin especificar';
  }

  formatBoolean(value: boolean): string {
    return value ? 'Si' : 'No';
  }

  formatColorBlindness(type?: 'deuteranopia' | 'protanopia' | 'tritanopia' | null): string {
    switch (type) {
      case 'deuteranopia': return 'Deuteranopía (rojo-verde)';
      case 'protanopia':   return 'Protanopía (rojo)';
      case 'tritanopia':   return 'Tritanopía (azul-amarillo)';
      default:             return 'Sin especificar';
    }
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Persons]);
  }

  goToEdit(): void {
    this.router.navigate([AppRoutes.Admin.Persons, this.person.id, 'edit']);
  }
}
