import { Component, Input, Output, EventEmitter, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { map } from 'rxjs';
import { AssignmentsService, PersonsService, ToastService } from '@services';
import {
  PersonListItemResponse,
  ProfessionalPersonResponse,
} from '@models';
import { SearchableSelectComponent } from '@shared/components/searchable-select/searchable-select.component';
import {
  ButtonDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-professional-persons',
  standalone: true,
  imports: [
    ButtonDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    ReactiveFormsModule,
    FormsModule,
    IconModule,
    SearchableSelectComponent,
    DataTableComponent,
    ConfirmModalComponent,
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

  showAssignPersonModal = signal(false);
  showDeactivatePersonModal = signal(false);
  personToDeactivate = signal<ProfessionalPersonResponse | null>(null);
  isSubmitting = signal(false);

  assignPersonForm: FormGroup = this.fb.group({
    personId: [null],
    isPrimaryProfessional: [false],
    canSuperviseLogin: [false],
  });

  /** searchFn para SearchableSelectComponent — server-side, excluye ya asignados */
  readonly searchPersonsFn = (query: string) => {
    const assignedIds = new Set(this.persons.filter(p => p.isActive).map(p => p.personId));
    return this.personsService.getPersons({ search: query, pageSize: 20, isActive: true }).pipe(
      map(r => r.data.filter(p => !assignedIds.has(p.id)))
    );
  };

  readonly displayPerson = (p: PersonListItemResponse) => p.fullName ?? '';
  readonly subDisplayPerson = (p: PersonListItemResponse) => p.disabilityTypeName ?? '';
  readonly valueFromPerson = (p: PersonListItemResponse) => p.id;

  columns: TableColumn[] = [
    { key: 'personFullName', label: 'Nombre' },
    {
      key: 'isPrimaryProfessional',
      label: 'Principal',
      type: 'badge',
      badgeMap: {
        'true':  { color: 'success',   label: 'Sí' },
        'false': { color: 'secondary', label: 'No' },
      },
    },
    {
      key: 'canSuperviseLogin',
      label: 'Supervisa login',
      type: 'badge',
      badgeMap: {
        'true':  { color: 'success',   label: 'Sí' },
        'false': { color: 'secondary', label: 'No' },
      },
    },
    {
      key: 'isActive',
      label: 'Estado',
      type: 'badge',
      badgeMap: {
        'true':  { color: 'success', label: 'Activo'   },
        'false': { color: 'danger',  label: 'Inactivo' },
      },
    },
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'deactivate', label: 'Desasignar', icon: 'cil-user-unfollow', visible: (item) => item.isActive },
      ],
    },
  ];

  onRowAction(event: { action: string; item: ProfessionalPersonResponse }): void {
    if (event.action === 'deactivate') this.openDeactivatePersonModal(event.item);
  }

  ngOnInit(): void {}

  openAssignPersonModal(): void {
    this.assignPersonForm.reset({ personId: null, isPrimaryProfessional: false, canSuperviseLogin: false });
    this.showAssignPersonModal.set(true);
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
