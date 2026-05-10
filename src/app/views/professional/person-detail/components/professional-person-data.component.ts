import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PersonsService, ToastService } from '@services';
import { PersonResponse, UpdatePersonRequest } from '@models';
import { toDisplayDate, toIsoDate } from '@shared/utils';
import {
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
    DatePipe,
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
  private readonly toastService   = inject(ToastService);

  editingField = signal<string | null>(null);
  isSaving     = signal(false);
  fieldError   = signal<string | null>(null);
  private _cancelling = false;

  draft = { firstName: '', lastName: '', documentNumber: '', birthDate: '' };

  startField(field: string): void {
    if (this.isSaving()) return;
    this._cancelling = false;
    this.fieldError.set(null);
    this.draft = {
      firstName:      this.person.firstName,
      lastName:       this.person.lastName,
      documentNumber: this.person.documentNumber ?? '',
      birthDate:      toDisplayDate(this.person.birthDate),
    };
    this.editingField.set(field);
  }

  cancelField(): void {
    this._cancelling = true;
    this.fieldError.set(null);
    this.editingField.set(null);
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter')  { event.preventDefault(); this.saveField(); }
    if (event.key === 'Escape') { event.preventDefault(); this.cancelField(); }
  }

  saveField(): void {
    if (this._cancelling) { this._cancelling = false; return; }
    if (!this.editingField()) return;

    if (this.editingField() === 'documentNumber' && this.draft.documentNumber) {
      const doc = this.draft.documentNumber;
      if (!/^[a-zA-Z0-9]+$/.test(doc)) {
        this.fieldError.set('Solo letras y números, sin espacios ni caracteres especiales');
        return;
      }
      if (doc.length < 6 || doc.length > 20) {
        this.fieldError.set('El documento debe tener entre 6 y 20 caracteres');
        return;
      }
    }
    this.fieldError.set(null);
    this.isSaving.set(true);
    const request: UpdatePersonRequest = {
      firstName:      this.draft.firstName,
      lastName:       this.draft.lastName,
      documentNumber: this.draft.documentNumber || undefined,
      birthDate:      this.draft.birthDate ? toIsoDate(this.draft.birthDate) : undefined,
    };
    this.personsService.updatePerson(this.person.id, request).subscribe({
      next: (person) => {
        this.personChange.emit(person);
        this.editingField.set(null);
        this.isSaving.set(false);
        this.toastService.success('Dato actualizado');
      },
      error: () => {
        this.isSaving.set(false);
        this.toastService.error('Error al actualizar');
      },
    });
  }
}
