import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PersonsService, ToastService } from '@services';
import { PersonResponse, UpdatePersonRequest } from '@models';
import { toDisplayDate, toIsoDate } from '@shared/utils';
import {
  ButtonDirective,
  ColComponent,
  FormControlDirective,
  FormLabelDirective,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-person-data',
  standalone: true,
  imports: [
    CommonModule,
    ButtonDirective,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    RowComponent,
    SpinnerComponent,
    FormsModule,
  ],
  template: `
    @if (!isEditing()) {
      <div class="d-flex justify-content-end mb-3">
        <button cButton color="primary" size="sm" (click)="startEditing()">Editar datos</button>
      </div>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Nombre</label>
          <input cFormControl [value]="person.firstName" readonly />
        </c-col>
        <c-col sm="6">
          <label cLabel>Apellido</label>
          <input cFormControl [value]="person.lastName" readonly />
        </c-col>
      </c-row>
      <c-row class="mb-3">
        <c-col sm="4">
          <label cLabel>Documento</label>
          <input cFormControl [value]="person.documentNumber ?? 'Sin especificar'" readonly />
        </c-col>
        <c-col sm="4">
          <label cLabel>Fecha de nacimiento</label>
          <input cFormControl [value]="person.birthDate | date:'dd/MM/yyyy'" readonly />
        </c-col>
        <c-col sm="4">
          <label cLabel>Edad</label>
          <input cFormControl [value]="person.age + ' años'" readonly />
        </c-col>
      </c-row>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Tipo de discapacidad</label>
          <input cFormControl [value]="person.disabilityTypeName ?? 'Sin especificar'" readonly />
        </c-col>
      </c-row>

    } @else {
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Nombre</label>
          <input cFormControl [(ngModel)]="editData.firstName" />
        </c-col>
        <c-col sm="6">
          <label cLabel>Apellido</label>
          <input cFormControl [(ngModel)]="editData.lastName" />
        </c-col>
      </c-row>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Documento</label>
          <input cFormControl [(ngModel)]="editData.documentNumber" />
        </c-col>
        <c-col sm="6">
          <label cLabel>Fecha de nacimiento (dd/mm/aaaa)</label>
          <input cFormControl [(ngModel)]="editData.birthDate" placeholder="dd/mm/aaaa" />
        </c-col>
      </c-row>
      <div class="d-flex gap-2 mt-3">
        <button cButton color="primary" (click)="save()" [disabled]="isSaving()">
          @if (isSaving()) { <c-spinner size="sm" class="me-1"></c-spinner> }
          Guardar
        </button>
        <button cButton color="secondary" (click)="cancel()">Cancelar</button>
      </div>
    }
  `,
})
export class ProfessionalPersonDataComponent {
  @Input({ required: true }) person!: PersonResponse;
  @Output() personChange = new EventEmitter<PersonResponse>();

  private readonly personsService = inject(PersonsService);
  private readonly toastService = inject(ToastService);

  isEditing = signal(false);
  isSaving = signal(false);
  editData = { firstName: '', lastName: '', documentNumber: '', birthDate: '' };

  startEditing(): void {
    this.editData = {
      firstName: this.person.firstName,
      lastName: this.person.lastName,
      documentNumber: this.person.documentNumber ?? '',
      birthDate: toDisplayDate(this.person.birthDate),
    };
    this.isEditing.set(true);
  }

  cancel(): void {
    this.isEditing.set(false);
  }

  save(): void {
    this.isSaving.set(true);
    const request: UpdatePersonRequest = {
      firstName: this.editData.firstName,
      lastName: this.editData.lastName,
      documentNumber: this.editData.documentNumber || undefined,
      birthDate: this.editData.birthDate ? toIsoDate(this.editData.birthDate) : undefined,
    };

    this.personsService.updatePerson(this.person.id, request).subscribe({
      next: (person) => {
        this.personChange.emit(person);
        this.isEditing.set(false);
        this.isSaving.set(false);
        this.toastService.success('Datos personales actualizados');
      },
      error: () => {
        this.isSaving.set(false);
        this.toastService.error('Error al actualizar datos');
      },
    });
  }
}
