import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { AssignmentsService, InstitutionsService, ToastService } from '@services';
import { InstitutionResponse, ProfessionalInstitutionResponse } from '@models';
import {
  ButtonDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  TableDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-institutions',
  standalone: true,
  imports: [
    ButtonDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    TableDirective,
    DatePipe,
  ],
  template: `
    <div class="d-flex justify-content-end mb-3">
      <button cButton color="primary" size="sm" (click)="openAssignInstitutionModal()">Agregar institucion</button>
    </div>
    @if (institutions.length === 0) {
      <p class="text-body-secondary text-center py-3">No hay instituciones asignadas.</p>
    } @else {
      <table cTable hover>
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Fecha asignacion</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          @for (inst of institutions; track inst.institutionId) {
            <tr>
              <td>{{ inst.institutionName }}</td>
              <td>{{ inst.assignedAt | date:'dd/MM/yyyy HH:mm' }}</td>
              <td>
                <button cButton color="danger" size="sm" variant="ghost" (click)="openRemoveInstitutionModal(inst)">Remover</button>
              </td>
            </tr>
          }
        </tbody>
      </table>
    }

    <!-- Modal agregar institucion -->
    <c-modal [visible]="showAssignInstitutionModal()" (visibleChange)="showAssignInstitutionModal.set($event)" alignment="center">
      <c-modal-header><h5 cModalTitle>Agregar institucion</h5></c-modal-header>
      <c-modal-body>
        <div class="mb-3">
          <label cLabel>Institucion</label>
          <select cSelect (change)="selectedInstitutionId = $any($event.target).value ? +$any($event.target).value : null">
            <option value="">Seleccione una institucion...</option>
            @for (inst of availableInstitutions; track inst.id) {
              <option [value]="inst.id">{{ inst.name }}</option>
            }
          </select>
        </div>
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="cancelAssignInstitution()">Cancelar</button>
        <button cButton color="primary" (click)="confirmAssignInstitution()" [disabled]="!selectedInstitutionId">Agregar</button>
      </c-modal-footer>
    </c-modal>

    <!-- Modal remover institucion -->
    <c-modal [visible]="showRemoveInstitutionModal()" (visibleChange)="showRemoveInstitutionModal.set($event)" alignment="center">
      <c-modal-header><h5 cModalTitle>Confirmar remocion</h5></c-modal-header>
      <c-modal-body>
        @if (institutionToRemove()) {
          <p>¿Esta seguro de que desea remover la institucion <strong>{{ institutionToRemove()!.institutionName }}</strong>?</p>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="cancelRemoveInstitution()">Cancelar</button>
        <button cButton color="danger" (click)="confirmRemoveInstitution()">Remover</button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class ProfessionalInstitutionsComponent {
  @Input({ required: true }) professionalId!: string;
  @Input() institutions: ProfessionalInstitutionResponse[] = [];
  @Output() institutionsChange = new EventEmitter<ProfessionalInstitutionResponse[]>();

  private readonly assignmentsService = inject(AssignmentsService);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly toastService = inject(ToastService);

  availableInstitutions: InstitutionResponse[] = [];
  selectedInstitutionId: number | null = null;
  showAssignInstitutionModal = signal(false);
  showRemoveInstitutionModal = signal(false);
  institutionToRemove = signal<ProfessionalInstitutionResponse | null>(null);

  openAssignInstitutionModal(): void {
    this.selectedInstitutionId = null;
    this.institutionsService.getAll().subscribe({
      next: (data) => {
        const assignedIds = new Set(this.institutions.filter((i) => i.isActive).map((i) => i.institutionId));
        this.availableInstitutions = data.filter((i) => i.isActive && !assignedIds.has(i.id));
        this.showAssignInstitutionModal.set(true);
      },
      error: () => this.toastService.error('Error al cargar instituciones disponibles'),
    });
  }

  confirmAssignInstitution(): void {
    if (!this.selectedInstitutionId) return;

    this.assignmentsService
      .assignInstitution(this.professionalId, { institutionId: this.selectedInstitutionId })
      .subscribe({
        next: () => {
          this.showAssignInstitutionModal.set(false);
          this.toastService.success('Institucion asignada exitosamente');
          this.loadAssignedInstitutions();
        },
        error: () => this.toastService.error('Error al asignar institucion'),
      });
  }

  cancelAssignInstitution(): void {
    this.showAssignInstitutionModal.set(false);
  }

  openRemoveInstitutionModal(inst: ProfessionalInstitutionResponse): void {
    this.institutionToRemove.set(inst);
    this.showRemoveInstitutionModal.set(true);
  }

  confirmRemoveInstitution(): void {
    if (!this.institutionToRemove()) return;

    this.assignmentsService
      .removeInstitutionAssignment(this.professionalId, this.institutionToRemove()!.institutionId)
      .subscribe({
        next: () => {
          this.showRemoveInstitutionModal.set(false);
          this.institutionToRemove.set(null);
          this.toastService.success('Institucion removida exitosamente');
          this.loadAssignedInstitutions();
        },
        error: () => {
          this.showRemoveInstitutionModal.set(false);
          this.toastService.error('Error al remover la institucion');
        },
      });
  }

  cancelRemoveInstitution(): void {
    this.showRemoveInstitutionModal.set(false);
    this.institutionToRemove.set(null);
  }

  private loadAssignedInstitutions(): void {
    this.assignmentsService.getInstitutionsByProfessional(this.professionalId).subscribe({
      next: (data) => this.institutionsChange.emit(data),
      error: () => this.toastService.error('Error al cargar instituciones asignadas'),
    });
  }
}
