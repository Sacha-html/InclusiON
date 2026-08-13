import { Component, Input, Output, EventEmitter, inject, OnInit, signal, computed } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { map } from 'rxjs';
import { AssignmentsService, PersonsService, ToastService } from '@services';
import { ProfessionalsService } from '@services/professionals.service';
import {
  PersonListItemResponse,
  ProfessionalPersonResponse,
  ProfessionalListItemResponse,
  ClassroomResponse,
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
import { NgIf } from '@angular/common';

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
    NgIf,
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
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly toastService = inject(ToastService);

  // ── State ──────────────────────────────────────────────────────────────
  showAssignPersonModal = signal(false);
  showDeactivatePersonModal = signal(false);
  showTransferModal = signal(false);
  showMovePersonModal = signal(false);
  personToDeactivate = signal<ProfessionalPersonResponse | null>(null);
  personToTransfer = signal<ProfessionalPersonResponse | null>(null);
  personToMove = signal<ProfessionalPersonResponse | null>(null);
  selectedTargetProfessionalId = signal<string>('');
  activeProfessionalsList = signal<ProfessionalListItemResponse[]>([]);
  isSubmitting = signal(false);

  // ── Classrooms & Filter ────────────────────────────────────────────────
  classrooms = signal<ClassroomResponse[]>([]);
  isLoadingClassrooms = signal(false);
  selectedClassroomIdFilter = signal<string | null>(null);

  filteredPersons = computed(() => {
    const filterId = this.selectedClassroomIdFilter();
    if (!filterId) return this.persons;
    if (filterId === 'unassigned') {
      return this.persons.filter(p => !p.classroomId && !p.classroomName);
    }
    const selectedRoom = this.classrooms().find(
      c => c.id === filterId || (c.id && c.id.toLowerCase() === filterId.toLowerCase())
    );
    const targetName = selectedRoom?.name?.toLowerCase()?.trim();

    return this.persons.filter(p => p.classroomName?.toLowerCase()?.trim() === targetName);
  });

  toggleClassroomFilter(classroomId: string | null): void {
    if (this.selectedClassroomIdFilter() === classroomId) {
      this.selectedClassroomIdFilter.set(null);
    } else {
      this.selectedClassroomIdFilter.set(classroomId);
    }
  }

  // Modal: Crear aula
  showCreateClassroomModal = signal(false);
  selectedPersonsToAssign = signal<PersonListItemResponse[]>([]);
  tempPersonControl = new FormControl<PersonListItemResponse | null>(null);
  createClassroomForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    isPrimaryProfessional: [false],
    canSuperviseLogin: [false],
  });

  // Modal: Renombrar aula
  showRenameClassroomModal = signal(false);
  classroomToRename = signal<ClassroomResponse | null>(null);
  renameClassroomForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
  });

  // Modal: Dar de baja aula
  showDeactivateClassroomModal = signal(false);
  classroomToDeactivate = signal<ClassroomResponse | null>(null);

  // Modal: Eliminar aula
  showDeleteClassroomModal = signal(false);
  classroomToDelete = signal<ClassroomResponse | null>(null);

  // ── Forms ──────────────────────────────────────────────────────────────
  assignPersonForm: FormGroup = this.fb.group({
    personId: [null],
    classroomId: [null],
    isPrimaryProfessional: [false],
    canSuperviseLogin: [false],
  });

  // ── Search fns ─────────────────────────────────────────────────────────
  readonly searchPersonsFn = (query: string) => {
    const assignedIds = new Set(this.persons.filter(p => p.isActive).map(p => p.personId));
    return this.personsService.getPersons({ search: query, pageSize: 20, isActive: true }).pipe(
      map(r => r.data.filter(p => !assignedIds.has(p.id)))
    );
  };

  readonly searchPersonsForClassroomFn = (query: string) => {
    const assignedIds = new Set(this.persons.filter(p => p.isActive).map(p => p.personId));
    const alreadyAddedIds = new Set(this.selectedPersonsToAssign().map(p => p.id));
    return this.personsService.getPersons({ search: query, pageSize: 20, isActive: true }).pipe(
      map(r => r.data.filter(p => !assignedIds.has(p.id) && !alreadyAddedIds.has(p.id)))
    );
  };

  readonly fullPersonValue = (p: PersonListItemResponse) => p;
  readonly displayPerson = (p: PersonListItemResponse) => p.fullName ?? '';
  readonly subDisplayPerson = (p: PersonListItemResponse) =>
    p.documentNumber ? `${p.disabilityTypeName ?? 'Sin tipo'} (DNI: ${p.documentNumber})` : p.disabilityTypeName ?? '';
  readonly valueFromPerson = (p: PersonListItemResponse) => p.id;

  // ── Table columns ──────────────────────────────────────────────────────
  columns: TableColumn[] = [
    { key: 'personFullName', label: 'Nombre' },
    { key: 'personDocumentNumber', label: 'Documento' },
    { key: 'classroomName', label: 'Aula' },
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
        { action: 'move-classroom', label: 'Cambiar Aula', icon: 'cilPencil', visible: (item) => item.isActive },
        { action: 'transfer', label: 'Transferir', icon: 'cilSwapHorizontal', visible: (item) => item.isActive },
        { action: 'deactivate', label: 'Desasignar', icon: 'cilUserUnfollow', visible: (item) => item.isActive },
      ],
    },
  ];

  // ── Lifecycle ──────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.professionalsService.getProfessionals({ pageSize: 500, status: 'active' }).subscribe({
      next: (r) => {
        this.activeProfessionalsList.set(r.data.filter(p => p.id !== this.professionalId));
      }
    });
    this.loadClassrooms();
  }

  // ── Table actions ──────────────────────────────────────────────────────
  onRowAction(event: { action: string; item: ProfessionalPersonResponse }): void {
    if (event.action === 'deactivate') {
      this.openDeactivatePersonModal(event.item);
    } else if (event.action === 'transfer') {
      this.openTransferModal(event.item);
    } else if (event.action === 'move-classroom') {
      this.openMovePersonModal(event.item);
    }
  }

  // ── Transfer ───────────────────────────────────────────────────────────
  openTransferModal(person: ProfessionalPersonResponse): void {
    this.personToTransfer.set(person);
    this.selectedTargetProfessionalId.set('');
    this.showTransferModal.set(true);
  }

  confirmTransfer(): void {
    const student = this.personToTransfer();
    const targetProfId = this.selectedTargetProfessionalId();
    if (!student || !targetProfId) return;

    this.isSubmitting.set(true);
    this.assignmentsService.transferStudent({
      personId: student.personId,
      fromProfessionalId: this.professionalId,
      toProfessionalId: targetProfId
    }).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.showTransferModal.set(false);
        this.personToTransfer.set(null);
        this.toastService.success(res?.message ?? 'Alumno transferido exitosamente');
        this.loadAssignedPersons();
        this.loadClassrooms();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.toastService.error(err?.userMessage ?? 'Error al transferir alumno');
      }
    });
  }

  cancelTransfer(): void {
    this.showTransferModal.set(false);
    this.personToTransfer.set(null);
  }

  // ── Assign person ──────────────────────────────────────────────────────
  openAssignPersonModal(): void {
    this.assignPersonForm.reset({ personId: null, classroomId: null, isPrimaryProfessional: false, canSuperviseLogin: false });
    this.showAssignPersonModal.set(true);
  }

  confirmAssignPerson(): void {
    const val = this.assignPersonForm.value;
    if (!val.personId) return;

    this.isSubmitting.set(true);
    this.assignmentsService
      .assignPerson(this.professionalId, {
        personId: val.personId,
        classroomId: val.classroomId || null,
        isPrimaryProfessional: val.isPrimaryProfessional ?? false,
        canSuperviseLogin: val.canSuperviseLogin ?? false,
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showAssignPersonModal.set(false);
          this.toastService.success('Persona asignada exitosamente');
          this.loadAssignedPersons();
          this.loadClassrooms();
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

  // ── Move person to classroom ────────────────────────────────────────────
  movePersonForm: FormGroup = this.fb.group({ classroomId: [null] });

  openMovePersonModal(person: ProfessionalPersonResponse): void {
    this.personToMove.set(person);
    this.movePersonForm.reset({ classroomId: person.classroomId ?? null });
    this.showMovePersonModal.set(true);
  }

  confirmMovePersonToClassroom(): void {
    const person = this.personToMove();
    if (!person) return;
    const classroomId = this.movePersonForm.value.classroomId || null;
    this.isSubmitting.set(true);
    this.assignmentsService
      .movePersonToClassroom(this.professionalId, person.personId, classroomId)
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showMovePersonModal.set(false);
          this.toastService.success('Alumno movido al aula correctamente');
          this.loadAssignedPersons();
          this.loadClassrooms();
        },
        error: () => {
          this.isSubmitting.set(false);
          this.toastService.error('Error al mover el alumno de aula');
        },
      });
  }

  cancelMovePersonModal(): void {
    this.showMovePersonModal.set(false);
  }

  // ── Deactivate person assignment ────────────────────────────────────────
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
          this.loadClassrooms();
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

  // ── Create classroom ───────────────────────────────────────────────────
  openCreateClassroomModal(): void {
    this.createClassroomForm.reset({ name: '', isPrimaryProfessional: false, canSuperviseLogin: false });
    this.selectedPersonsToAssign.set([]);
    this.tempPersonControl.reset(null);
    this.showCreateClassroomModal.set(true);
  }

  addPersonToClassroom(person: PersonListItemResponse | null): void {
    if (!person) return;
    const current = this.selectedPersonsToAssign();
    if (!current.some(p => p.id === person.id)) {
      this.selectedPersonsToAssign.set([...current, person]);
    }
    this.tempPersonControl.reset(null);
  }

  removePersonFromClassroom(personId: string): void {
    const current = this.selectedPersonsToAssign();
    this.selectedPersonsToAssign.set(current.filter(p => p.id !== personId));
  }

  confirmCreateClassroom(): void {
    const val = this.createClassroomForm.value;
    const personIds = this.selectedPersonsToAssign().map(p => p.id);

    if (this.createClassroomForm.invalid) return;

    this.isSubmitting.set(true);
    this.assignmentsService.createClassroom(this.professionalId, {
      name: val.name,
      personIds: personIds,
      isPrimaryProfessional: val.isPrimaryProfessional ?? false,
      canSuperviseLogin: val.canSuperviseLogin ?? false
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showCreateClassroomModal.set(false);
        this.toastService.success('Aula creada exitosamente');
        this.loadAssignedPersons();
        this.loadClassrooms();
      },
      error: () => {
        this.isSubmitting.set(false);
        this.toastService.error('Error al crear el aula');
      }
    });
  }

  cancelCreateClassroom(): void {
    this.showCreateClassroomModal.set(false);
    this.selectedPersonsToAssign.set([]);
    this.tempPersonControl.reset(null);
  }

  // ── Rename classroom ───────────────────────────────────────────────────
  openRenameClassroomModal(classroom: ClassroomResponse): void {
    this.classroomToRename.set(classroom);
    this.renameClassroomForm.reset({ name: classroom.name });
    this.showRenameClassroomModal.set(true);
  }

  confirmRenameClassroom(): void {
    if (this.renameClassroomForm.invalid || !this.classroomToRename()) return;

    this.isSubmitting.set(true);
    this.assignmentsService
      .updateClassroom(this.professionalId, this.classroomToRename()!.id, this.renameClassroomForm.value.name)
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showRenameClassroomModal.set(false);
          this.classroomToRename.set(null);
          this.toastService.success('Aula renombrada exitosamente');
          this.loadClassrooms();
        },
        error: () => {
          this.isSubmitting.set(false);
          this.toastService.error('Error al renombrar el aula');
        }
      });
  }

  cancelRenameClassroom(): void {
    this.showRenameClassroomModal.set(false);
    this.classroomToRename.set(null);
  }

  // ── Deactivate classroom ───────────────────────────────────────────────
  openDeactivateClassroomModal(classroom: ClassroomResponse): void {
    this.classroomToDeactivate.set(classroom);
    this.showDeactivateClassroomModal.set(true);
  }

  confirmDeactivateClassroom(): void {
    if (!this.classroomToDeactivate()) return;

    this.isSubmitting.set(true);
    this.assignmentsService
      .deactivateClassroom(this.professionalId, this.classroomToDeactivate()!.id)
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showDeactivateClassroomModal.set(false);
          this.classroomToDeactivate.set(null);
          this.toastService.success('Aula dada de baja. Los alumnos siguen asignados al profesional.');
          this.loadClassrooms();
          this.loadAssignedPersons();
        },
        error: () => {
          this.isSubmitting.set(false);
          this.toastService.error('Error al dar de baja el aula');
        }
      });
  }

  cancelDeactivateClassroom(): void {
    this.showDeactivateClassroomModal.set(false);
    this.classroomToDeactivate.set(null);
  }

  // ── Delete classroom ───────────────────────────────────────────────────
  openDeleteClassroomModal(classroom: ClassroomResponse): void {
    this.classroomToDelete.set(classroom);
    this.showDeleteClassroomModal.set(true);
  }

  confirmDeleteClassroom(): void {
    if (!this.classroomToDelete()) return;

    this.isSubmitting.set(true);
    this.assignmentsService
      .deleteClassroom(this.professionalId, this.classroomToDelete()!.id)
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showDeleteClassroomModal.set(false);
          this.classroomToDelete.set(null);
          this.toastService.success('Aula eliminada exitosamente');
          this.loadClassrooms();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.toastService.error(err?.userMessage ?? 'No se puede eliminar el aula porque tiene alumnos asignados.');
        }
      });
  }

  cancelDeleteClassroom(): void {
    this.showDeleteClassroomModal.set(false);
    this.classroomToDelete.set(null);
  }

  // ── Loaders ────────────────────────────────────────────────────────────
  private loadClassrooms(): void {
    this.isLoadingClassrooms.set(true);
    this.assignmentsService.getClassroomsByProfessional(this.professionalId).subscribe({
      next: (data) => {
        this.classrooms.set(data);
        this.isLoadingClassrooms.set(false);
      },
      error: () => {
        this.isLoadingClassrooms.set(false);
        this.toastService.error('Error al cargar las aulas');
      }
    });
  }

  private loadAssignedPersons(): void {
    this.assignmentsService.getPersonsByProfessional(this.professionalId).subscribe({
      next: (data) => this.personsChange.emit(data),
      error: () => this.toastService.error('Error al cargar personas asignadas'),
    });
  }
}
