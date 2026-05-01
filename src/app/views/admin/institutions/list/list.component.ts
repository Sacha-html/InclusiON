import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InstitutionsService, ToastService } from '@services';
import { InstitutionResponse } from '../../../../models';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';
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

  institutions: InstitutionResponse[] = [];
  filteredInstitutions: InstitutionResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  sortBy = 'name';
  sortDirection: 'ASC' | 'DESC' = 'ASC';
  loading = false;

  public cols: TableColumn[] = [
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'detail', label: 'Ver detalle', icon: 'cil-search' },
        { action: 'edit', label: 'Editar', icon: 'cil-notes' },
      ],
    },
    { key: 'name', label: 'Nombre', sortable: true },
    { key: 'address', label: 'Direccion', sortable: true },
    { key: 'phone', label: 'Telefono', sortable: true },
    { key: 'isActive', label: 'Estado', type: 'badge', sortable: true },
  ];

  ngOnInit(): void {
    this.loadInstitutions();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyFilter();
  }

  onSort(event: { sortBy: string; sortDirection: 'ASC' | 'DESC' }): void {
    this.sortBy = event.sortBy;
    this.sortDirection = event.sortDirection;
    this.currentPage = 1;
    this.applyFilter();
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    this.applyFilter(term);
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.applyFilter();
  }

  clearFilters(): void {
    this.statusFilter = '';
    this.currentPage = 1;
    this.applyFilter();
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate(['/admin/institutions/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'detail':
        this.router.navigate(['/admin/institutions', event.item.id, 'detail']);
        break;
      case 'edit':
        this.router.navigate(['/admin/institutions', event.item.id, 'edit']);
        break;
    }
  }

  private loadInstitutions(): void {
    this.institutionsService.getAll().subscribe({
      next: (data) => {
        this.institutions = data;
        this.applyFilter();
      },
      error: () => {
        this.toastService.error('Error al obtener instituciones');
      },
    });
  }

  private applyFilter(search?: string): void {
    let filtered = [...this.institutions];

    if (this.statusFilter) {
      const isActive = this.statusFilter === 'active';
      filtered = filtered.filter(i => i.isActive === isActive);
    }

    if (search) {
      const term = search.toLowerCase();
      filtered = filtered.filter(
        (i) =>
          i.name.toLowerCase().includes(term) ||
          (i.address && i.address.toLowerCase().includes(term)),
      );
    }

    filtered.sort((a, b) => {
      const aVal = (a[this.sortBy as keyof InstitutionResponse] ?? '').toString().toLowerCase();
      const bVal = (b[this.sortBy as keyof InstitutionResponse] ?? '').toString().toLowerCase();
      const direction = this.sortDirection === 'ASC' ? 1 : -1;
      return aVal.localeCompare(bVal) * direction;
    });

    this.totalItems = filtered.length;
    const start = (this.currentPage - 1) * this.pageSize;
    this.filteredInstitutions = filtered.slice(start, start + this.pageSize);
  }
}
