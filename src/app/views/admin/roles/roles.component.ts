import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RolesService, ToastService } from '@services';
import { RoleResponse } from '@models';

import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  TableDirective,
  ButtonDirective,
  BadgeComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  SpinnerComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  RowComponent,
  ColComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    TableDirective,
    ButtonDirective,
    BadgeComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    SpinnerComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    RowComponent,
    ColComponent,
  ],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss',
})
export class RolesComponent implements OnInit {
  private readonly rolesService = inject(RolesService);
  private readonly toastService = inject(ToastService);

  roles: RoleResponse[] = [];
  availablePermissions: string[] = [];
  isLoading = true;
  isSaving = false;

  // Modal state
  showModal = false;
  selectedRole: RoleResponse | null = null;
  selectedPermissions: Set<string> = new Set();

  // Permission groups for UI
  permissionGroups: { module: string; permissions: string[] }[] = [];

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.isLoading = true;

    this.rolesService.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
        this.loadPermissions();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar roles');
      },
    });
  }

  private loadPermissions(): void {
    this.rolesService.getAvailablePermissions().subscribe({
      next: (permissions) => {
        this.availablePermissions = permissions;
        this.buildPermissionGroups(permissions);
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar permisos');
      },
    });
  }

  private buildPermissionGroups(permissions: string[]): void {
    const groups = new Map<string, string[]>();

    for (const perm of permissions) {
      const [module] = perm.split(':');
      if (!groups.has(module)) {
        groups.set(module, []);
      }
      groups.get(module)!.push(perm);
    }

    this.permissionGroups = Array.from(groups.entries()).map(([module, perms]) => ({
      module,
      permissions: perms,
    }));
  }

  getModuleLabel(module: string): string {
    const labels: Record<string, string> = {
      users: 'Usuarios',
      persons: 'Personas',
      professionals: 'Profesionales',
      family: 'Familiares',
      activities: 'Actividades',
      reports: 'Reportes',
      messages: 'Mensajes',
      invitations: 'Invitaciones',
      settings: 'Configuracion',
      audit: 'Auditoria',
      institutions: 'Instituciones',
    };
    return labels[module] || module;
  }

  getActionLabel(permission: string): string {
    const action = permission.split(':')[1];
    const labels: Record<string, string> = {
      read: 'Ver',
      create: 'Crear',
      update: 'Editar',
      delete: 'Eliminar',
      export: 'Exportar',
      respond: 'Responder',
    };
    return labels[action] || action;
  }

  openEditModal(role: RoleResponse): void {
    this.selectedRole = role;
    this.selectedPermissions = new Set(role.permissions);
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedRole = null;
  }

  isPermissionSelected(permission: string): boolean {
    return this.selectedPermissions.has(permission);
  }

  togglePermission(permission: string): void {
    if (this.selectedPermissions.has(permission)) {
      this.selectedPermissions.delete(permission);
    } else {
      this.selectedPermissions.add(permission);
    }
  }

  toggleModule(module: string): void {
    const group = this.permissionGroups.find((g) => g.module === module);
    if (!group) return;

    const allSelected = group.permissions.every((p) => this.selectedPermissions.has(p));

    if (allSelected) {
      group.permissions.forEach((p) => this.selectedPermissions.delete(p));
    } else {
      group.permissions.forEach((p) => this.selectedPermissions.add(p));
    }
  }

  isModuleFullySelected(module: string): boolean {
    const group = this.permissionGroups.find((g) => g.module === module);
    if (!group) return false;
    return group.permissions.every((p) => this.selectedPermissions.has(p));
  }

  isModulePartiallySelected(module: string): boolean {
    const group = this.permissionGroups.find((g) => g.module === module);
    if (!group) return false;
    const selected = group.permissions.filter((p) => this.selectedPermissions.has(p));
    return selected.length > 0 && selected.length < group.permissions.length;
  }

  savePermissions(): void {
    if (!this.selectedRole) return;

    this.isSaving = true;
    const permissions = Array.from(this.selectedPermissions);

    this.rolesService.updateRolePermissions(this.selectedRole.id, permissions).subscribe({
      next: (updated) => {
        this.isSaving = false;
        const index = this.roles.findIndex((r) => r.id === updated.id);
        if (index >= 0) {
          this.roles[index] = updated;
        }
        this.toastService.success(`Permisos de ${updated.name} actualizados`);
        this.closeModal();
      },
      error: () => {
        this.isSaving = false;
        this.toastService.error('Error al guardar permisos');
      },
    });
  }

  getRoleLabel(name: string): string {
    const labels: Record<string, string> = {
      Admin: 'Administrador',
      Professional: 'Profesional',
      FamilyRepresentative: 'Representante Familiar',
      PersonWithDisability: 'Persona con Discapacidad',
    };
    return labels[name] || name;
  }

  getRoleBadgeColor(name: string): string {
    const colors: Record<string, string> = {
      Admin: 'danger',
      Professional: 'primary',
      FamilyRepresentative: 'success',
      PersonWithDisability: 'info',
    };
    return colors[name] || 'secondary';
  }
}
