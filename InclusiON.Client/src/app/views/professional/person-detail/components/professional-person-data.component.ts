import { Component, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { CatalogItem, PersonResponse, UpdatePersonDisabilityTypeRequest } from '@models';
import {
  ButtonDirective,
  ColComponent,
  FormLabelDirective,
  FormSelectDirective,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-person-data',
  standalone: true,
  imports: [
    DatePipe,
    ButtonDirective,
    ColComponent,
    FormLabelDirective,
    FormSelectDirective,
    RowComponent,
    SpinnerComponent,
    FormsModule,
  ],
  templateUrl: './professional-person-data.component.html',
})
export class ProfessionalPersonDataComponent implements OnInit {
  @Input({ required: true }) person!: PersonResponse;
  @Output() personChange = new EventEmitter<PersonResponse>();

  private readonly catalogsService = inject(CatalogsService);
  private readonly personsService = inject(PersonsService);
  private readonly toastService = inject(ToastService);

  disabilityTypes: CatalogItem[] = [];
  disabilityTypeId: number | null = null;
  isEditing = signal(false);
  isSaving = signal(false);

  ngOnInit(): void {
    this.catalogsService.getDisabilityTypes().subscribe({
      next: (types) => this.disabilityTypes = types,
    });
  }

  startEditing(): void {
    this.disabilityTypeId = this.person.disabilityTypeId ?? null;
    this.isEditing.set(true);
  }

  cancel(): void {
    this.isEditing.set(false);
  }

  save(): void {
    if (this.disabilityTypeId === null) return;

    this.isSaving.set(true);
    const request: UpdatePersonDisabilityTypeRequest = {
      disabilityTypeId: this.disabilityTypeId,
    };

    this.personsService.updateDisabilityType(this.person.id, request).subscribe({
      next: (person) => {
        this.personChange.emit(person);
        this.isEditing.set(false);
        this.isSaving.set(false);
        this.toastService.success('Tipo de discapacidad actualizado');
      },
      error: () => {
        this.isSaving.set(false);
        this.toastService.error('Error al actualizar el tipo de discapacidad');
      },
    });
  }
}
