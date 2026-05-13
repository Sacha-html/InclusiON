import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { UserManagementService } from '../../../../services/user-management.service';
import { AdminUserListItemResponse } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import {
  ColComponent,
  RowComponent,
  FormLabelDirective,
  FormSelectDirective,
  AlertComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  ModalTitleDirective,
  ButtonDirective,
  GridModule,
} from '@coreui/angular';

@Component({
  selector: 'app-user-management-list',
  imports: [
    FormsModule,
    DataTableComponent,
    ConfirmModalComponent,
    ColComponent,
    RowComponent,
    FormLabelDirective,
    FormSelectDirective,
    AlertComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ModalTitleDirective,
    ButtonDirective,
    GridModule,
  ],
  templateUrl: './list.component.html',
})
export class UserManagementListComponent implements OnInit {
  private readonly userService = inject(UserManagementService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  selectedRole = '';
  selectedStatus = '';
  searchTerm = '';

  users: AdminUserListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  sortBy = 'FirstName';
  sortDirection: 'ASC' | 'DESC' = 'ASC';
  loading = false;

  ngOnInit(): void {
    this.loadUsers();
  }

  // Deactivate modal
  showConfirmModal = false;
  itemToDeactivate: AdminUserListItemResponse | null = null;

  // Reset password modal
  showResetPasswordModal = false;
  itemToReset: AdminUserListItemResponse | null = null;

  // Password result modal
  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';

  public cols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre', sortable: true },
    { key: 'email', label: 'Email', sortable: true },
    {
      key: 'role',
      label: 'Rol',
      type: 'badge',
      badgeMap: {
        'Admin':                { color: 'danger',  label: 'Administrador'            },
        'Professional':         { color: 'primary', label: 'Profesional'              },
        'FamilyRepresentative': { color: 'success', label: 'Representante Familiar'   },
        'PersonWithDisability': { color: 'info',    label: 'Persona con Discapacidad' },
      },
    },
    { key: 'isActive', label: 'Estado', type: 'badge', badgeMap: { 'true': { color: 'success', label: 'Activo' }, 'false': { color: 'danger', label: 'Inactivo' } } },
    { key: 'lastLoginDate', label: 'Último acceso', type: 'date' },
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver', icon: 'cilSearch' },
        { action: 'reset-password', label: 'Resetear', icon: 'cilLockUnlocked', visible: (item) => item.isActive },
        { action: 'deactivate', label: 'Desactivar', icon: 'cilX', visible: (item) => item.isActive },
        { action: 'reactivate', label: 'Reactivar', icon: 'cilCheck', visible: (item) => !item.isActive },
      ],
    },
  ];

  onRoleFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  clearFilters(): void {
    this.selectedRole = '';
    this.selectedStatus = '';
    this.currentPage = 1;
    this.loadUsers();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadUsers();
  }

  onSort(event: { sortBy: string; sortDirection: 'ASC' | 'DESC' }): void {
    const sortMap: Record<string, string> = {
      'fullName': 'FirstName',
      'email': 'Email',
    };
    this.sortBy = sortMap[event.sortBy] ?? event.sortBy;
    this.sortDirection = event.sortDirection;
    this.currentPage = 1;
    this.loadUsers();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadUsers();
  }

  onRowAction(event: { action: string; item: any }): void {
    const user = event.item as AdminUserListItemResponse;
    switch (event.action) {
      case 'view':
        this.router.navigate([AppRoutes.Admin.Users, user.userId]);
        break;
      case 'reset-password':
        this.itemToReset = user;
        this.showResetPasswordModal = true;
        break;
      case 'deactivate':
        this.itemToDeactivate = user;
        this.showConfirmModal = true;
        break;
      case 'reactivate':
        this.reactivateUser(user);
        break;
    }
  }

  confirmResetPassword(): void {
    if (!this.itemToReset) return;
    const user = this.itemToReset;
    this.showResetPasswordModal = false;
    this.itemToReset = null;
    this.userService.resetPassword(user.userId).subscribe({
      next: (result) => {
        this.tempPassword = result.temporaryPassword;
        this.tempPasswordEmail = result.userEmail;
        this.showPasswordModal = true;
        this.toastService.success('Contraseña reseteada exitosamente');
      },
      error: () => {
        this.toastService.error('Error al resetear la contraseña');
      },
    });
  }

  cancelResetPassword(): void {
    this.showResetPasswordModal = false;
    this.itemToReset = null;
  }

  confirmDeactivate(): void {
    if (!this.itemToDeactivate) return;

    this.userService.deactivateUser(this.itemToDeactivate.userId).subscribe({
      next: () => {
        this.toastService.success('Usuario desactivado exitosamente');
        this.showConfirmModal = false;
        this.itemToDeactivate = null;
        this.loadUsers();
      },
      error: () => {
        this.toastService.error('Error al desactivar el usuario');
        this.showConfirmModal = false;
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
    this.itemToDeactivate = null;
  }

  reactivateUser(user: AdminUserListItemResponse): void {
    this.userService.reactivateUser(user.userId).subscribe({
      next: (result) => {
        this.tempPassword = result.temporaryPassword;
        this.tempPasswordEmail = result.userEmail;
        this.showPasswordModal = true;
        this.toastService.success('Usuario reactivado exitosamente');
        this.loadUsers();
      },
      error: () => {
        this.toastService.error('Error al reactivar el usuario');
      },
    });
  }

  closePasswordModal(): void {
    this.showPasswordModal = false;
    this.tempPassword = '';
    this.tempPasswordEmail = '';
  }

  copyPassword(): void {
    navigator.clipboard.writeText(this.tempPassword).then(() => {
      this.toastService.success('Contraseña copiada al portapapeles');
    });
  }

  loadUsers(): void {
    this.loading = true;
    this.userService
      .getUsers({
        page: this.currentPage,
        pageSize: this.pageSize,
        search: this.searchTerm || undefined,
        role: this.selectedRole || undefined,
        isActive: this.selectedStatus === '' ? undefined : this.selectedStatus === 'true',
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
      })
      .subscribe({
        next: (response) => {
          this.users = [...response.data];
          this.totalItems = response.totalRecords;
          this.loading = false;
        },
        error: () => {
          this.toastService.error('Error al obtener usuarios');
          this.loading = false;
        },
      });
  }
}
