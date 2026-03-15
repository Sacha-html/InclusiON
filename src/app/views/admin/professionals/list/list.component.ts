import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { ProfessionalListItemResponse } from '../../../../models';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';

@Component({
  selector: 'app-list',
  imports: [DataTableComponent],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly router = inject(Router);

  professionals: ProfessionalListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;

  public cols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre' },
    { key: 'specialty', label: 'Especialidad' },
    { key: 'licenseNumber', label: 'Matrícula' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
    { key: 'actions', label: 'Acciones', type: 'actions' },
  ];

  ngOnInit(): void {
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

  onRowAction(event: { action: string; item: any }): void {
    if (event.action === 'view') {
      this.router.navigate(['/admin/professionals', event.item.id]);
    }
  }

  private loadProfessionals(search?: string): void {
    this.professionalsService
      .getProfessionals({ page: this.currentPage, pageSize: this.pageSize, search })
      .subscribe({
        next: (response) => {
          this.professionals = response.data;
          this.totalItems = response.totalRecords;
        },
        error: (error) => {
          console.error('Error al obtener profesionales:', error);
        },
      });
  }
}
