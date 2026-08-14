import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import {
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';
import { AuthService } from '@services';
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
import { forkJoin } from 'rxjs';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    SpinnerComponent,
    AlertComponent,
    StatCardComponent,
    HighContrastPieChartComponent,
    LevelHistogramChartComponent,
    ProfessionalProductivityChartComponent,
    ReportStatusPieChartComponent,
  ],
})
export class DashboardComponent implements OnInit {
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly analyticsService = inject(AnalyticsService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  dashboard: AdminDashboardResponse | null = null;
  analytics: AnalyticsDashboardResponse | null = null;
  reportsAnalytics: AdminReportsAnalyticsResponse | null = null;

  loading = true;
  error = false;
  isGlobalAdmin = false;

  ngOnInit(): void {
    this.isGlobalAdmin = this.authService.isGlobalAdmin();
    this.loadAllData();
  }

  loadAllData(): void {
    this.loading = true;
    forkJoin({
      dashboard: this.adminUsersService.getDashboard(),
      analytics: this.analyticsService.getAdminAnalytics(),
      reportsAnalytics: this.analyticsService.getAdminReportsAnalytics(),
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

  /**
   * Navega a la vista de reportes filtrando directamente por los que están pendientes de revisión (Submitted)
   */
  navigateToPendingReports(): void {
    this.router.navigate(['/admin/reports'], {
      queryParams: { status: 'Submitted' },
    });
  }
}
