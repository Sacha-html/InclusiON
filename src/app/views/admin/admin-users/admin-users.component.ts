import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AdminUsersService } from '@services';
import { AuthService } from '../../../services/auth.service';
import { AdminUserResponse } from '@models';

import {
  BadgeComponent, ButtonDirective, CardBodyComponent, CardComponent,
  CardHeaderComponent, SpinnerComponent, TableDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    DatePipe,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    TableDirective, BadgeComponent, SpinnerComponent, ButtonDirective,
  ],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
})
export class AdminUsersComponent implements OnInit {
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  admins: AdminUserResponse[] = [];
  isLoading = true;

  get currentUserId(): string {
    return this.authService.getCurrentUser()?.id ?? '';
  }

  ngOnInit(): void {
    this.loadAdmins();
  }

  loadAdmins(): void {
    this.isLoading = true;
    this.adminUsersService.getAdmins().subscribe({
      next: (data) => { this.admins = data; this.isLoading = false; },
      error: () => this.isLoading = false,
    });
  }

  goToNew(): void {
    this.router.navigate(['/admin/admins/new']);
  }

  goToEdit(): void {
    this.router.navigate(['/admin/admins/edit']);
  }

  getTypeLabel(admin: AdminUserResponse): string {
    return admin.isGlobalAdmin ? 'Global' : 'Institucional';
  }

  getTypeColor(admin: AdminUserResponse): string {
    return admin.isGlobalAdmin ? 'primary' : 'info';
  }

  getStatusColor(isActive: boolean): string {
    return isActive ? 'success' : 'danger';
  }
}
