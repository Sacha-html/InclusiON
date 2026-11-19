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
  @Output() reactivate = new EventEmitter<void>();

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

  formatAvatarColor(hex?: string | null): string {
    if (!hex) return 'Sin especificar';
    const colorMap: Record<string, string> = {
      '#8B0000': 'Rojo',
      '#880E4F': 'Rosa',
      '#6A1B9A': 'Violeta',
      '#4527A0': 'Púrpura',
      '#283593': 'Índigo',
      '#0D47A1': 'Azul',
      '#004D40': 'Verde Azulado',
      '#03A9F4': 'Celeste',
      '#00BCD4': 'Cian',
      '#4CAF50': 'Verde',
      '#8BC34A': 'Verde Claro',
      '#CDDC39': 'Lima',
      '#FFEB3B': 'Amarillo',
      '#FFC107': 'Ámbar',
      '#FF9800': 'Naranja',
      '#5D1F00': 'Naranja Oscuro',
      '#667EEA': 'Azul Índigo',
      '#4F46E5': 'Índigo',
      '#3B82F6': 'Azul',
      '#10B981': 'Verde',
      '#EF4444': 'Rojo',
      '#F59E0B': 'Ámbar',
      '#8B5CF6': 'Violeta',
      '#EC4899': 'Rosa',
    };
    return colorMap[hex.trim().toUpperCase()] ?? 'Personalizado';
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Persons]);
  }

  goToEdit(): void {
    this.router.navigate([AppRoutes.Admin.Persons, this.person.id, 'edit']);
  }
}
