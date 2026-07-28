import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InstitutionsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { InstitutionResponse } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ButtonDirective, FormLabelDirective, FormSelectDirective, GridModule } from '@coreui/angular';

@Component({
  selector: 'app-list',
  imports: [DataTableComponent, FormsModule, ButtonDirective, FormLabelDirective, FormSelectDirective, GridModule],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly institutionsService = inject(InstitutionsService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  statusFilter = '';
  searchTerm = '';

  institutions: InstitutionResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  loading = false;

  public cols: TableColumn[] = [
    { key: 'name', label: 'Nombre', sortable: true },
    { key: 'address', label: 'Direccion', sortable: true },
    { key: 'phone', label: 'Telefono', sortable: true },
    { key: 'isActive', label: 'Estado', type: 'badge', sortable: true, badgeMap: { 'true': { color: 'success', label: 'Activo' }, 'false': { color: 'danger', label: 'Inactivo' } } },
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'detail', label: 'Ver detalle', icon: 'cilSearch' },
        { action: 'edit', label: 'Editar', icon: 'cilNotes' },
      ],
    },
  ];

  ngOnInit(): void {
    this.loadInstitutions();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadInstitutions();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadInstitutions();
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadInstitutions();
  }

  clearFilters(): void {
    this.statusFilter = '';
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadInstitutions();
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate([AppRoutes.Admin.Institutions + '/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'detail':
        this.router.navigate([AppRoutes.Admin.Institutions, event.item.id, 'detail']);
        break;
      case 'edit':
        this.router.navigate([AppRoutes.Admin.Institutions, event.item.id, 'edit']);
        break;
    }
  }

  private loadInstitutions(): void {
    this.loading = true;
    let isActive: boolean | undefined;
    if (this.statusFilter === 'active') {
      isActive = true;
    } else if (this.statusFilter === 'inactive') {
      isActive = false;
    }

    this.institutionsService.getPaged({
      page: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchTerm || undefined,
      isActive,
    }).subscribe({
      next: (response) => {
        this.institutions = response.data;
        this.totalItems = response.totalRecords;
        this.loading = false;
      },
      error: () => {
        this.toastService.error('Error al obtener instituciones');
        this.loading = false;
      },
    });
  }
}
