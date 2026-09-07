import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe, NgClass } from '@angular/common';
import { ToastService } from '@services';
import { UserRoles } from '@shared/constants/roles';
import { AppRoutes } from '@shared/constants/app-routes';
import { UserManagementService } from '@services/user-management.service';
import { AdminUserDetailResponse, UserRecentSessionResponse } from '@models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { IconDirective } from '@coreui/icons-angular';
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
  ModalTitleDirective,
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
    ModalTitleDirective,
    ConfirmModalComponent,
    SpinnerComponent,
    TableDirective,
    IconDirective,
    NgClass,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
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
      case UserRoles.Admin:                return 'danger';
      case UserRoles.Professional:         return 'primary';
      case UserRoles.FamilyRepresentative: return 'warning';
      case UserRoles.PersonWithDisability: return 'info';
      default: return 'secondary';
    }
  }

  get roleLabel(): string {
    switch (this.user?.role) {
      case UserRoles.Admin:                return 'Administrador';
      case UserRoles.Professional:         return 'Profesional';
      case UserRoles.FamilyRepresentative: return 'Representante Familiar';
      case UserRoles.PersonWithDisability: return 'Alumno';
      default: return this.user?.role ?? '';
    }
  }

  get roleIcon(): string {
    switch (this.user?.role) {
      case UserRoles.Admin:                return 'cilSettings';
      case UserRoles.Professional:         return 'cilUser';
      case UserRoles.FamilyRepresentative: return 'cilHome';
      case UserRoles.PersonWithDisability: return 'cilPeople';
      default: return 'cilUser';
    }
  }

  get initials(): string {
    if (!this.user?.fullName) return 'U';
    const parts = this.user.fullName.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  get avatarClass(): string {
    switch (this.user?.role) {
      case UserRoles.Admin:                return 'avatar-admin';
      case UserRoles.Professional:         return 'avatar-professional';
      case UserRoles.FamilyRepresentative: return 'avatar-family';
      case UserRoles.PersonWithDisability: return 'avatar-person';
      default: return 'avatar-professional';
    }
  }

  get linkedEntityTitle(): string {
    switch (this.user?.linkedEntity?.entityType) {
      case UserRoles.Professional:         return 'Datos Profesionales';
      case UserRoles.FamilyRepresentative: return 'Datos del Representante Familiar';
      case UserRoles.PersonWithDisability: return 'Datos del Alumno';
      default: return 'Entidad Vinculada';
    }
  }

  get linkedEntityIcon(): string {
    switch (this.user?.linkedEntity?.entityType) {
      case UserRoles.Professional:         return 'cilUser';
      case UserRoles.FamilyRepresentative: return 'cilHome';
      case UserRoles.PersonWithDisability: return 'cilPeople';
      default: return 'cilLink';
    }
  }

  get viewProfileButtonText(): string {
    switch (this.user?.linkedEntity?.entityType) {
      case UserRoles.Professional:         return 'Ver perfil completo del profesional';
      case UserRoles.FamilyRepresentative: return 'Ver perfil completo del familiar';
      case UserRoles.PersonWithDisability: return 'Ver perfil completo del alumno';
      default: return 'Ver perfil vinculado';
    }
  }

  goToLinkedEntity(): void {
    if (!this.user?.linkedEntity?.entityId) return;
    const { entityType, entityId } = this.user.linkedEntity;
    switch (entityType) {
      case UserRoles.Professional:
        this.router.navigate([AppRoutes.Admin.Professionals, entityId]);
        break;
      case UserRoles.FamilyRepresentative:
        this.router.navigate([AppRoutes.Admin.Family, entityId]);
        break;
      case UserRoles.PersonWithDisability:
        this.router.navigate([AppRoutes.Admin.Persons, entityId]);
        break;
    }
  }

  get entityTypeLabel(): string {
    switch (this.user?.linkedEntity?.entityType) {
      case UserRoles.Professional:         return 'Profesional';
      case UserRoles.PersonWithDisability: return 'Alumno';
      case UserRoles.FamilyRepresentative: return 'Representante Familiar';
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
    if (this.user.role === UserRoles.FamilyRepresentative) {
      this.showDeactivateModal = false;
      return;
    }

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
    if (this.user.role === UserRoles.FamilyRepresentative) return;

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
