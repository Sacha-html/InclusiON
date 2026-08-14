import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ReportListItemResponse, ReportStatus } from '@models';
import {
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';

@Component({
  selector: 'app-professional-reports',
  standalone: true,
  imports: [
    FormsModule,
    FormSelectDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    DataTableComponent,
  ],
  templateUrl: './professional-reports.component.html',
})
export class ProfessionalReportsComponent implements OnInit {
  @Input({ required: true }) professionalId!: string;

  private readonly reportsService = inject(ReportsService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  reports = signal<ReportListItemResponse[]>([]);
  isLoading = signal(true);
  isProcessing = signal(false);
  currentPage = signal(1);
  totalPages = signal(1);
  totalRecords = signal(0);

  statusFilter = '';
  rejectComment = '';

  showApproveModal = signal(false);
  showRejectModal = signal(false);
  selectedReport = signal<ReportListItemResponse | null>(null);

  readonly columns: TableColumn[] = [
    { key: 'reportDateFmt',  label: 'Fecha',   sortable: false },
    { key: 'title',          label: 'Título',  sortable: false },
    { key: 'personName',     label: 'Persona', sortable: false },
    { key: 'reportTypeName', label: 'Tipo',    sortable: false },
    {
      key: 'status', label: 'Estado', type: 'badge',
      badgeMap: {
        [ReportStatus.Draft]:     { color: 'secondary', label: 'Borrador'  },
        [ReportStatus.Submitted]: { color: 'warning',   label: 'Pendiente' },
        [ReportStatus.Approved]:  { color: 'success',   label: 'Aprobado'  },
        [ReportStatus.Rejected]:  { color: 'danger',    label: 'Rechazado' },
      },
    },
    {
      key: '', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view',    label: 'Ver',      color: 'primary' },
        { action: 'approve', label: 'Aprobar',  color: 'success', visible: (item: any) => item.status === 'Submitted' },
        { action: 'reject',  label: 'Rechazar', color: 'danger',  visible: (item: any) => item.status === 'Submitted' },
      ],
    },
  ];

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.isLoading.set(true);
    this.reportsService.getReports({
      professionalId: this.professionalId,
      status: this.statusFilter || undefined,
      page: this.currentPage(),
      pageSize: 10,
      sortBy: 'reportDate',
      sortDirection: 'desc',
    }).subscribe({
      next: (response) => {
        this.reports.set(response.data.map(r => ({
          ...r,
          reportDateFmt: r.reportDate
            ? new Date(r.reportDate).toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })
            : '—',
        })));
        this.totalPages.set(response.totalPages);
        this.totalRecords.set(response.totalRecords);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar los reportes.');
        this.isLoading.set(false);
      },
    });
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'view':    this.viewReport(event.item.encryptedId); break;
      case 'approve': this.openApproveModal(event.item); break;
      case 'reject':  this.openRejectModal(event.item); break;
    }
  }

  viewReport(encryptedId: string): void {
    this.router.navigate([AppRoutes.Admin.Reports, encryptedId]);
  }

  openApproveModal(report: ReportListItemResponse): void {
    this.selectedReport.set(report);
    this.showApproveModal.set(true);
  }

  confirmApprove(): void {
    if (!this.selectedReport()) return;
    this.isProcessing.set(true);
    this.reportsService.approveReport(this.selectedReport()!.encryptedId).subscribe({
      next: () => {
        this.toastService.success('Reporte aprobado exitosamente.');
        this.showApproveModal.set(false);
        this.selectedReport.set(null);
        this.isProcessing.set(false);
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al aprobar el reporte.');
        this.isProcessing.set(false);
      },
    });
  }

  openRejectModal(report: ReportListItemResponse): void {
    this.selectedReport.set(report);
    this.rejectComment = '';
    this.showRejectModal.set(true);
  }

  confirmReject(): void {
    if (!this.selectedReport() || !this.rejectComment.trim()) return;
    this.isProcessing.set(true);
    this.reportsService.rejectReport(this.selectedReport()!.encryptedId, this.rejectComment.trim()).subscribe({
      next: () => {
        this.toastService.success('Reporte rechazado.');
        this.closeRejectModal();
        this.isProcessing.set(false);
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al rechazar el reporte.');
        this.isProcessing.set(false);
      },
    });
  }

  closeRejectModal(): void {
    this.showRejectModal.set(false);
    this.selectedReport.set(null);
    this.rejectComment = '';
  }
}
