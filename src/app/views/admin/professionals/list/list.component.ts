import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService, ProfessionalsService } from '@services';
import { ProfessionalListItemResponse } from '../../../../models';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';
import { ButtonDirective } from '@coreui/angular';

@Component({
  selector: 'app-list',
  imports: [DataTableComponent, ButtonDirective, RouterLink],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  canCreate = this.authService.hasPermission('professionals:create');

  professionals: ProfessionalListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;

  public cols: TableColumn[] = [
    { key: 'actions', label: 'Acciones', type: 'actions' },
    { key: 'fullName', label: 'Nombre' },
    { key: 'specialty', label: 'Especialidad' },
    { key: 'licenseNumber', label: 'Matrícula' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
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
