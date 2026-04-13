import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AssignmentsService, ProfessionalsService, ReportsService } from '@services';
import { ReportListItemResponse } from '@models/responses/reports/report.response';
import { ProfessionalPersonResponse } from '@models';
import { GetReportsRequest } from '@models/requests/reports/get-reports.request';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import {
  ButtonDirective,
  FormLabelDirective,
  FormSelectDirective,
  GridModule,
} from '@coreui/angular';

@Component({
  selector: 'app-reports-list',
  imports: [
    RouterModule,
    FormsModule,
    DataTableComponent,
    ButtonDirective,
    FormLabelDirective,
    FormSelectDirective,
    GridModule,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly reportsService = inject(ReportsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly router = inject(Router);

  reports = signal<ReportListItemResponse[]>([]);
  persons = signal<ProfessionalPersonResponse[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalRecords = signal(0);
  totalPages = signal(0);
  searchTerm = signal('');

  selectedPersonId = '';
  statusFilter = '';

  columns: TableColumn[] = [
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'view', label: 'Ver detalle', icon: 'cil-search' },
      ],
    },
    { key: 'reportDate', label: 'Fecha', type: 'date', sortable: true },
    { key: 'title', label: 'Título', sortable: true },
    { key: 'personName', label: 'Persona', sortable: true },
    { key: 'reportTypeName', label: 'Tipo', sortable: true },
    {
      key: 'isActive',
      label: 'Estado',
      type: 'badge',
      sortable: true,
      badgeMap: {
        true: { color: 'success', label: 'Activo' },
        false: { color: 'secondary', label: 'Inactivo' },
      }
    },
  ];

  headerButtons = [
    { action: 'create', label: 'Agregar', icon: 'cilPlus', routerLink: '/pro/reports/new' },
  ];

  ngOnInit(): void {
    this.loadReports();
    this.loadPersons();
  }

  private loadPersons(): void {
    this.professionalsService.getMyProfile().subscribe({
      next: (profile) => {
        this.assignmentsService.getPersonsByProfessional(profile.id).subscribe({
          next: (persons) => this.persons.set(persons.filter(p => p.isActive)),
        });
      },
    });
  }

  loadReports(): void {
    this.isLoading.set(true);

    const isActive = this.statusFilter === 'true' ? true
                   : this.statusFilter === 'false' ? false
                   : undefined;

    const request: GetReportsRequest = {
      page: this.currentPage(),
      pageSize: this.pageSize(),
      search: this.searchTerm() || undefined,
      sortBy: 'reportDate',
      sortDirection: 'desc',
      personId: this.selectedPersonId || undefined,
      isActive,
    };

    this.reportsService.getReports(request).subscribe({
      next: (response) => {
        this.reports.set(response.data);
        this.totalRecords.set(response.totalRecords);
        this.totalPages.set(response.totalPages);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onPersonFilterChange(): void {
    this.currentPage.set(1);
    this.loadReports();
  }

  onStatusFilterChange(): void {
    this.currentPage.set(1);
    this.loadReports();
  }

  clearFilters(): void {
    this.selectedPersonId = '';
    this.statusFilter = '';
    this.currentPage.set(1);
    this.loadReports();
  }

  onSearch(term: string): void {
    this.searchTerm.set(term);
    this.currentPage.set(1);
    this.loadReports();
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  onSort(event: { sortBy: string; sortDirection: string }): void {
    this.loadReports();
  }

  onRowAction(event: { action: string; item: ReportListItemResponse }): void {
    if (event.action === 'view') {
      this.router.navigate(['/pro/reports', event.item.id]);
    }
  }

  onHeaderAction(action: string): void {
    if (action === 'create') {
      this.router.navigate(['/pro/reports/new']);
    }
  }
}
