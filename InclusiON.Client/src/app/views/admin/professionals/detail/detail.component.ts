import { Component, inject, OnInit, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AssignmentsService, ProfessionalsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ProfessionalPersonResponse, ProfessionalResponse } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
} from '@coreui/angular';
import { ProfessionalBasicInfoComponent } from './components/professional-basic-info.component';
import { ProfessionalPersonsComponent } from './components/professional-persons.component';
import { ProfessionalUserComponent } from './components/professional-user.component';
import { ProfessionalReportsComponent } from './components/professional-reports.component';
import { ModalBodyComponent, ModalComponent, ModalFooterComponent, ModalHeaderComponent } from '@coreui/angular';

@Component({
  selector: 'app-detail',
  imports: [
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ButtonDirective,
    BadgeComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    FormsModule,
    ProfessionalBasicInfoComponent,
    ProfessionalPersonsComponent,
    ProfessionalUserComponent,
    ProfessionalReportsComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly toastService = inject(ToastService);

  activeTab: 'datos' | 'personas' | 'usuario' | 'reportes' = 'datos';

  professional: ProfessionalResponse | null = null;
  showConfirmModal = false;

  // Validate / reactivate
  showValidateModal = false;
  showReactivateModal = false;
  validateApprove = true;
  validateObservation = '';
  isValidating = false;

  assignedPersons: ProfessionalPersonResponse[] = [];

  ngOnInit(): void {
    const tab = this.route.snapshot.queryParams['tab'];
    if (tab && ['datos', 'personas', 'usuario', 'reportes'].includes(tab)) {
      this.activeTab = tab as 'datos' | 'personas' | 'usuario' | 'reportes';
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.professionalsService.getProfessionalById(id).subscribe({
        next: (data) => {
          this.professional = data;
          this.assignmentsService.getPersonsByProfessional(id).subscribe({
            next: (persons: ProfessionalPersonResponse[]) => this.assignedPersons = persons,
          });
        },
        error: () => this.router.navigate([AppRoutes.Admin.Professionals]),
      });
    }
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Professionals]);
  }

  onPersonsChange(persons: ProfessionalPersonResponse[]): void {
    this.assignedPersons = persons;
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
      error: (err) => {
        this.showConfirmModal = false;
        if (err?.errorCode === 710 || err?.errorCode === 'HAS_PENDING_REPORTS') {
          this.toastService.error('Este profesional tiene informes pendientes. Debe reasignarlos o finalizarlos antes de proceder con la baja.');
          
          /* TODO: Reemplazar este aviso por un Componente Modal interactivo que permita
             seleccionar un nuevo profesional y transferir los reportes.
             this.showTransferReportsModal = true;
          */
        } else {
          this.toastService.error(err?.userMessage || 'Error al desactivar el profesional');
        }
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
  }

  openValidate(approve: boolean): void {
    this.validateApprove = approve;
    this.validateObservation = '';
    this.showValidateModal = true;
  }

  confirmValidate(): void {
    if (!this.professional) return;
    this.isValidating = true;
    this.professionalsService.validateProfessional(this.professional.id, {
      isApproved: this.validateApprove,
      observation: this.validateObservation || undefined,
    }).subscribe({
      next: () => {
        this.professional!.statusName = this.validateApprove ? 'Aprobado' : 'Rechazado';
        this.professional!.isActive = this.validateApprove;
        this.showValidateModal = false;
        this.isValidating = false;
        this.toastService.success(this.validateApprove ? 'Profesional aprobado exitosamente' : 'Profesional rechazado');
      },
      error: () => {
        this.showValidateModal = false;
        this.isValidating = false;
        this.toastService.error('Error al procesar la validación');
      },
    });
  }

  openReactivate(): void {
    this.showReactivateModal = true;
  }

  confirmReactivate(): void {
    if (!this.professional) return;
    this.isValidating = true;
    this.professionalsService.reactivateProfessional(this.professional.id).subscribe({
      next: () => {
        this.professional!.statusName = 'Aprobado';
        this.professional!.isActive = true;
        this.showReactivateModal = false;
        this.isValidating = false;
        this.toastService.success('Profesional reactivado exitosamente');
      },
      error: () => {
        this.showReactivateModal = false;
        this.isValidating = false;
        this.toastService.error('Error al reactivar el profesional');
      },
    });
  }
}
