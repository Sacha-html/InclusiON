import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AdminInstitutionsService, InstitutionsService, ProfessionalsService, ToastService } from '@services';
import { AdminUserResponse, InstitutionResponse, ProfessionalListItemResponse } from '../../../../models';
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
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';

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

  adminCols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre' },
    { key: 'email', label: 'Email' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
  ];

  professionalCols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre' },
    { key: 'specialty', label: 'Especialidad' },
    { key: 'email', label: 'Email' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
  ];

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.router.navigate(['/admin/institutions']);
      return;
    }

    forkJoin({
      institution: this.institutionsService.getById(id),
      admins: this.adminInstitutionsService.getAdmins(),
      professionals: this.professionalsService.getProfessionals({ institutionId: id, pageSize: 100 }),
    }).subscribe({
      next: ({ institution, admins, professionals }) => {
        if (!institution) {
          this.toastService.error('Institución no encontrada');
          this.router.navigate(['/admin/institutions']);
          return;
        }
        this.institution = institution;
        this.admins = admins.filter((a) =>
          a.institutions.some((i) => i.institutionId === id),
        );
        this.professionals = professionals.data;
        this.loading = false;
      },
      error: () => {
        this.toastService.error('Error al cargar los datos de la institución');
        this.router.navigate(['/admin/institutions']);
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/institutions']);
  }

  goToEdit(): void {
    this.router.navigate(['/admin/institutions', this.institution!.id, 'edit']);
  }

  deactivate(): void {
    this.showConfirmModal = true;
  }

  confirmDeactivate(): void {
    if (!this.institution) return;
    this.deactivateLoading = true;

    this.institutionsService.patchStatus(this.institution.id, false).subscribe({
      next: (updated) => {
        this.institution = updated;
        this.showConfirmModal = false;
        this.deactivateLoading = false;
        this.toastService.success('Institución dada de baja exitosamente.');
      },
      error: (err) => {
        this.showConfirmModal = false;
        this.deactivateLoading = false;
        const msg = err?.error?.message ?? 'Error al dar de baja la institución.';
        this.toastService.error(msg);
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
  }
}
