import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  CardBodyComponent,
  CardComponent,
  ColComponent,
  RowComponent,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';
import { AuthService } from '@services';
import { AdminUsersService } from '../../../services/admin-users.service';
import { AdminDashboardResponse } from '../../../models';
import { StatCardComponent } from '@shared/components/stat-card/stat-card.component';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  imports: [
    CommonModule,
    RouterLink,
    CardComponent,
    CardBodyComponent,
    RowComponent,
    ColComponent,
    SpinnerComponent,
    AlertComponent,
    StatCardComponent,
  ],
})
export class DashboardComponent implements OnInit {
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly authService = inject(AuthService);

  dashboard: AdminDashboardResponse | null = null;
  loading = true;
  error = false;
  isGlobalAdmin = false;

  ngOnInit(): void {
    this.isGlobalAdmin = this.authService.isGlobalAdmin();
    this.adminUsersService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
        this.loading = false;
      },
      error: () => {
        this.error = true;
        this.loading = false;
      },
    });
  }
}
