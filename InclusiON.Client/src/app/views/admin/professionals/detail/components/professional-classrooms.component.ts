import { Component, Input, Output, EventEmitter, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { map } from 'rxjs';
import { AssignmentsService, PersonsService, ToastService } from '@services';
import {
  PersonListItemResponse,
  ProfessionalPersonResponse,
  ClassroomResponse,
} from '@models';
import { SearchableSelectComponent } from '@shared/components/searchable-select/searchable-select.component';
import {
  ButtonDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormControlDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-professional-classrooms',
  standalone: true,
  imports: [
    ButtonDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormControlDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    ReactiveFormsModule,
    FormsModule,
    IconModule,
    SearchableSelectComponent,
    ConfirmModalComponent,
  ],
  templateUrl: './professional-classrooms.component.html',
  styleUrl: './professional-classrooms.component.scss',
})
export class ProfessionalClassroomsComponent implements OnInit {
  @Input({ required: true }) professionalId!: string;
  @Input() persons: ProfessionalPersonResponse[] = [];
  @Output() classroomsChange = new EventEmitter<ClassroomResponse[]>();
  @Output() classroomsCountChange = new EventEmitter<number>();
  @Output() personsChange = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly personsService = inject(PersonsService);
  private readonly toastService = inject(ToastService);

  // ── State ──────────────────────────────────────────────────────────────
  classrooms = signal<ClassroomResponse[]>([]);
  isLoadingClassrooms = signal(false);
  isSubmitting = signal(false);

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

  // ── Search fns ─────────────────────────────────────────────────────────
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

  // ── Lifecycle ──────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.loadClassrooms();
  }

  // ── Loaders ────────────────────────────────────────────────────────────
  loadClassrooms(): void {
    this.isLoadingClassrooms.set(true);
    this.assignmentsService.getClassroomsByProfessional(this.professionalId).subscribe({
      next: (data) => {
        this.classrooms.set(data);
        this.isLoadingClassrooms.set(false);
        this.classroomsChange.emit(data);
        this.classroomsCountChange.emit(data.length);
      },
      error: () => {
        this.isLoadingClassrooms.set(false);
        this.toastService.error('Error al cargar las aulas');
      }
    });
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
        this.loadClassrooms();
        this.personsChange.emit();
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
          this.personsChange.emit();
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
}
