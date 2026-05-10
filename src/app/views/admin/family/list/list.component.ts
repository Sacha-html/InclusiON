import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonDirective, FormControlDirective, FormLabelDirective, FormSelectDirective, GridModule } from '@coreui/angular';
import { AuthService, FamilyService, ToastService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import { AppRoutes } from '@shared/constants/app-routes';
import { FamilyListItemResponse } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { InstitutionFilterComponent } from '@shared/components/institution-filter/institution-filter.component';

@Component({
  selector: 'app-family-list',
  imports: [
    FormsModule,
    GridModule,
    ButtonDirective,
    FormControlDirective,
    FormLabelDirective,
    FormSelectDirective,
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

  canCreate = this.authService.hasPermission(Permissions.Family.Create);

  selectedInstitutionId: number | undefined;

  linkedPersonSearch = '';
  statusFilter = '';

  families: FamilyListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  sortBy = 'LastName';
  sortDirection: 'ASC' | 'DESC' = 'ASC';
  loading = false;

  showConfirmModal = false;
  itemToDeactivate: FamilyListItemResponse | null = null;

  public cols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre', sortable: true },
    { key: 'linkedPersonNames', label: 'Familiar de' },
    { key: 'relationship', label: 'Parentesco' },
    { key: 'phone', label: 'Telefono' },
    { key: 'isActive', label: 'Estado', type: 'badge', badgeMap: { 'true': { color: 'success', label: 'Activo' }, 'false': { color: 'danger', label: 'Inactivo' } } },
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver', icon: 'cil-search' },
        { action: 'edit', label: 'Editar', icon: 'cil-notes', visible: (item) => item.isActive },
        { action: 'deactivate', label: 'Desactivar', icon: 'cil-x', visible: (item) => item.isActive },
      ],
    },
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

  onSort(event: { sortBy: string; sortDirection: 'ASC' | 'DESC' }): void {
    const sortMap: Record<string, string> = {
      'fullName': 'LastName',
    };
    this.sortBy = sortMap[event.sortBy] ?? event.sortBy;
    this.sortDirection = event.sortDirection;
    this.currentPage = 1;
    this.loadFamily();
  }

  onLinkedPersonSearch(): void {
    this.currentPage = 1;
    this.loadFamily();
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadFamily();
  }

  clearFilters(): void {
    this.linkedPersonSearch = '';
    this.statusFilter = '';
    this.currentPage = 1;
    this.loadFamily();
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    this.loadFamily(term);
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate([AppRoutes.Admin.Family + '/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'view':
        this.router.navigate([AppRoutes.Admin.Family, event.item.id]);
        break;
      case 'edit':
        this.router.navigate([AppRoutes.Admin.Family, event.item.id, 'edit']);
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
    this.loading = true;
    const isActive = this.statusFilter === 'true' ? true
                   : this.statusFilter === 'false' ? false
                   : undefined;

    this.familyService
      .getFamily({
        page: this.currentPage,
        pageSize: this.pageSize,
        search,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
        institutionId: this.selectedInstitutionId,
        linkedPersonSearch: this.linkedPersonSearch || undefined,
        isActive,
      })
      .subscribe({
        next: (response) => {
          this.families = response.data.map((f: any) => ({
            ...f,
            linkedPersonNames: f.linkedPersons?.map((p: any) => p.fullName).join(', ') || '—',
          }));
          this.totalItems = response.totalRecords;
          this.loading = false;
        },
        error: () => {
          this.toastService.error('Error al obtener familiares');
          this.loading = false;
        },
      });
  }
}
