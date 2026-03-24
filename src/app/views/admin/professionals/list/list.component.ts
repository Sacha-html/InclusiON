import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, ProfessionalsService, ToastService } from '@services';
import { ProfessionalListItemResponse } from '../../../../models';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { InstitutionFilterComponent } from '@shared/components/institution-filter/institution-filter.component';

@Component({
  selector: 'app-list',
  imports: [
    DataTableComponent,
    ConfirmModalComponent,
    InstitutionFilterComponent,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  canCreate = this.authService.hasPermission('professionals:create');

  selectedInstitutionId: number | undefined;

  professionals: ProfessionalListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;

  showConfirmModal = false;
  itemToDeactivate: ProfessionalListItemResponse | null = null;

  public cols: TableColumn[] = [
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver detalle' },
        { action: 'persons', label: 'Personas a cargo' },
        { action: 'institutions', label: 'Instituciones' },
        { action: 'edit', label: 'Editar', visible: (item) => item.isActive },
        { action: 'deactivate', label: 'Desactivar', visible: (item) => item.isActive },
      ],
    },
    { key: 'fullName', label: 'Nombre' },
    { key: 'specialty', label: 'Especialidad' },
    { key: 'licenseNumber', label: 'Matrícula' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
  ];

  onInstitutionFilterChange(institutionId: number | undefined): void {
    this.selectedInstitutionId = institutionId;
    this.currentPage = 1;
    this.loadProfessionals();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadProfessionals();
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    this.loadProfessionals(term);
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate(['/admin/professionals/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'view':
        this.router.navigate(['/admin/professionals', event.item.id]);
        break;
      case 'persons':
        this.router.navigate(['/admin/professionals', event.item.id], { queryParams: { tab: 'personas' } });
        break;
      case 'institutions':
        this.router.navigate(['/admin/professionals', event.item.id], { queryParams: { tab: 'instituciones' } });
        break;
      case 'edit':
        this.router.navigate(['/admin/professionals', event.item.id, 'edit']);
        break;
      case 'deactivate':
        this.itemToDeactivate = event.item;
        this.showConfirmModal = true;
        break;
    }
  }

  confirmDeactivate(): void {
    if (!this.itemToDeactivate) return;

    this.professionalsService.deactivateProfessional(this.itemToDeactivate.id).subscribe({
      next: () => {
        this.toastService.success('Profesional desactivado exitosamente');
        this.showConfirmModal = false;
        this.itemToDeactivate = null;
        this.loadProfessionals();
      },
      error: () => {
        this.toastService.error('Error al desactivar el profesional');
        this.showConfirmModal = false;
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
    this.itemToDeactivate = null;
  }

  loadProfessionals(search?: string): void {
    this.professionalsService
      .getProfessionals({
        page: this.currentPage,
        pageSize: this.pageSize,
        search,
        institutionId: this.selectedInstitutionId,
      })
      .subscribe({
        next: (response) => {
          this.professionals = response.data;
          this.totalItems = response.totalRecords;
        },
        error: () => {
          this.toastService.error('Error al obtener profesionales');
        },
      });
  }
}
