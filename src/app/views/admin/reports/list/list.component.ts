import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportsService, ToastService } from '@services';
import { ReportListItemResponse } from '@models/responses/reports/report.response';
import { GetReportsRequest } from '@models/requests/reports/get-reports.request';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import {
  ButtonDirective,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  GridModule,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-reports-list',
  standalone: true,
  imports: [
    FormsModule,
    DataTableComponent,
    ConfirmModalComponent,
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
  private readonly router = inject(Router);

  reports = signal<ReportListItemResponse[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalRecords = signal(0);

  // Filtros
  statusFilter = 'Submitted'; // Por defecto: pendientes de revisión
  typeFilter = '';
  dateFrom = '';
  dateTo = '';

  // Modales
  showApproveModal = false;
  showRejectModal = false;
  selectedReport: ReportListItemResponse | null = null;
  isActioning = false;

  readonly reportTypes = [
    { id: 1, name: 'Evaluación Mensual' },
    { id: 2, name: 'Informe de Progreso' },
    { id: 3, name: 'Evaluación Trimestral' },
    { id: 4, name: 'Informe Anual' },
  ];

  columns: TableColumn[] = [
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'view', label: 'Ver detalle', icon: 'cil-search' },
        {
          action: 'approve',
          label: 'Aprobar',
          icon: 'cil-check-circle',
          visible: (item: ReportListItemResponse) => item.status === 'Submitted',
        },
        {
          action: 'reject',
          label: 'Rechazar',
          icon: 'cil-x-circle',
          visible: (item: ReportListItemResponse) => item.status === 'Submitted',
        },
      ],
    },
    { key: 'reportDate', label: 'Fecha', type: 'date', sortable: true },
    { key: 'title', label: 'Título', sortable: true },
    { key: 'personName', label: 'Persona', sortable: true },
    { key: 'professionalName', label: 'Profesional', sortable: true },
    { key: 'reportTypeName', label: 'Tipo', sortable: true },
    {
      key: 'status',
      label: 'Estado',
      type: 'badge',
      badgeMap: {
        Draft:     { color: 'secondary', label: 'Borrador' },
        Submitted: { color: 'warning',   label: 'Pendiente' },
        Approved:  { color: 'success',   label: 'Aprobado' },
        Rejected:  { color: 'danger',    label: 'Rechazado' },
      },
    },
  ];

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.isLoading.set(true);
    const request: GetReportsRequest = {
      page: this.currentPage(),
      pageSize: this.pageSize(),
      sortBy: 'reportDate',
      sortDirection: 'desc',
      status: this.statusFilter || undefined,
      reportTypeId: this.typeFilter ? +this.typeFilter : undefined,
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined,
    };

    this.reportsService.getReports(request).subscribe({
      next: (response) => {
        this.reports.set(response.data);
        this.totalRecords.set(response.totalRecords);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onStatusFilterChange(): void { this.currentPage.set(1); this.loadReports(); }
  onTypeFilterChange(): void   { this.currentPage.set(1); this.loadReports(); }
  onDateChange(): void         { this.currentPage.set(1); this.loadReports(); }

  clearFilters(): void {
    this.statusFilter = 'Submitted';
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

  onSearch(term: string): void {
    this.currentPage.set(1);
    this.loadReports();
  }

  onSort(_event: { sortBy: string; sortDirection: string }): void {
    this.loadReports();
  }

  onRowAction(event: { action: string; item: ReportListItemResponse }): void {
    this.selectedReport = event.item;
    if (event.action === 'view') {
      this.router.navigate(['/admin/reports', event.item.id]);
    } else if (event.action === 'approve') {
      this.showApproveModal = true;
    } else if (event.action === 'reject') {
      this.showRejectModal = true;
    }
  }

  confirmApprove(): void {
    if (!this.selectedReport) return;
    this.isActioning = true;
    this.reportsService.approveReport(this.selectedReport.id).subscribe({
      next: () => {
        this.toastService.success('Reporte aprobado. El familiar ya puede consultarlo.');
        this.showApproveModal = false;
        this.selectedReport = null;
        this.isActioning = false;
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al aprobar el reporte.');
        this.isActioning = false;
      },
    });
  }

  confirmReject(comment: string): void {
    if (!this.selectedReport) return;
    this.isActioning = true;
    this.reportsService.rejectReport(this.selectedReport.id, comment).subscribe({
      next: () => {
        this.toastService.success('Reporte rechazado. El profesional fue notificado.');
        this.showRejectModal = false;
        this.selectedReport = null;
        this.isActioning = false;
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al rechazar el reporte.');
        this.isActioning = false;
      },
    });
  }

  cancelAction(): void {
    this.showApproveModal = false;
    this.showRejectModal = false;
    this.selectedReport = null;
  }
}
