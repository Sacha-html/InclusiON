import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminInstitutionsService, AuthService, InstitutionsService, ToastService } from '@services';
import { AdminInstitutionResponse, InstitutionResponse } from '@models';

import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  TableDirective, BadgeComponent, SpinnerComponent,
  AlertComponent, ButtonDirective, FormSelectDirective,
  ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
} from '@coreui/angular';
import { IfGlobalAdminDirective } from '@shared/directives/if-global-admin.directive';

@Component({
  selector: 'app-admin-institutions',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    TableDirective, BadgeComponent, SpinnerComponent,
    AlertComponent, ButtonDirective, FormSelectDirective,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    IfGlobalAdminDirective,
  ],
  templateUrl: './admin-institutions.component.html',
  styleUrl: './admin-institutions.component.scss',
})
export class AdminInstitutionsComponent implements OnInit {
  private readonly adminInstitutionsService = inject(AdminInstitutionsService);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  myInstitutions: AdminInstitutionResponse[] = [];
  allInstitutions: InstitutionResponse[] = [];
  isLoading = true;
  isSaving = false;
  isGlobalAdmin = false;

  showAssignModal = false;
  selectedInstitutionId: string | null = null;

  showRemoveModal = false;
  institutionToRemove: AdminInstitutionResponse | null = null;

  get currentUserId(): string {
    return this.authService.getCurrentUser()?.id ?? '';
  }

  get availableInstitutions(): InstitutionResponse[] {
    const assignedIds = new Set(this.myInstitutions.map((i) => i.encryptedInstitutionId));
    return this.allInstitutions.filter((i) => i.isActive && !assignedIds.has(i.encryptedId));
  }

  ngOnInit(): void {
    this.isGlobalAdmin = this.authService.isGlobalAdmin();
    this.loadData();
  }

  openAssignModal(): void {
    this.selectedInstitutionId = null;
    this.showAssignModal = true;
  }

  confirmAssign(): void {
    if (!this.selectedInstitutionId || !this.currentUserId) return;

    this.isSaving = true;
    this.adminInstitutionsService.assign(this.currentUserId, this.selectedInstitutionId).subscribe({
      next: () => {
        this.toastService.success('Institucion asignada exitosamente');
        this.showAssignModal = false;
        this.isSaving = false;
        this.loadMyInstitutions();
      },
      error: () => {
        this.toastService.error('Error al asignar la institucion');
        this.isSaving = false;
      },
    });
  }

  cancelAssign(): void {
    this.showAssignModal = false;
    this.selectedInstitutionId = null;
  }

  openRemoveModal(institution: AdminInstitutionResponse): void {
    this.institutionToRemove = institution;
    this.showRemoveModal = true;
  }

  confirmRemove(): void {
    if (!this.institutionToRemove || !this.currentUserId) return;

    this.isSaving = true;
    this.adminInstitutionsService.remove(this.currentUserId, this.institutionToRemove.encryptedInstitutionId).subscribe({
      next: () => {
        this.toastService.success('Institucion desasignada exitosamente');
        this.showRemoveModal = false;
        this.institutionToRemove = null;
        this.isSaving = false;
        this.loadMyInstitutions();
      },
      error: () => {
        this.toastService.error('Error al desasignar la institucion');
        this.isSaving = false;
      },
    });
  }

  cancelRemove(): void {
    this.showRemoveModal = false;
    this.institutionToRemove = null;
  }

  private loadData(): void {
    this.isLoading = true;
    this.institutionsService.getAll().subscribe({
      next: (data) => {
        this.allInstitutions = data;
        this.loadMyInstitutions();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar las instituciones');
      },
    });
  }

  private loadMyInstitutions(): void {
    this.adminInstitutionsService.getMyInstitutions().subscribe({
      next: (data) => {
        this.myInstitutions = data;
        this.isLoading = false;
      },
      error: () => {
        this.myInstitutions = [];
        this.isLoading = false;
      },
    });
  }
}
