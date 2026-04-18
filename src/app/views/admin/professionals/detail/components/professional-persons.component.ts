import { Component, Input, Output, EventEmitter, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
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
    FormLabelDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    TableDirective,
    ReactiveFormsModule,
    NgSelectModule,
    IconModule,
  ],
  template: `
    <div class="d-flex justify-content-end mb-3">
      <button cButton color="primary" size="sm" (click)="openAssignPersonModal()">
        <svg cIcon name="cil-user-follow" class="me-1"></svg>
        Asignar persona
      </button>
    </div>

    @if (persons.length === 0) {
      <div class="text-center text-body-secondary py-4">
        <svg cIcon name="cil-people" size="3xl" class="mb-2 d-block mx-auto opacity-50"></svg>
        <p class="mb-0">No hay personas asignadas.</p>
      </div>
    } @else {
      <table cTable hover responsive>
        <thead>
          <tr>
            <th>Nombre</th>
            <th class="text-center">Principal</th>
            <th class="text-center">Supervisa login</th>
            <th class="text-center">Estado</th>
            <th class="text-end">Acciones</th>
          </tr>
        </thead>
        <tbody>
          @for (person of persons; track person.personId) {
            <tr>
              <td class="align-middle">{{ person.personFullName }}</td>
              <td class="text-center align-middle">
                <c-badge [color]="person.isPrimaryProfessional ? 'success' : 'secondary'">
                  {{ person.isPrimaryProfessional ? 'Sí' : 'No' }}
                </c-badge>
              </td>
              <td class="text-center align-middle">
                <c-badge [color]="person.canSuperviseLogin ? 'success' : 'secondary'">
                  {{ person.canSuperviseLogin ? 'Sí' : 'No' }}
                </c-badge>
              </td>
              <td class="text-center align-middle">
                <c-badge [color]="person.isActive ? 'success' : 'danger'">
                  {{ person.isActive ? 'Activo' : 'Inactivo' }}
                </c-badge>
              </td>
              <td class="text-end align-middle">
                @if (person.isActive) {
                  <button cButton color="danger" size="sm" variant="ghost" (click)="openDeactivatePersonModal(person)">
                    <svg cIcon name="cil-user-unfollow" class="me-1"></svg>
                    Desasignar
                  </button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    }

    <!-- Modal asignar persona -->
    <c-modal [visible]="showAssignPersonModal()" (visibleChange)="showAssignPersonModal.set($event)" alignment="center" size="lg">
      <c-modal-header>
        <h5 cModalTitle>
          <svg cIcon name="cil-user-follow" class="me-2"></svg>
          Asignar persona
        </h5>
      </c-modal-header>
      <c-modal-body>
        <form [formGroup]="assignPersonForm">
          <div class="mb-4">
            <label cLabel class="fw-semibold mb-1">Persona</label>
            <ng-select
              [items]="availablePersons"
              bindValue="id"
              formControlName="personId"
              placeholder="Buscar por nombre o documento..."
              [searchFn]="searchPersonFn"
              notFoundText="No se encontraron personas"
              loadingText="Cargando..."
              appendTo="body"
            >
              <ng-template ng-label-tmp let-item="item">
                <strong>{{ item.fullName }}</strong>
              </ng-template>
              <ng-template ng-option-tmp let-item="item">
                <div class="d-flex flex-column py-1">
                  <strong>{{ item.fullName }}</strong>
                  <small class="text-body-secondary">
                    {{ item.disabilityTypeName ?? 'Sin tipo' }}{{ item.documentNumber ? ' · DNI ' + item.documentNumber : '' }}
                  </small>
                </div>
              </ng-template>
            </ng-select>
          </div>

          <div class="border rounded p-3 bg-body-tertiary">
            <p class="text-body-secondary small fw-semibold mb-2 text-uppercase" style="letter-spacing:.04em">Permisos de la asignación</p>
            <c-form-check class="mb-2">
              <input cFormCheckInput type="checkbox" id="isPrimaryProfessional" formControlName="isPrimaryProfessional" />
              <label cFormCheckLabel for="isPrimaryProfessional" class="fw-medium">
                Profesional principal
                <small class="d-block text-body-secondary fw-normal">Este profesional aparece como referente principal de la persona</small>
              </label>
            </c-form-check>
            <c-form-check>
              <input cFormCheckInput type="checkbox" id="canSuperviseLogin" formControlName="canSuperviseLogin" />
              <label cFormCheckLabel for="canSuperviseLogin" class="fw-medium">
                Puede supervisar login
                <small class="d-block text-body-secondary fw-normal">Este profesional puede asistir el inicio de sesión de la persona</small>
              </label>
            </c-form-check>
          </div>
        </form>
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" variant="outline" (click)="cancelAssignPerson()" [disabled]="isSubmitting()">Cancelar</button>
        <button cButton color="primary" (click)="confirmAssignPerson()" [disabled]="!assignPersonForm.value.personId || isSubmitting()">
          @if (isSubmitting()) {
            <c-spinner size="sm" class="me-1"></c-spinner>
            Asignando...
          } @else {
            Asignar
          }
        </button>
      </c-modal-footer>
    </c-modal>

    <!-- Modal desasignar persona -->
    <c-modal [visible]="showDeactivatePersonModal()" (visibleChange)="showDeactivatePersonModal.set($event)" alignment="center">
      <c-modal-header>
        <h5 cModalTitle>Confirmar desasignación</h5>
      </c-modal-header>
      <c-modal-body>
        @if (personToDeactivate()) {
          <p class="mb-0">
            ¿Está seguro de que desea desasignar a
            <strong>{{ personToDeactivate()!.personFullName }}</strong>?
          </p>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" variant="outline" (click)="cancelDeactivatePerson()">Cancelar</button>
        <button cButton color="danger" (click)="confirmDeactivatePerson()">Desasignar</button>
      </c-modal-footer>
    </c-modal>
  `,
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

  assignPersonForm: FormGroup = this.fb.group({
    personId: [null],
    isPrimaryProfessional: [false],
    canSuperviseLogin: [false],
  });

  searchPersonFn = (term: string, item: PersonListItemResponse): boolean => {
    const lower = term.toLowerCase();
    return (
      (item.fullName?.toLowerCase().includes(lower) ||
        item.documentNumber?.toLowerCase().includes(lower)) ??
      false
    );
  };

  ngOnInit(): void {}

  openAssignPersonModal(): void {
    this.assignPersonForm.reset({ personId: null, isPrimaryProfessional: false, canSuperviseLogin: false });
    this.personsService.getPersons({ pageSize: 1000, isActive: true }).subscribe({
      next: (response) => {
        const assignedIds = new Set(this.persons.filter((p) => p.isActive).map((p) => p.personId));
        this.availablePersons = response.data.filter((p) => !assignedIds.has(p.id));
        this.showAssignPersonModal.set(true);
      },
      error: () => this.toastService.error('Error al cargar personas disponibles'),
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
