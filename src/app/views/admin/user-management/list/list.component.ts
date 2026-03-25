import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, ToastService } from '@services';
import { UserManagementService } from '../../../../services/user-management.service';
import { AdminUserListItemResponse } from '../../../../models/responses/admin-user-list-item.response';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { InstitutionFilterComponent } from '@shared/components/institution-filter/institution-filter.component';
import {
  ColComponent,
  RowComponent,
  FormSelectDirective,
  CardComponent,
  CardBodyComponent,
  AlertComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  ButtonDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-user-management-list',
  imports: [
    FormsModule,
    DataTableComponent,
    ConfirmModalComponent,
    InstitutionFilterComponent,
    ColComponent,
    RowComponent,
    FormSelectDirective,
    AlertComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ButtonDirective,
  ],
  templateUrl: './list.component.html',
})
export class UserManagementListComponent {
  private readonly userService = inject(UserManagementService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  selectedInstitutionId: number | undefined;
  selectedRole = '';
  selectedStatus = '';
  searchTerm = '';

  users: AdminUserListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;

  // Deactivate modal
  showConfirmModal = false;
  itemToDeactivate: AdminUserListItemResponse | null = null;

  // Password modal
  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';

  public cols: TableColumn[] = [
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver detalle', icon: 'cil-search' },
        { action: 'reset-password', label: 'Resetear contraseña', icon: 'cil-lock-unlocked', visible: (item) => item.isActive },
        { action: 'deactivate', label: 'Desactivar', icon: 'cil-x', visible: (item) => item.isActive },
        { action: 'reactivate', label: 'Reactivar', icon: 'cil-check', visible: (item) => !item.isActive },
      ],
    },
    { key: 'fullName', label: 'Nombre' },
    { key: 'email', label: 'Email' },
    { key: 'role', label: 'Rol', type: 'badge' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
    { key: 'lastLoginDate', label: 'Último acceso', type: 'date' },
  ];

  onInstitutionFilterChange(institutionId: number | undefined): void {
    this.selectedInstitutionId = institutionId;
    this.currentPage = 1;
    this.loadUsers();
  }

  onRoleFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
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
        this.router.navigate(['/admin/users', user.userId]);
        break;
      case 'reset-password':
        this.resetPassword(user);
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

  resetPassword(user: AdminUserListItemResponse): void {
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
    this.userService
      .getUsers({
        page: this.currentPage,
        pageSize: this.pageSize,
        search: this.searchTerm || undefined,
        role: this.selectedRole || undefined,
        isActive: this.selectedStatus === '' ? undefined : this.selectedStatus === 'true',
        institutionId: this.selectedInstitutionId,
      })
      .subscribe({
        next: (response) => {
          this.users = response.data;
          this.totalItems = response.totalRecords;
        },
        error: () => {
          this.toastService.error('Error al obtener usuarios');
        },
      });
  }
}
