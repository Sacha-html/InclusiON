import { Component, inject, OnInit, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AssignmentsService, ProfessionalsService, ToastService } from '@services';
import { ProfessionalInstitutionResponse, ProfessionalPersonResponse, ProfessionalResponse } from '../../../../models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
} from '@coreui/angular';
import { ProfessionalBasicInfoComponent } from './components/professional-basic-info.component';
import { ProfessionalPersonsComponent } from './components/professional-persons.component';
import { ProfessionalInstitutionsComponent } from './components/professional-institutions.component';
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
    ProfessionalBasicInfoComponent,
    ProfessionalPersonsComponent,
    ProfessionalInstitutionsComponent,
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
  private readonly toastService = inject(ToastService);

  activeTab: 'datos' | 'personas' | 'instituciones' | 'usuario' | 'reportes' = 'datos';

  professional: ProfessionalResponse | null = null;
  showConfirmModal = false;

  assignedPersons: ProfessionalPersonResponse[] = [];
  assignedInstitutions: ProfessionalInstitutionResponse[] = [];

  ngOnInit(): void {
    const tab = this.route.snapshot.queryParams['tab'];
    if (tab && ['datos', 'personas', 'instituciones', 'usuario', 'reportes'].includes(tab)) {
      this.activeTab = tab as 'datos' | 'personas' | 'instituciones' | 'usuario' | 'reportes';
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.professionalsService.getProfessionalById(id).subscribe({
        next: (data) => {
          this.professional = data;
        },
        error: () => this.router.navigate(['/admin/professionals']),
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/admin/professionals']);
  }

  onPersonsChange(persons: ProfessionalPersonResponse[]): void {
    this.assignedPersons = persons;
  }

  onInstitutionsChange(institutions: ProfessionalInstitutionResponse[]): void {
    this.assignedInstitutions = institutions;
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
}
