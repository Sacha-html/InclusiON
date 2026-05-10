import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
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
  templateUrl: './professional-institutions.component.html',
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
