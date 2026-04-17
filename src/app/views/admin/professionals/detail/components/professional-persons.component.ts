import { Component, Input, Output, EventEmitter, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
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
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  TableDirective,
} from '@coreui/angular';

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
    FormSelectDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    TableDirective,
    ReactiveFormsModule,
  ],
  template: `
    <div class="d-flex justify-content-end mb-3">
      <button cButton color="primary" size="sm" (click)="openAssignPersonModal()">Asignar persona</button>
    </div>
    @if (persons.length === 0) {
      <p class="text-body-secondary text-center py-3">No hay personas asignadas.</p>
    } @else {
      <table cTable hover>
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Principal</th>
            <th>Supervisa login</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          @for (person of persons; track person.personId) {
            <tr>
              <td>{{ person.personFullName }}</td>
              <td>
                <c-badge [color]="person.isPrimaryProfessional ? 'success' : 'secondary'">
                  {{ person.isPrimaryProfessional ? 'Si' : 'No' }}
                </c-badge>
              </td>
              <td>
                <c-badge [color]="person.canSuperviseLogin ? 'success' : 'secondary'">
                  {{ person.canSuperviseLogin ? 'Si' : 'No' }}
                </c-badge>
              </td>
              <td>
                <c-badge [color]="person.isActive ? 'success' : 'danger'">
                  {{ person.isActive ? 'Activo' : 'Inactivo' }}
                </c-badge>
              </td>
              <td>
                @if (person.isActive) {
                  <button cButton color="danger" size="sm" variant="ghost" (click)="openDeactivatePersonModal(person)">Desasignar</button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    }

    <!-- Modal asignar persona -->
    <c-modal [visible]="showAssignPersonModal()" (visibleChange)="showAssignPersonModal.set($event)" alignment="center">
      <c-modal-header><h5 cModalTitle>Asignar persona</h5></c-modal-header>
      <c-modal-body>
        <form [formGroup]="assignPersonForm">
          <div class="mb-3">
            <label cLabel>Persona</label>
            <input cFormControl
                   placeholder="Buscar por nombre o documento (min. 3 caracteres)..."
                   [value]="personSearchTerm()"
                   (input)="filterPersons($any($event.target).value)"
                   class="mb-1" />
            @if (filteredPersons().length > 0) {
              <select cSelect formControlName="personId" size="5" class="mt-2">
                @for (person of filteredPersons(); track person.id) {
                  <option [value]="person.id">{{ person.fullName }} — {{ person.disabilityTypeName ?? 'Sin tipo' }}</option>
                }
              </select>
            } @else if (personSearchTerm().length >= 3) {
              <small class="text-body-secondary">No se encontraron personas.</small>
            } @else if (personSearchTerm().length > 0) {
              <small class="text-body-secondary">Escribi al menos 3 caracteres para buscar.</small>
            }
          </div>
          <c-form-check>
            <input cFormCheckInput type="checkbox" id="isPrimaryProfessional" formControlName="isPrimaryProfessional" />
            <label cFormCheckLabel for="isPrimaryProfessional">Profesional principal</label>
          </c-form-check>
          <c-form-check class="mt-2">
            <input cFormCheckInput type="checkbox" id="canSuperviseLogin" formControlName="canSuperviseLogin" />
            <label cFormCheckLabel for="canSuperviseLogin">Puede supervisar login</label>
          </c-form-check>
        </form>
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="cancelAssignPerson()">Cancelar</button>
        <button cButton color="primary" (click)="confirmAssignPerson()" [disabled]="!assignPersonForm.value.personId">Asignar</button>
      </c-modal-footer>
    </c-modal>

    <!-- Modal desasignar persona -->
    <c-modal [visible]="showDeactivatePersonModal()" (visibleChange)="showDeactivatePersonModal.set($event)" alignment="center">
      <c-modal-header><h5 cModalTitle>Confirmar desasignacion</h5></c-modal-header>
      <c-modal-body>
        @if (personToDeactivate()) {
          <p>¿Esta seguro de que desea desasignar a <strong>{{ personToDeactivate()!.personFullName }}</strong>?</p>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="cancelDeactivatePerson()">Cancelar</button>
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
  filteredPersons = signal<PersonListItemResponse[]>([]);
  personSearchTerm = signal('');
  showAssignPersonModal = signal(false);
  showDeactivatePersonModal = signal(false);
  personToDeactivate = signal<ProfessionalPersonResponse | null>(null);

  assignPersonForm: FormGroup = this.fb.group({
    personId: [''],
    isPrimaryProfessional: [false],
    canSuperviseLogin: [false],
  });

  ngOnInit(): void {}

  openAssignPersonModal(): void {
    this.assignPersonForm.reset({ personId: '', isPrimaryProfessional: false, canSuperviseLogin: false });
    this.personSearchTerm.set('');
    this.filteredPersons.set([]);
    this.personsService.getPersons({ pageSize: 1000, isActive: true }).subscribe({
      next: (response) => {
        const assignedIds = new Set(this.persons.filter((p) => p.isActive).map((p) => p.personId));
        this.availablePersons = response.data.filter((p) => !assignedIds.has(p.id));
        this.showAssignPersonModal.set(true);
      },
      error: () => this.toastService.error('Error al cargar personas disponibles'),
    });
  }

  filterPersons(term: string): void {
    this.personSearchTerm.set(term);
    this.assignPersonForm.patchValue({ personId: '' });
    if (term.length < 3) {
      this.filteredPersons.set([]);
      return;
    }
    const lower = term.toLowerCase();
    this.filteredPersons.set(
      this.availablePersons.filter(
        (p) =>
          p.fullName?.toLowerCase().includes(lower) ||
          p.documentNumber?.toLowerCase().includes(lower)
      )
    );
  }

  confirmAssignPerson(): void {
    const val = this.assignPersonForm.value;
    if (!val.personId) return;

    this.assignmentsService
      .assignPerson(this.professionalId, {
        personId: val.personId,
        isPrimaryProfessional: val.isPrimaryProfessional ?? false,
        canSuperviseLogin: val.canSuperviseLogin ?? false,
      })
      .subscribe({
        next: () => {
          this.showAssignPersonModal.set(false);
          this.toastService.success('Persona asignada exitosamente');
          this.loadAssignedPersons();
        },
        error: () => this.toastService.error('Error al asignar persona'),
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
