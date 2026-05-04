import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ReportListItemResponse, ReportStatus } from '@models/responses/reports/report.response';
import {
  BadgeComponent,
  ButtonDirective,
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
  TableDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-person-reports',
  standalone: true,
  imports: [
    FormsModule,
    BadgeComponent,
    ButtonDirective,
    FormSelectDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    TableDirective,
  ],
  templateUrl: './admin-person-reports.component.html',
})
export class AdminPersonReportsComponent implements OnInit {
  @Input({ required: true }) personId!: string;

  private readonly reportsService = inject(ReportsService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  readonly ReportStatus = ReportStatus;

  reports = signal<ReportListItemResponse[]>([]);
  isLoading = signal(true);
  isProcessing = signal(false);
  currentPage = signal(1);
  totalPages = signal(1);

  statusFilter = '';
  rejectComment = '';

  showApproveModal = signal(false);
  showRejectModal = signal(false);
  selectedReport = signal<ReportListItemResponse | null>(null);

  readonly badgeMap: Record<ReportStatus, { color: string; label: string }> = {
    [ReportStatus.Draft]:     { color: 'secondary', label: 'Borrador' },
    [ReportStatus.Submitted]: { color: 'warning',   label: 'Enviado' },
    [ReportStatus.Approved]:  { color: 'success',   label: 'Aprobado' },
    [ReportStatus.Rejected]:  { color: 'danger',    label: 'Rechazado' },
  };

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.isLoading.set(true);
    this.reportsService.getReports({
      personId: this.personId,
      status: this.statusFilter || undefined,
      page: this.currentPage(),
      pageSize: 10,
      sortBy: 'reportDate',
      sortDirection: 'desc',
    }).subscribe({
      next: (response) => {
        this.reports.set(response.data);
        this.totalPages.set(response.totalPages);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar los reportes.');
        this.isLoading.set(false);
      },
    });
  }

  changePage(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  viewReport(id: number): void {
    this.router.navigate([AppRoutes.Admin.Reports, id]);
  }

  openApproveModal(report: ReportListItemResponse): void {
    this.selectedReport.set(report);
    this.showApproveModal.set(true);
  }

  confirmApprove(): void {
    if (!this.selectedReport()) return;
    this.isProcessing.set(true);
    this.reportsService.approveReport(this.selectedReport()!.id).subscribe({
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
    this.reportsService.rejectReport(this.selectedReport()!.id, this.rejectComment.trim()).subscribe({
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

  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }
}
