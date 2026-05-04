import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
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
    CommonModule,
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
  private _cancelling = false;

  draft = { firstName: '', lastName: '', documentNumber: '', birthDate: '' };

  startField(field: string): void {
    if (this.isSaving()) return;
    this._cancelling = false;
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
    this.editingField.set(null);
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter')  { event.preventDefault(); this.saveField(); }
    if (event.key === 'Escape') { event.preventDefault(); this.cancelField(); }
  }

  saveField(): void {
    if (this._cancelling) { this._cancelling = false; return; }
    if (!this.editingField()) return;
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
