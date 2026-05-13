import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ToastService } from '@services';
import { UserRoles } from '@shared/constants/roles';
import { AppRoutes } from '@shared/constants/app-routes';
import { UserManagementService } from '../../../../services/user-management.service';
import { AdminUserDetailResponse, UserRecentSessionResponse } from '@models';
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
  SpinnerComponent,
  TableDirective,
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
    SpinnerComponent,
    TableDirective,
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
  recentSessions: UserRecentSessionResponse[] = [];
  sessionsLoading = true;

  showDeactivateModal = false;
  showResetPasswordModal = false;
  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';

  ngOnInit(): void {
    const userId = this.route.snapshot.paramMap.get('id');
    if (userId) {
      this.loadUser(userId);
      this.loadSessions(userId);
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

  loadSessions(userId: string): void {
    this.sessionsLoading = true;
    this.userService.getUserActivity(userId).subscribe({
      next: (sessions) => {
        this.recentSessions = sessions;
        this.sessionsLoading = false;
      },
      error: () => {
        this.sessionsLoading = false;
      },
    });
  }

  getSessionStatus(session: UserRecentSessionResponse): { color: string; label: string } {
    if (!session.isActive) return { color: 'danger',  label: 'Revocada' };
    if (new Date(session.expiresAt) < new Date()) return { color: 'warning', label: 'Expirada' };
    return { color: 'success', label: 'Activa' };
  }

  formatUserAgent(ua: string | null): string {
    if (!ua) return '—';
    if (ua.includes('Chrome') && !ua.includes('Chromium')) return 'Chrome';
    if (ua.includes('Firefox')) return 'Firefox';
    if (ua.includes('Safari') && !ua.includes('Chrome')) return 'Safari';
    if (ua.includes('Edge')) return 'Edge';
    if (ua.includes('MSIE') || ua.includes('Trident')) return 'IE';
    return ua.length > 40 ? ua.slice(0, 37) + '…' : ua;
  }

  get roleBadgeColor(): string {
    switch (this.user?.role) {
      case UserRoles.Admin:                return 'primary';
      case UserRoles.Professional:         return 'info';
      case UserRoles.FamilyRepresentative: return 'warning';
      case UserRoles.PersonWithDisability: return 'success';
      default: return 'secondary';
    }
  }

  get roleLabel(): string {
    switch (this.user?.role) {
      case UserRoles.Admin:                return 'Administrador';
      case UserRoles.Professional:         return 'Profesional';
      case UserRoles.FamilyRepresentative: return 'Familiar';
      case UserRoles.PersonWithDisability: return 'Persona';
      default: return this.user?.role ?? '';
    }
  }

  get entityTypeLabel(): string {
    switch (this.user?.linkedEntity?.entityType) {
      case UserRoles.Professional:         return 'Profesional';
      case UserRoles.PersonWithDisability: return 'Persona con Discapacidad';
      case UserRoles.FamilyRepresentative: return 'Familiar';
      case UserRoles.Admin:                return 'Administrador';
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
    this.router.navigate([AppRoutes.Admin.Users]);
  }
}
