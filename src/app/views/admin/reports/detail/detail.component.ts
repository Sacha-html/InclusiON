import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ReportsService, ToastService } from '@services';
import { ReportResponse } from '@models/responses/reports/report.response';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
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

  report = signal<ReportResponse | null>(null);
  isLoading = signal(true);

  showApproveModal = false;
  showRejectModal = false;
  isActioning = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadReport(+id);
  }

  loadReport(id: number): void {
    this.reportsService.getById(id).subscribe({
      next: (data) => { this.report.set(data); this.isLoading.set(false); },
      error: () => { this.isLoading.set(false); this.router.navigate(['/admin/reports']); },
    });
  }

  onBack(): void {
    this.router.navigate(['/admin/reports']);
  }

  confirmApprove(): void {
    const r = this.report();
    if (!r) return;
    this.isActioning = true;
    this.reportsService.approveReport(r.id).subscribe({
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
    this.reportsService.rejectReport(r.id, comment).subscribe({
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

  getStatusColor(status: string): string {
    return { Draft: 'secondary', Submitted: 'warning', Approved: 'success', Rejected: 'danger' }[status] ?? 'secondary';
  }

  getStatusLabel(status: string): string {
    return { Draft: 'Borrador', Submitted: 'Pendiente', Approved: 'Aprobado', Rejected: 'Rechazado' }[status] ?? status;
  }
}
