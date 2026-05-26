import { Component, Input, OnInit, signal } from '@angular/core';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ReportsService } from '@services/reports.service';
import { ReportListItemResponse, ReportStatus } from '@models';
import { ReportStatus as StatusLabels } from '@shared/constants/status-labels';
import { AppRoutes } from '@shared/constants/app-routes';
import {
  BadgeComponent,
  ButtonDirective,
  SpinnerComponent,
} from '@coreui/angular';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-professional-reports-tab',
  standalone: true,
  imports: [
    BadgeComponent,
    ButtonDirective,
    SpinnerComponent,
    EmptyStateComponent,
  ],
  templateUrl: './professional-reports-tab.component.html',
})
export class ProfessionalReportsTabComponent implements OnInit {
  @Input({ required: true }) personId!: string;

  private readonly reportsService = inject(ReportsService);
  private readonly router         = inject(Router);

  reports   = signal<ReportListItemResponse[]>([]);
  isLoading = signal(true);
  hasError  = signal(false);

  totalRecords = signal(0);
  currentPage  = signal(1);
  readonly pageSize = 10;

  readonly ReportStatus = ReportStatus;

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.isLoading.set(true);
    this.reportsService.getReports({
      page: this.currentPage(),
      pageSize: this.pageSize,
      personId: this.personId,
      sortBy: 'reportDate',
      sortDirection: 'DESC',
    }).subscribe({
      next: (res) => {
        this.reports.set(res.data);
        this.totalRecords.set(res.totalRecords);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      },
    });
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  viewReport(report: ReportListItemResponse): void {
    this.router.navigate([AppRoutes.Pro.Reports, report.encryptedId]);
  }

  statusColor(status: ReportStatus): string {
    switch (status) {
      case ReportStatus.Draft:     return 'secondary';
      case ReportStatus.Submitted: return 'warning';
      case ReportStatus.Approved:  return 'success';
      case ReportStatus.Rejected:  return 'danger';
      default:                     return 'secondary';
    }
  }

  statusLabel(status: ReportStatus): string {
    switch (status) {
      case ReportStatus.Draft:     return StatusLabels.Borrador;
      case ReportStatus.Submitted: return StatusLabels.Enviado;
      case ReportStatus.Approved:  return StatusLabels.Aprobado;
      case ReportStatus.Rejected:  return StatusLabels.Rechazado;
      default:                     return String(status);
    }
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }

  get pages(): number[] {
    const total = Math.ceil(this.totalRecords() / this.pageSize);
    return Array.from({ length: total }, (_, i) => i + 1);
  }
}
