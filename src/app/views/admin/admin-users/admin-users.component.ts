import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AdminUsersService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { AuthService } from '../../../services/auth.service';
import { AdminUserResponse } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [DataTableComponent],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
})
export class AdminUsersComponent implements OnInit {
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  admins: AdminUserResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  isLoading = false;

  get currentUserId(): string {
    return this.authService.getCurrentUser()?.id ?? '';
  }

  public cols: TableColumn[] = [
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'edit', label: 'Editar', icon: 'cil-notes', visible: (item: AdminUserResponse) => item.id === this.currentUserId },
      ],
    },
    { key: 'fullName',  label: 'Nombre',       sortable: true },
    { key: 'email',     label: 'Email',         sortable: true },
    { key: 'isActive',  label: 'Estado',        type: 'badge', sortable: true },
    { key: 'createdAt', label: 'Fecha',         type: 'date',  sortable: true },
  ];

  ngOnInit(): void {
    this.loadAdmins();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadAdmins();
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    this.loadAdmins(term);
  }

  onHeaderAction(action: string): void {
    if (action === 'new') this.router.navigate([AppRoutes.Admin.Admins + '/new']);
  }

  onRowAction(event: { action: string; item: AdminUserResponse }): void {
    if (event.action === 'edit') this.router.navigate([AppRoutes.Admin.Admins + '/edit']);
  }

  loadAdmins(search?: string): void {
    this.isLoading = true;
    this.adminUsersService.getAdmins(this.currentPage, this.pageSize, search).subscribe({
      next: (response) => {
        this.admins     = response.data;
        this.totalItems = response.totalRecords;
        this.isLoading  = false;
      },
      error: () => { this.isLoading = false; },
    });
  }
}
