import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ReportsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ReportResponse, ReportStatus } from '@models/responses/reports/report.response';
import { ReportStatus as ReportStatusLabels } from '@shared/constants/status-labels';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  SpinnerComponent,
  ColComponent,
  RowComponent,
  AlertComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-report-detail',
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

  readonly ReportStatus = ReportStatus;
  report = signal<ReportResponse | null>(null);
  isLoading = signal(true);
  showSubmitModal     = false;
  isSubmitting        = false;
  showDeactivateModal = false;
  isDeactivating      = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadReport(+id);
    }
  }

  loadReport(id: number): void {
    this.reportsService.getById(id).subscribe({
      next: (data) => {
        this.report.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.router.navigate([AppRoutes.Pro.Reports]);
      },
    });
  }

  onBack(): void {
    this.router.navigate([AppRoutes.Pro.Reports]);
  }

  onSubmitClick(): void {
    this.showSubmitModal = true;
  }

  confirmSubmit(): void {
    const r = this.report();
    if (!r) return;
    this.isSubmitting = true;
    this.reportsService.submitReport(r.id).subscribe({
      next: (updated) => {
        this.report.set(updated);
        this.toastService.success('Reporte enviado al administrador para revisión.');
        this.showSubmitModal = false;
        this.isSubmitting = false;
      },
      error: () => {
        this.toastService.error('Error al enviar el reporte.');
        this.isSubmitting = false;
      },
    });
  }

  cancelSubmit(): void {
    this.showSubmitModal = false;
  }

  onEditClick(): void {
    const r = this.report();
    if (r) this.router.navigate([AppRoutes.Pro.Reports, r.id, 'edit']);
  }

  onDeactivateClick(): void {
    this.showDeactivateModal = true;
  }

  confirmDeactivate(): void {
    const r = this.report();
    if (!r) return;
    this.isDeactivating = true;
    this.reportsService.deactivate(r.id).subscribe({
      next: () => {
        this.toastService.success('Reporte dado de baja exitosamente.');
        this.router.navigate([AppRoutes.Pro.Reports]);
      },
      error: (err) => {
        const msg = err?.userMessage ?? 'Error al dar de baja el reporte.';
        this.toastService.error(msg);
        this.isDeactivating = false;
        this.showDeactivateModal = false;
      },
    });
  }

  cancelDeactivate(): void {
    this.showDeactivateModal = false;
  }

  readonly statusMap: Partial<Record<string, { color: string; label: string }>> = {
    [ReportStatus.Draft]:     { color: 'secondary', label: ReportStatusLabels.Borrador  },
    [ReportStatus.Submitted]: { color: 'warning',   label: ReportStatusLabels.Enviado   },
    [ReportStatus.Approved]:  { color: 'success',   label: ReportStatusLabels.Aprobado  },
    [ReportStatus.Rejected]:  { color: 'danger',    label: ReportStatusLabels.Rechazado },
  };
}
