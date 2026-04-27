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
  templateUrl: './professional-person-data.component.html',
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
