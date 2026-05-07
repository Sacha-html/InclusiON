import { Component, Input, Output, EventEmitter, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AssignmentsService, PersonsService, ToastService } from '@services';
import {
  PersonListItemResponse,
  ProfessionalPersonResponse,
} from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormControlDirective,
  FormLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
  TableDirective,
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';

@Component({
  selector: 'app-professional-persons',
  standalone: true,
  imports: [
    BadgeComponent,
    ButtonDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormControlDirective,
    FormLabelDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    TableDirective,
    ReactiveFormsModule,
    FormsModule,
    IconModule,
  ],
  templateUrl: './professional-persons.component.html',
  styleUrl: './professional-persons.component.scss',
})
export class ProfessionalPersonsComponent implements OnInit {
  @Input({ required: true }) professionalId!: string;
  @Input() persons: ProfessionalPersonResponse[] = [];
  @Output() personsChange = new EventEmitter<ProfessionalPersonResponse[]>();

  private readonly fb = inject(FormBuilder);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly personsService = inject(PersonsService);
  private readonly toastService = inject(ToastService);

  availablePersons: PersonListItemResponse[] = [];
  showAssignPersonModal = signal(false);
  showDeactivatePersonModal = signal(false);
  personToDeactivate = signal<ProfessionalPersonResponse | null>(null);
  isSubmitting = signal(false);
  isLoadingPersons = signal(false);

  searchPersonText = '';
  filteredPersons: PersonListItemResponse[] = [];
  selectedPersonDisplay: PersonListItemResponse | null = null;

  assignPersonForm: FormGroup = this.fb.group({
    personId: [null],
    isPrimaryProfessional: [false],
    canSuperviseLogin: [false],
  });

  filterPersons(text: string): void {
    if (!text) { this.filteredPersons = []; return; }
    const lower = text.toLowerCase();
    this.filteredPersons = this.availablePersons.filter(p =>
      (p.fullName?.toLowerCase().includes(lower) ||
       p.documentNumber?.toLowerCase().includes(lower)) ?? false
    );
  }

  selectPerson(p: PersonListItemResponse): void {
    this.selectedPersonDisplay = p;
    this.searchPersonText = '';
    this.filteredPersons = [];
    this.assignPersonForm.patchValue({ personId: p.id });
  }

  clearSelectedPerson(): void {
    this.selectedPersonDisplay = null;
    this.searchPersonText = '';
    this.filteredPersons = [];
    this.assignPersonForm.patchValue({ personId: null });
  }

  ngOnInit(): void {}

  openAssignPersonModal(): void {
    this.assignPersonForm.reset({ personId: null, isPrimaryProfessional: false, canSuperviseLogin: false });
    this.isLoadingPersons.set(true);
    this.personsService.getPersons({ pageSize: 1000, isActive: true }).subscribe({
      next: (response) => {
        const assignedIds = new Set(this.persons.filter((p) => p.isActive).map((p) => p.personId));
        this.availablePersons = response.data.filter((p) => !assignedIds.has(p.id));
        this.isLoadingPersons.set(false);
        this.showAssignPersonModal.set(true);
        this.searchPersonText = '';
        this.filteredPersons = [];
        this.selectedPersonDisplay = null;
      },
      error: () => {
        this.isLoadingPersons.set(false);
        this.toastService.error('Error al cargar personas disponibles');
      },
    });
  }

  confirmAssignPerson(): void {
    const val = this.assignPersonForm.value;
    if (!val.personId) return;

    this.isSubmitting.set(true);
    this.assignmentsService
      .assignPerson(this.professionalId, {
        personId: val.personId,
        isPrimaryProfessional: val.isPrimaryProfessional ?? false,
        canSuperviseLogin: val.canSuperviseLogin ?? false,
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showAssignPersonModal.set(false);
          this.toastService.success('Persona asignada exitosamente');
          this.loadAssignedPersons();
        },
        error: () => {
          this.isSubmitting.set(false);
          this.toastService.error('Error al asignar persona');
        },
      });
  }

  cancelAssignPerson(): void {
    this.showAssignPersonModal.set(false);
  }

  openDeactivatePersonModal(person: ProfessionalPersonResponse): void {
    this.personToDeactivate.set(person);
    this.showDeactivatePersonModal.set(true);
  }

  confirmDeactivatePerson(): void {
    if (!this.personToDeactivate()) return;

    this.assignmentsService
      .deactivatePersonAssignment(this.professionalId, this.personToDeactivate()!.personId)
      .subscribe({
        next: () => {
          this.showDeactivatePersonModal.set(false);
          this.personToDeactivate.set(null);
          this.toastService.success('Asignacion desactivada exitosamente');
          this.loadAssignedPersons();
        },
        error: () => {
          this.showDeactivatePersonModal.set(false);
          this.toastService.error('Error al desactivar la asignacion');
        },
      });
  }

  cancelDeactivatePerson(): void {
    this.showDeactivatePersonModal.set(false);
    this.personToDeactivate.set(null);
  }

  private loadAssignedPersons(): void {
    this.assignmentsService.getPersonsByProfessional(this.professionalId).subscribe({
      next: (data) => this.personsChange.emit(data),
      error: () => this.toastService.error('Error al cargar personas asignadas'),
    });
  }
}
