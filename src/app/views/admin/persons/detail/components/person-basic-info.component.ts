import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { PersonResponse } from '@models';
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
  selector: 'app-person-basic-info',
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
        <c-badge class="ms-2" [color]="person.isActive ? 'success' : 'danger'"
              role="status"
              [attr.aria-label]="'Estado: ' + (person.isActive ? 'Activo' : 'Inactivo')">
          {{ person.isActive ? 'Activo' : 'Inactivo' }}
        </c-badge>
      </c-card-header>
      <c-card-body>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-firstName">Nombre</label>
            <input cFormControl id="detail-firstName" [value]="person.firstName" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-lastName">Apellido</label>
            <input cFormControl id="detail-lastName" [value]="person.lastName" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-documentNumber">Documento</label>
            <input cFormControl id="detail-documentNumber" [value]="person.documentNumber ?? 'Sin especificar'" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-birthDate">Fecha de nacimiento</label>
            <input cFormControl id="detail-birthDate" [value]="formatDate(person.birthDate)" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-age">Edad</label>
            <input cFormControl id="detail-age" [value]="person.age + ' años'" readonly />
          </c-col>
        </c-row>

        <h5 class="mt-4 mb-3">Discapacidad</h5>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-disabilityType">Tipo de discapacidad</label>
            <input cFormControl id="detail-disabilityType" [value]="person.disabilityTypeName ?? 'Sin especificar'" readonly />
          </c-col>
        </c-row>

        <h5 class="mt-4 mb-3">Perfil funcional</h5>

        <c-row class="mb-3">
          <c-col sm="4">
            <label cLabel for="detail-attentionLevel">Atención</label>
            <input cFormControl id="detail-attentionLevel" [value]="formatLevel(person.attentionLevel)" readonly />
          </c-col>
          <c-col sm="4">
            <label cLabel for="detail-communicationLevel">Comunicación</label>
            <input cFormControl id="detail-communicationLevel" [value]="formatLevel(person.communicationLevel)" readonly />
          </c-col>
          <c-col sm="4">
            <label cLabel for="detail-motorSkillLevel">Motricidad</label>
            <input cFormControl id="detail-motorSkillLevel" [value]="formatLevel(person.motorSkillLevel)" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-usesAAC">Usa CAA</label>
            <input cFormControl id="detail-usesAAC" [value]="formatBoolean(person.usesAAC)" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-usesSignLanguage">Usa lengua de señas</label>
            <input cFormControl id="detail-usesSignLanguage" [value]="formatBoolean(person.usesSignLanguage)" readonly />
          </c-col>
        </c-row>

        <h5 class="mt-4 mb-3">Preferencias</h5>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-interests">Intereses y motivadores</label>
            <textarea cFormControl id="detail-interests" [value]="person.interestsAndMotivators ?? 'Sin especificar'" readonly rows="3"></textarea>
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-learningStyle">Estilo de aprendizaje</label>
            <textarea cFormControl id="detail-learningStyle" [value]="person.learningStyle ?? 'Sin especificar'" readonly rows="3"></textarea>
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-resources">Recursos disponibles</label>
            <textarea cFormControl id="detail-resources" [value]="person.availableResources ?? 'Sin especificar'" readonly rows="3"></textarea>
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-therapies">Terapias adicionales</label>
            <textarea cFormControl id="detail-therapies" [value]="person.additionalTherapies ?? 'Sin especificar'" readonly rows="3"></textarea>
          </c-col>
        </c-row>

        <h5 class="mt-4 mb-3">Accesibilidad</h5>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-largeFont">Requiere fuente grande</label>
            <input cFormControl id="detail-largeFont" [value]="formatBoolean(person.requiresLargeFont)" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-highContrast">Requiere alto contraste</label>
            <input cFormControl id="detail-highContrast" [value]="formatBoolean(person.requiresHighContrast)" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-visualNoise">Sensibilidad al ruido visual</label>
            <input cFormControl id="detail-visualNoise" [value]="formatBoolean(person.visualNoiseSensitivity)" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-sound">Sensibilidad al sonido</label>
            <input cFormControl id="detail-sound" [value]="formatBoolean(person.soundSensitivity)" readonly />
          </c-col>
        </c-row>

        <h5 class="mt-4 mb-3">Configuración de acceso</h5>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-autonomyLevel">Nivel de autonomía</label>
            <input cFormControl id="detail-autonomyLevel" [value]="person.autonomyLevelName ?? 'Sin especificar'" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-loginMethod">Método de login</label>
            <input cFormControl id="detail-loginMethod" [value]="person.loginMethodName ?? 'Sin especificar'" readonly />
          </c-col>
        </c-row>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-pinConfigured">PIN configurado</label>
            <input cFormControl id="detail-pinConfigured" [value]="formatBoolean(person.hasPinConfigured)" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-avatarColor">Color de avatar</label>
            <div class="d-flex align-items-center gap-2">
              <span class="avatar-color-preview" [style.background-color]="person.avatarColor ?? '#ccc'"></span>
              <input cFormControl id="detail-avatarColor" [value]="person.avatarColor ?? 'Sin especificar'" readonly />
            </div>
          </c-col>
        </c-row>

        <h5 class="mt-4 mb-3">Auditoría</h5>

        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel for="detail-createdAt">Fecha de alta</label>
            <input cFormControl id="detail-createdAt" [value]="formatDateTime(person.createdAt)" readonly />
          </c-col>
          <c-col sm="6">
            <label cLabel for="detail-updatedAt">Última actualización</label>
            <input cFormControl id="detail-updatedAt" [value]="person.updatedAt ? formatDateTime(person.updatedAt) : 'Sin actualizar'" readonly />
          </c-col>
        </c-row>

        <div class="mt-3 d-flex justify-content-between">
          <div class="d-flex gap-2">
            <button cButton color="secondary" (click)="goBack()" aria-label="Volver al listado de personas">Volver</button>
            @if (person.isActive) {
              <button cButton color="primary" (click)="goToEdit()" [attr.aria-label]="'Editar persona ' + person.fullName">Editar</button>
            }
          </div>
          @if (person.isActive) {
            <button cButton color="danger" (click)="deactivate.emit()" [attr.aria-label]="'Desactivar persona ' + person.fullName">Desactivar</button>
          }
        </div>

      </c-card-body>
    </c-card>
  `,
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

  goBack(): void {
    this.router.navigate(['/admin/persons']);
  }

  goToEdit(): void {
    this.router.navigate(['/admin/persons', this.person.id, 'edit']);
  }
}
