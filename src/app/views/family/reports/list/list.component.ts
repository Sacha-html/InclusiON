import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogsService, ReportsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { CatalogItem, ReportListItemResponse, GetReportsRequest } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import {
  ButtonDirective,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  GridModule,
} from '@coreui/angular';

@Component({
  selector: 'app-family-reports-list',
  standalone: true,
  imports: [
    FormsModule,
    DataTableComponent,
    ButtonDirective,
    FormControlDirective,
    FormLabelDirective,
    FormSelectDirective,
    GridModule,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly reportsService = inject(ReportsService);
  private readonly toastService = inject(ToastService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly router = inject(Router);

  reports = signal<ReportListItemResponse[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalRecords = signal(0);

  sortBy = 'createdAt';
  sortDirection = 'DESC';

  // Filtros
  typeFilter = '';
  dateFrom = '';
  dateTo = '';

  reportTypes = signal<CatalogItem[]>([]);

  columns: TableColumn[] = [
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'view', label: 'Ver reporte', icon: 'cilSearch' },
      ],
    },
    { key: 'reportDate', label: 'Fecha', type: 'date', sortable: true },
    { key: 'title', label: 'Título', sortable: true },
    { key: 'reportTypeName', label: 'Tipo', sortable: true },
    { key: 'professionalName', label: 'Profesional', sortable: true },
    { key: 'personName', label: 'Persona', sortable: true },
  ];

  ngOnInit(): void {
    this.catalogsService.getReportTypes().subscribe(types => this.reportTypes.set(types));
    this.loadReports();
  }

  loadReports(): void {
    this.isLoading.set(true);
    const request: GetReportsRequest = {
      page: this.currentPage(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy,
      sortDirection: this.sortDirection,
      reportTypeId: this.typeFilter ? +this.typeFilter : undefined,
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined,
    };

    this.reportsService.getFamilyReports(request).subscribe({
      next: (response) => {
        this.reports.set(response.data);
        this.totalRecords.set(response.totalRecords);
        this.isLoading.set(false);
      },
      error: () => { this.isLoading.set(false); this.toastService.error('Error al cargar los informes'); },
    });
  }

  onTypeFilterChange(): void { this.currentPage.set(1); this.loadReports(); }
  onDateChange(): void       { this.currentPage.set(1); this.loadReports(); }

  clearFilters(): void {
    this.typeFilter = '';
    this.dateFrom = '';
    this.dateTo = '';
    this.currentPage.set(1);
    this.loadReports();
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  onSort(event: { sortBy: string; sortDirection: string }): void {
    this.sortBy = event.sortBy;
    this.sortDirection = event.sortDirection;
    this.loadReports();
  }

  onRowAction(event: { action: string; item: ReportListItemResponse }): void {
    if (event.action === 'view') {
      this.router.navigate([AppRoutes.Family.Reports, event.item.encryptedId]);
    }
  }
}
