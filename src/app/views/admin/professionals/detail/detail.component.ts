import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { AssignmentsService, InstitutionsService, PersonsService, ProfessionalsService, ToastService } from '@services';
import {
  InstitutionResponse,
  PersonListItemResponse,
  ProfessionalInstitutionResponse,
  ProfessionalPersonResponse,
  ProfessionalResponse,
} from '../../../../models';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
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
  RowComponent,
  TableDirective,
  BadgeComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-detail',
  imports: [
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    FormSelectDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    ButtonDirective,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ReactiveFormsModule,
    TableDirective,
    BadgeComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly personsService = inject(PersonsService);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly toastService = inject(ToastService);

  activeTab: 'datos' | 'personas' | 'instituciones' = 'datos';

  professional: ProfessionalResponse | null = null;
  showConfirmModal = false;

  // Persons assignments
  assignedPersons: ProfessionalPersonResponse[] = [];
  availablePersons: PersonListItemResponse[] = [];
  showAssignPersonModal = false;
  assignPersonForm: FormGroup = this.fb.group({
    personId: [''],
    isPrimaryProfessional: [false],
    canSuperviseLogin: [false],
  });

  // Deactivate person assignment
  showDeactivatePersonModal = false;
  personToDeactivate: ProfessionalPersonResponse | null = null;

  // Institution assignments
  assignedInstitutions: ProfessionalInstitutionResponse[] = [];
  availableInstitutions: InstitutionResponse[] = [];
  showAssignInstitutionModal = false;
  selectedInstitutionId: number | null = null;

  // Remove institution assignment
  showRemoveInstitutionModal = false;
  institutionToRemove: ProfessionalInstitutionResponse | null = null;

  ngOnInit(): void {
    const tab = this.route.snapshot.queryParams['tab'];
    if (tab && ['datos', 'personas', 'instituciones'].includes(tab)) {
      this.activeTab = tab;
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.professionalsService.getProfessionalById(id).subscribe({
        next: (data) => {
          this.professional = data;
          this.loadAssignedPersons();
          this.loadAssignedInstitutions();
        },
        error: () => this.router.navigate(['/admin/professionals']),
      });
    }
  }

  // ---- Professional actions ----

  goToEdit(): void {
    if (this.professional) {
      this.router.navigate(['/admin/professionals', this.professional.id, 'edit']);
    }
  }

  deactivate(): void {
    this.showConfirmModal = true;
  }

  confirmDeactivate(): void {
    if (!this.professional) return;

    this.professionalsService.deactivateProfessional(this.professional.id).subscribe({
      next: () => {
        this.professional!.isActive = false;
        this.showConfirmModal = false;
        this.toastService.success('Profesional desactivado exitosamente');
      },
      error: () => {
        this.showConfirmModal = false;
        this.toastService.error('Error al desactivar el profesional');
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
  }

  // ---- Persons assignments ----

  private loadAssignedPersons(): void {
    if (!this.professional) return;
    this.assignmentsService.getPersonsByProfessional(this.professional.id).subscribe({
      next: (data) => (this.assignedPersons = data),
      error: () => this.toastService.error('Error al cargar personas asignadas'),
    });
  }

  openAssignPersonModal(): void {
    if (!this.professional) return;
    this.assignPersonForm.reset({ personId: '', isPrimaryProfessional: false, canSuperviseLogin: false });
    this.personsService.getPersons({ pageSize: 1000, isActive: true }).subscribe({
      next: (response) => {
        const assignedIds = new Set(this.assignedPersons.filter((p) => p.isActive).map((p) => p.personId));
        this.availablePersons = response.data.data.filter((p) => !assignedIds.has(p.id));
        this.showAssignPersonModal = true;
      },
      error: () => this.toastService.error('Error al cargar personas disponibles'),
    });
  }

  confirmAssignPerson(): void {
    if (!this.professional) return;
    const val = this.assignPersonForm.value;
    if (!val.personId) return;

    this.assignmentsService
      .assignPerson(this.professional.id, {
        personId: val.personId,
        isPrimaryProfessional: val.isPrimaryProfessional ?? false,
        canSuperviseLogin: val.canSuperviseLogin ?? false,
      })
      .subscribe({
        next: () => {
          this.showAssignPersonModal = false;
          this.toastService.success('Persona asignada exitosamente');
          this.loadAssignedPersons();
        },
        error: () => this.toastService.error('Error al asignar persona'),
      });
  }

  cancelAssignPerson(): void {
    this.showAssignPersonModal = false;
  }

  openDeactivatePersonModal(person: ProfessionalPersonResponse): void {
    this.personToDeactivate = person;
    this.showDeactivatePersonModal = true;
  }

  confirmDeactivatePerson(): void {
    if (!this.professional || !this.personToDeactivate) return;

    this.assignmentsService
      .deactivatePersonAssignment(this.professional.id, this.personToDeactivate.personId)
      .subscribe({
        next: () => {
          this.showDeactivatePersonModal = false;
          this.personToDeactivate = null;
          this.toastService.success('Asignacion desactivada exitosamente');
          this.loadAssignedPersons();
        },
        error: () => {
          this.showDeactivatePersonModal = false;
          this.toastService.error('Error al desactivar la asignacion');
        },
      });
  }

  cancelDeactivatePerson(): void {
    this.showDeactivatePersonModal = false;
    this.personToDeactivate = null;
  }

  // ---- Institution assignments ----

  private loadAssignedInstitutions(): void {
    if (!this.professional) return;
    this.assignmentsService.getInstitutionsByProfessional(this.professional.id).subscribe({
      next: (data) => (this.assignedInstitutions = data),
      error: () => this.toastService.error('Error al cargar instituciones asignadas'),
    });
  }

  openAssignInstitutionModal(): void {
    if (!this.professional) return;
    this.selectedInstitutionId = null;
    this.institutionsService.getAll().subscribe({
      next: (data) => {
        const assignedIds = new Set(this.assignedInstitutions.filter((i) => i.isActive).map((i) => i.institutionId));
        this.availableInstitutions = data.filter((i) => i.isActive && !assignedIds.has(i.id));
        this.showAssignInstitutionModal = true;
      },
      error: () => this.toastService.error('Error al cargar instituciones disponibles'),
    });
  }

  confirmAssignInstitution(): void {
    if (!this.professional || !this.selectedInstitutionId) return;

    this.assignmentsService
      .assignInstitution(this.professional.id, { institutionId: this.selectedInstitutionId })
      .subscribe({
        next: () => {
          this.showAssignInstitutionModal = false;
          this.toastService.success('Institucion asignada exitosamente');
          this.loadAssignedInstitutions();
        },
        error: () => this.toastService.error('Error al asignar institucion'),
      });
  }

  cancelAssignInstitution(): void {
    this.showAssignInstitutionModal = false;
  }

  openRemoveInstitutionModal(inst: ProfessionalInstitutionResponse): void {
    this.institutionToRemove = inst;
    this.showRemoveInstitutionModal = true;
  }

  confirmRemoveInstitution(): void {
    if (!this.professional || !this.institutionToRemove) return;

    this.assignmentsService
      .removeInstitutionAssignment(this.professional.id, this.institutionToRemove.institutionId)
      .subscribe({
        next: () => {
          this.showRemoveInstitutionModal = false;
          this.institutionToRemove = null;
          this.toastService.success('Institucion removida exitosamente');
          this.loadAssignedInstitutions();
        },
        error: () => {
          this.showRemoveInstitutionModal = false;
          this.toastService.error('Error al remover la institucion');
        },
      });
  }

  cancelRemoveInstitution(): void {
    this.showRemoveInstitutionModal = false;
    this.institutionToRemove = null;
  }

  // ---- Helpers ----

  formatDate(date: string | null | undefined): string {
    if (!date) return 'Sin especificar';
    const d = new Date(date);
    if (isNaN(d.getTime())) return 'Sin especificar';
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  formatDateTime(date: string | null | undefined): string {
    if (!date) return 'Sin especificar';
    const d = new Date(date);
    if (isNaN(d.getTime())) return 'Sin especificar';
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })
      + ' ' + d.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  }

  goBack(): void {
    this.router.navigate(['/admin/professionals']);
  }
}
