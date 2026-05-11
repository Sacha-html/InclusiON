import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AdminInstitutionsService, InstitutionsService, ProfessionalsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { AdminUserResponse, InstitutionResponse, ProfessionalListItemResponse } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { DatePipe } from '@angular/common';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';

@Component({
  selector: 'app-detail',
  imports: [
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    ButtonDirective,
    BadgeComponent,
    SpinnerComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    DatePipe,
    DataTableComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly adminInstitutionsService = inject(AdminInstitutionsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly toastService = inject(ToastService);

  institution: InstitutionResponse | null = null;
  admins: AdminUserResponse[] = [];
  professionals: ProfessionalListItemResponse[] = [];
  loading = true;
  showConfirmModal = false;
  deactivateLoading = false;
  showReactivateModal = false;
  reactivateLoading = false;

  adminCols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre' },
    { key: 'email', label: 'Email' },
    { key: 'isActive', label: 'Estado', type: 'badge', badgeMap: { 'true': { color: 'success', label: 'Activo' }, 'false': { color: 'danger', label: 'Inactivo' } } },
  ];

  professionalCols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre' },
    { key: 'specialty', label: 'Especialidad' },
    { key: 'email', label: 'Email' },
    { key: 'isActive', label: 'Estado', type: 'badge', badgeMap: { 'true': { color: 'success', label: 'Activo' }, 'false': { color: 'danger', label: 'Inactivo' } } },
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate([AppRoutes.Admin.Institutions]);
      return;
    }

    this.institutionsService.getById(id).subscribe({
      next: (institution) => {
        if (!institution) {
          this.toastService.error('Institución no encontrada');
          this.router.navigate([AppRoutes.Admin.Institutions]);
          return;
        }
        this.institution = institution;
        forkJoin({
          admins: this.adminInstitutionsService.getAdmins(1, 500),
          professionals: this.professionalsService.getProfessionals({ institutionId: institution.id, pageSize: 100 }),
        }).subscribe({
          next: ({ admins, professionals }) => {
            this.admins = admins.data.filter((a) =>
              a.institutions.some((i) => i.institutionId === institution.id),
            );
            this.professionals = professionals.data;
            this.loading = false;
          },
          error: () => {
            this.toastService.error('Error al cargar los datos de la institución');
            this.router.navigate([AppRoutes.Admin.Institutions]);
          },
        });
      },
      error: () => {
        this.toastService.error('Error al cargar los datos de la institución');
        this.router.navigate([AppRoutes.Admin.Institutions]);
      },
    });
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Institutions]);
  }

  goToEdit(): void {
    this.router.navigate([AppRoutes.Admin.Institutions, this.institution!.encryptedId, 'edit']);
  }

  deactivate(): void {
    this.showConfirmModal = true;
  }

  confirmDeactivate(): void {
    if (!this.institution) return;
    this.deactivateLoading = true;

    this.institutionsService.patchStatus(this.institution.encryptedId, false).subscribe({
      next: (updated) => {
        this.institution = updated;
        this.showConfirmModal = false;
        this.deactivateLoading = false;
        this.toastService.success('Institución dada de baja exitosamente.');
      },
      error: (err) => {
        this.showConfirmModal = false;
        this.deactivateLoading = false;
        const msg = err?.userMessage ?? 'Error al dar de baja la institución.';
        this.toastService.error(msg);
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
  }

  reactivate(): void {
    this.showReactivateModal = true;
  }

  confirmReactivate(): void {
    if (!this.institution) return;
    this.reactivateLoading = true;

    this.institutionsService.patchStatus(this.institution.encryptedId, true).subscribe({
      next: (updated) => {
        this.institution = updated;
        this.showReactivateModal = false;
        this.reactivateLoading = false;
        this.toastService.success('Institución reactivada exitosamente.');
      },
      error: (err) => {
        this.showReactivateModal = false;
        this.reactivateLoading = false;
        const msg = err?.userMessage ?? 'Error al reactivar la institución.';
        this.toastService.error(msg);
      },
    });
  }

  cancelReactivate(): void {
    this.showReactivateModal = false;
  }
}
