import { Component, Input, Output, EventEmitter, inject, OnInit, signal, computed } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AssignmentsService, ToastService } from '@services';
import { ProfessionalsService } from '@services/professionals.service';
import {
  ProfessionalPersonResponse,
  ProfessionalListItemResponse,
  ClassroomResponse,
} from '@models';
import {
  ButtonDirective,
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';

@Component({
  selector: 'app-professional-persons',
  standalone: true,
  imports: [
    ButtonDirective,
    FormSelectDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    ReactiveFormsModule,
    FormsModule,
    IconModule,
    DataTableComponent,
  ],
  templateUrl: './professional-persons.component.html',
  styleUrl: './professional-persons.component.scss',
})
export class ProfessionalPersonsComponent implements OnInit {
  @Input({ required: true }) professionalId!: string;
  @Input() persons: ProfessionalPersonResponse[] = [];
  @Output() personsChange = new EventEmitter<ProfessionalPersonResponse[]>();
  @Output() classroomsCountChange = new EventEmitter<number>();

  private readonly fb = inject(FormBuilder);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly toastService = inject(ToastService);

  // ── State ──────────────────────────────────────────────────────────────
  showTransferModal = signal(false);
  showMovePersonModal = signal(false);
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

  // ── Forms ──────────────────────────────────────────────────────────────
  movePersonForm: FormGroup = this.fb.group({
    classroomId: [null],
  });

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
    if (event.action === 'transfer') {
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

  // ── Move person to classroom ────────────────────────────────────────────
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
          if (classroomId) {
            this.selectedClassroomIdFilter.set(classroomId);
          }
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

  // ── Loaders ────────────────────────────────────────────────────────────
  loadClassrooms(): void {
    this.isLoadingClassrooms.set(true);
    this.assignmentsService.getClassroomsByProfessional(this.professionalId).subscribe({
      next: (data) => {
        this.classrooms.set(data);
        this.isLoadingClassrooms.set(false);
        this.classroomsCountChange.emit(data.length);
      },
      error: () => {
        this.isLoadingClassrooms.set(false);
      }
    });
  }

  private loadAssignedPersons(): void {
    this.assignmentsService.getPersonsByProfessional(this.professionalId).subscribe({
      next: (data) => {
        this.persons = data;
        this.personsChange.emit(data);
      },
      error: () => this.toastService.error('Error al cargar alumnos asignados'),
    });
  }
}
