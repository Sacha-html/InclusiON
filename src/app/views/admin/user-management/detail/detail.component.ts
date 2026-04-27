import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ToastService } from '@services';
import { UserManagementService } from '../../../../services/user-management.service';
import { AdminUserDetailResponse } from '../../../../models/responses/admin-user-detail.response';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  BadgeComponent,
  ButtonDirective,
  AlertComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
} from '@coreui/angular';


@Component({
  selector: 'app-user-management-detail',
  imports: [
    DatePipe,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ColComponent,
    RowComponent,
    BadgeComponent,
    ButtonDirective,
    AlertComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ConfirmModalComponent,
  ],
  templateUrl: './detail.component.html',
})
export class UserManagementDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userService = inject(UserManagementService);
  private readonly toastService = inject(ToastService);

  user: AdminUserDetailResponse | null = null;
  isLoading = true;

  showDeactivateModal = false;
  showResetPasswordModal = false;
  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';

  ngOnInit(): void {
    const userId = this.route.snapshot.paramMap.get('id');
    if (userId) {
      this.loadUser(userId);
    }
  }

  loadUser(userId: string): void {
    this.isLoading = true;
    this.userService.getUserDetail(userId).subscribe({
      next: (user) => {
        this.user = user;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Error al cargar el detalle del usuario');
        this.isLoading = false;
      },
    });
  }

  get roleBadgeColor(): string {
    switch (this.user?.role) {
      case 'Admin': return 'primary';
      case 'Professional': return 'info';
      case 'FamilyRepresentative': return 'warning';
      case 'PersonWithDisability': return 'success';
      default: return 'secondary';
    }
  }

  get roleLabel(): string {
    switch (this.user?.role) {
      case 'Admin': return 'Administrador';
      case 'Professional': return 'Profesional';
      case 'FamilyRepresentative': return 'Familiar';
      case 'PersonWithDisability': return 'Persona';
      default: return this.user?.role ?? '';
    }
  }

  get entityTypeLabel(): string {
    switch (this.user?.linkedEntity?.entityType) {
      case 'Professional': return 'Profesional';
      case 'PersonWithDisability': return 'Persona con Discapacidad';
      case 'FamilyRepresentative': return 'Familiar';
      case 'Admin': return 'Administrador';
      default: return '';
    }
  }

  resetPassword(): void {
    if (!this.user) return;
    this.showResetPasswordModal = true;
  }

  confirmResetPassword(): void {
    if (!this.user) return;
    this.showResetPasswordModal = false;
    this.userService.resetPassword(this.user.userId).subscribe({
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
    if (!this.user) return;
    this.userService.deactivateUser(this.user.userId).subscribe({
      next: () => {
        this.toastService.success('Usuario desactivado exitosamente');
        this.showDeactivateModal = false;
        this.loadUser(this.user!.userId);
      },
      error: () => {
        this.toastService.error('Error al desactivar el usuario');
        this.showDeactivateModal = false;
      },
    });
  }

  reactivateUser(): void {
    if (!this.user) return;
    this.userService.reactivateUser(this.user.userId).subscribe({
      next: (result) => {
        this.tempPassword = result.temporaryPassword;
        this.tempPasswordEmail = result.userEmail;
        this.showPasswordModal = true;
        this.toastService.success('Usuario reactivado exitosamente');
        this.loadUser(this.user!.userId);
      },
      error: () => {
        this.toastService.error('Error al reactivar el usuario');
      },
    });
  }

  closePasswordModal(): void {
    this.showPasswordModal = false;
    this.tempPassword = '';
  }

  copyPassword(): void {
    navigator.clipboard.writeText(this.tempPassword).then(() => {
      this.toastService.success('Contraseña copiada al portapapeles');
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/users']);
  }
}
