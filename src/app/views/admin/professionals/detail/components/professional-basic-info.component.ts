import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ProfessionalResponse } from '@models';
import { formatDate, formatDateTime } from '@shared/utils';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormLabelDirective,
  RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-basic-info',
  standalone: true,
  imports: [
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    ButtonDirective,
  ],
  template: `
    <c-card>
      <c-card-header>
        <strong>Datos Personales</strong>
        <c-badge class="ms-2" [color]="professional.isActive ? 'success' : 'danger'"
              role="status"
              [attr.aria-label]="'Estado: ' + (professional.isActive ? 'Activo' : 'Inactivo')">
          {{ professional.isActive ? 'Activo' : 'Inactivo' }}
        </c-badge>
      </c-card-header>
      <c-card-body>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-firstName">Nombre</label>
            <input cFormControl id="detail-firstName" [value]="professional.firstName" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-lastName">Apellido</label>
            <input cFormControl id="detail-lastName" [value]="professional.lastName" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-email">Email</label>
            <input cFormControl id="detail-email" [value]="professional.email ?? 'Sin especificar'" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-documentNumber">Documento</label>
            <input cFormControl id="detail-documentNumber" [value]="professional.documentNumber ?? 'Sin especificar'" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-specialty">Especialidad</label>
            <input cFormControl id="detail-specialty" [value]="professional.specialty ?? 'Sin especificar'" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-licenseNumber">Matricula</label>
            <input cFormControl id="detail-licenseNumber" [value]="professional.licenseNumber ?? 'Sin especificar'" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-phone">Telefono</label>
            <input cFormControl id="detail-phone" [value]="professional.phone ?? 'Sin especificar'" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-birthDate">Fecha de nacimiento</label>
            <input cFormControl id="detail-birthDate" [value]="formatDate(professional.birthDate)" readonly />
          </c-col>
        </c-row>

        <h5 class="mt-4 mb-3">Auditoria</h5>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-createdAt">Fecha de alta</label>
            <input cFormControl id="detail-createdAt" [value]="formatDateTime(professional.createdAt)" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-updatedAt">Ultima actualizacion</label>
            <input cFormControl id="detail-updatedAt" [value]="professional.updatedAt ? formatDateTime(professional.updatedAt) : 'Sin actualizar'" readonly />
          </c-col>
        </c-row>

        <div class="mt-3 d-flex justify-content-between">
          <div class="d-flex gap-2">
            <button cButton color="secondary" (click)="goBack()" aria-label="Volver al listado de profesionales">Volver</button>
            @if (professional.isActive) {
              <button cButton color="primary" (click)="goToEdit()" [attr.aria-label]="'Editar profesional ' + professional.fullName">Editar</button>
            }
          </div>
          @if (professional.isActive) {
            <button cButton color="danger" (click)="deactivate.emit()" [attr.aria-label]="'Desactivar profesional ' + professional.fullName">Desactivar</button>
          }
        </div>

      </c-card-body>
    </c-card>
  `,
})
export class ProfessionalBasicInfoComponent {
  @Input({ required: true }) professional!: ProfessionalResponse;
  @Output() deactivate = new EventEmitter<void>();

  readonly formatDate = formatDate;
  readonly formatDateTime = formatDateTime;

  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/admin/professionals']);
  }

  goToEdit(): void {
    this.router.navigate(['/admin/professionals', this.professional.id, 'edit']);
  }
}
