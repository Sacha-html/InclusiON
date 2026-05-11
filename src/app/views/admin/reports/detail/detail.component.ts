import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ReportsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ReportResponse, ReportStatus } from '@models/responses/reports/report.response';
import { ReportStatus as ReportStatusLabels } from '@shared/constants/status-labels';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { IconDirective } from '@coreui/icons-angular';
import {
  AlertComponent,
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-report-detail',
  standalone: true,
  imports: [
    DatePipe,
    IconDirective,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    BadgeComponent,
    ButtonDirective,
    SpinnerComponent,
    ColComponent,
    RowComponent,
    AlertComponent,
    ConfirmModalComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly reportsService = inject(ReportsService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly ReportStatus = ReportStatus;
  report = signal<ReportResponse | null>(null);
  isLoading = signal(true);

  showApproveModal = false;
  showRejectModal = false;
  isActioning = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadReport(id);
  }

  loadReport(id: string): void {
    this.reportsService.getById(id).subscribe({
      next: (data) => { this.report.set(data); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); this.router.navigate([AppRoutes.Admin.Reports]); },
    });
  }

  onBack(): void {
    this.router.navigate([AppRoutes.Admin.Reports]);
  }

  confirmApprove(): void {
    const r = this.report();
    if (!r) return;
    this.isActioning = true;
    this.reportsService.approveReport(r.encryptedId).subscribe({
      next: (updated) => {
        this.report.set(updated);
        this.toastService.success('Reporte aprobado. El familiar ya puede consultarlo.');
        this.showApproveModal = false;
        this.isActioning = false;
      },
      error: () => { this.toastService.error('Error al aprobar.'); this.isActioning = false; },
    });
  }

  confirmReject(comment: string): void {
    const r = this.report();
    if (!r) return;
    this.isActioning = true;
    this.reportsService.rejectReport(r.encryptedId, comment).subscribe({
      next: (updated) => {
        this.report.set(updated);
        this.toastService.success('Reporte rechazado. El profesional fue notificado.');
        this.showRejectModal = false;
        this.isActioning = false;
      },
      error: () => { this.toastService.error('Error al rechazar.'); this.isActioning = false; },
    });
  }

  cancelAction(): void {
    this.showApproveModal = false;
    this.showRejectModal = false;
  }

  readonly statusMap: Partial<Record<string, { color: string; label: string }>> = {
    [ReportStatus.Draft]:     { color: 'secondary', label: ReportStatusLabels.Borrador  },
    [ReportStatus.Submitted]: { color: 'warning',   label: ReportStatusLabels.Enviado   },
    [ReportStatus.Approved]:  { color: 'success',   label: ReportStatusLabels.Aprobado  },
    [ReportStatus.Rejected]:  { color: 'danger',    label: ReportStatusLabels.Rechazado },
  };
}
