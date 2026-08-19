import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  SpinnerComponent,
  AlertComponent,
  FormControlDirective,
  FormLabelDirective,
  ButtonDirective,
} from '@coreui/angular';
import { AuthService, ToastService } from '@services';
import { AdminUsersService } from '@services/admin-users.service';
import { AnalyticsService } from '@services/analytics.service';
import {
  AdminDashboardResponse,
  AnalyticsDashboardResponse,
  AdminReportsAnalyticsResponse,
} from '@models';
import { StatCardComponent } from '@shared/components/stat-card/stat-card.component';
import {
  HighContrastPieChartComponent,
  LevelHistogramChartComponent,
  ProfessionalProductivityChartComponent,
  ReportStatusPieChartComponent,
} from '@shared/components';
import { IconDirective } from '@coreui/icons-angular';
import { exportHtmlElementToPdf } from '@shared/utils';
import { forkJoin } from 'rxjs';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    SpinnerComponent,
    AlertComponent,
    FormControlDirective,
    FormLabelDirective,
    ButtonDirective,
    StatCardComponent,
    HighContrastPieChartComponent,
    LevelHistogramChartComponent,
    ProfessionalProductivityChartComponent,
    ReportStatusPieChartComponent,
    IconDirective,
  ],
})
export class DashboardComponent implements OnInit {
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly analyticsService = inject(AnalyticsService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  dashboard: AdminDashboardResponse | null = null;
  analytics: AnalyticsDashboardResponse | null = null;
  reportsAnalytics: AdminReportsAnalyticsResponse | null = null;

  dateFrom = '';
  dateTo = '';

  loading = true;
  error = false;
  isGlobalAdmin = false;
  isExportingPdf = false;

  ngOnInit(): void {
    this.isGlobalAdmin = this.authService.isGlobalAdmin();
    this.loadAllData();
  }

  loadAllData(): void {
    this.loading = true;
    forkJoin({
      dashboard: this.adminUsersService.getDashboard(),
      analytics: this.analyticsService.getAdminAnalytics(this.dateFrom || null, this.dateTo || null),
      reportsAnalytics: this.analyticsService.getAdminReportsAnalytics(this.dateFrom || null, this.dateTo || null),
    }).subscribe({
      next: ({ dashboard, analytics, reportsAnalytics }) => {
        this.dashboard = dashboard;
        this.analytics = analytics;
        this.reportsAnalytics = reportsAnalytics;
        this.loading = false;
      },
      error: () => {
        this.error = true;
        this.loading = false;
      },
    });
  }

  onDateFilterChange(): void {
    this.loadAllData();
  }

  clearDateFilters(): void {
    this.dateFrom = '';
    this.dateTo = '';
    this.loadAllData();
  }

  /**
   * Navega a la vista de reportes filtrando directamente por los que están pendientes de revisión (Submitted)
   */
  navigateToPendingReports(): void {
    this.router.navigate(['/admin/reports'], {
      queryParams: { status: 'Submitted' },
    });
  }

  async exportDashboardPdf(): Promise<void> {
    const container = document.getElementById('adminDashboardContainer');
    if (!container) {
      this.toastService.error('No se encontró el contenedor del dashboard para exportar.');
      return;
    }

    try {
      this.isExportingPdf = true;
      this.toastService.info('Generando PDF del panel de administración...');

      const fileName = `Dashboard_Admin_${new Date().toISOString().slice(0, 10)}.pdf`;

      // Esperar brevemente a que los gráficos svg/canvas terminen de asentarse
      await new Promise(resolve => setTimeout(resolve, 200));

      await exportHtmlElementToPdf(container, {
        filename: fileName,
        orientation: 'landscape',
        format: 'a4',
        margin: 10,
        scale: 2,
        fitToSinglePage: true,
      });

      this.toastService.success('PDF del dashboard exportado exitosamente.');
    } catch (error) {
      console.error('Error al exportar PDF:', error);
      this.toastService.error('Error al generar el PDF del dashboard.');
    } finally {
      this.isExportingPdf = false;
    }
  }
}
