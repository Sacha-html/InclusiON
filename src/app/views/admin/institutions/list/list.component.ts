import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { InstitutionsService, ToastService } from '@services';
import { InstitutionResponse } from '../../../../models';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';

@Component({
  selector: 'app-list',
  imports: [DataTableComponent],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly institutionsService = inject(InstitutionsService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  institutions: InstitutionResponse[] = [];
  filteredInstitutions: InstitutionResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;

  public cols: TableColumn[] = [
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'edit', label: 'Editar', icon: 'cil-notes' },
      ],
    },
    { key: 'name', label: 'Nombre' },
    { key: 'address', label: 'Direccion' },
    { key: 'phone', label: 'Telefono' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
  ];

  ngOnInit(): void {
    this.loadInstitutions();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyFilter();
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    this.applyFilter(term);
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate(['/admin/institutions/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
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
    let filtered = this.institutions;
    if (search) {
      const term = search.toLowerCase();
      filtered = this.institutions.filter(
        (i) =>
          i.name.toLowerCase().includes(term) ||
          (i.address && i.address.toLowerCase().includes(term)),
      );
    }
    this.totalItems = filtered.length;
    const start = (this.currentPage - 1) * this.pageSize;
    this.filteredInstitutions = filtered.slice(start, start + this.pageSize);
  }
}
