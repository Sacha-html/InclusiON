import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, FamilyService, ToastService } from '@services';
import { FamilyListItemResponse } from '../../../../models';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { InstitutionFilterComponent } from '@shared/components/institution-filter/institution-filter.component';

@Component({
  selector: 'app-family-list',
  imports: [
    DataTableComponent,
    ConfirmModalComponent,
    InstitutionFilterComponent,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent {
  private readonly familyService = inject(FamilyService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  canCreate = this.authService.hasPermission('family:create');

  selectedInstitutionId: number | undefined;

  families: FamilyListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;

  showConfirmModal = false;
  itemToDeactivate: FamilyListItemResponse | null = null;

  public cols: TableColumn[] = [
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver detalle' },
        { action: 'edit', label: 'Editar', visible: (item) => item.isActive },
        { action: 'deactivate', label: 'Desactivar', visible: (item) => item.isActive },
      ],
    },
    { key: 'fullName', label: 'Nombre' },
    { key: 'relationship', label: 'Parentesco' },
    { key: 'phone', label: 'Telefono' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
  ];

  onInstitutionFilterChange(institutionId: number | undefined): void {
    this.selectedInstitutionId = institutionId;
    this.currentPage = 1;
    this.loadFamily();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadFamily();
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    this.loadFamily(term);
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate(['/admin/family/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'view':
        this.router.navigate(['/admin/family', event.item.id]);
        break;
      case 'edit':
        this.router.navigate(['/admin/family', event.item.id, 'edit']);
        break;
      case 'deactivate':
        this.itemToDeactivate = event.item;
        this.showConfirmModal = true;
        break;
    }
  }

  confirmDeactivate(): void {
    if (!this.itemToDeactivate) return;

    this.familyService.deactivateFamily(this.itemToDeactivate.id).subscribe({
      next: () => {
        this.toastService.success('Familiar desactivado exitosamente');
        this.showConfirmModal = false;
        this.itemToDeactivate = null;
        this.loadFamily();
      },
      error: () => {
        this.toastService.error('Error al desactivar el familiar');
        this.showConfirmModal = false;
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
    this.itemToDeactivate = null;
  }

  loadFamily(search?: string): void {
    this.familyService
      .getFamily({ page: this.currentPage, pageSize: this.pageSize, search, institutionId: this.selectedInstitutionId })
      .subscribe({
        next: (response) => {
          this.families = response.data;
          this.totalItems = response.totalRecords;
        },
        error: () => {
          this.toastService.error('Error al obtener familiares');
        },
      });
  }
}
