import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, ProfessionalsService, ToastService, UserManagementService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import { AppRoutes } from '@shared/constants/app-routes';
import { ProfessionalListItemResponse, ValidateProfessionalRequest } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { InstitutionFilterComponent } from '@shared/components/institution-filter/institution-filter.component';
import { NavModule, ModalModule, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent, FormLabelDirective, FormSelectDirective, ButtonDirective, SpinnerComponent, TableDirective, BadgeComponent, AlertComponent, GridModule } from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-list',
  standalone: true,
  imports: [
    DatePipe,
    DataTableComponent,
    ConfirmModalComponent,
    InstitutionFilterComponent,
    NavModule,
    ModalModule,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    FormsModule,
    FormLabelDirective,
    FormSelectDirective,
    ButtonDirective,
    SpinnerComponent,
    TableDirective,
    BadgeComponent,
    IconModule,
    AlertComponent,
    GridModule,
  ],
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss'],
})
export class ListComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly userService = inject(UserManagementService);

  canCreate = this.authService.hasPermission(Permissions.Professionals.Create);
  canValidate = this.authService.hasPermission(Permissions.Professionals.Update) || this.authService.isGlobalAdmin();

  selectedInstitutionId: number | undefined;
  activeTab: 'active' | 'validations' = 'active';
  private isInitialized = false;
  statusFilter = '';

  professionals: ProfessionalListItemResponse[] = [];
  pendingProfessionals: ProfessionalListItemResponse[] = [];
  totalItems = 0;
  pendingCount = 0;
  pageSize = 10;
  currentPage = 1;
  sortBy = 'lastName';
  sortDirection: 'ASC' | 'DESC' = 'ASC';
  loading = false;

  showConfirmModal = false;
  showValidateModal = false;
  showHistoryModal = false;
  showReactivateModal = false;
  isValidationLoading = false;
  isReactivateLoading = false;
  isDeactivateLoading = false;
  itemToDeactivate: ProfessionalListItemResponse | null = null;
  itemToValidate: ProfessionalListItemResponse | null = null;
  itemToReactivate: ProfessionalListItemResponse | null = null;
  isApproveAction = true;
  statusHistory: any[] = [];
  statusHistoryLoading = false;

  // Reset password
  showResetPasswordModal = false;
  itemToResetPassword: ProfessionalListItemResponse | null = null;

  // Password modal
  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';

  readonly statusMap: Record<string, { color: string; label: string }> = {
    'pending':    { color: 'warning',   label: 'Pendiente'    },
    'approved':   { color: 'success',   label: 'Aprobado'     },
    'terminated': { color: 'secondary', label: 'Dado de baja' },
    'suspended':  { color: 'warning',   label: 'Suspendido'   },
    'rejected':   { color: 'danger',    label: 'Rechazado'    },
  };

  public cols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre', sortable: true },
    { key: 'specialty', label: 'Especialidad', sortable: true },
    { key: 'licenseNumber', label: 'Matrícula', sortable: true },
    { key: 'status', label: 'Estado', type: 'badge', sortable: true, badgeMap: this.statusMap },
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver', icon: 'cilSearch' },
        { action: 'reset-password', label: 'Resetear', icon: 'cilReload', visible: (item) => item.status === 'Approved' },
        { action: 'history', label: 'Historial', icon: 'cilHistory' },
        { action: 'persons', label: 'Personas', icon: 'cilPeople' },
        { action: 'institutions', label: 'Instituciones', icon: 'cilBook' },
        { action: 'edit', label: 'Editar', icon: 'cilNotes', visible: (item) => item.status === 'Approved' },
        { action: 'deactivate', label: 'Desactivar', icon: 'cilX', visible: (item) => item.status === 'Approved' },
        { action: 'reactivate', label: 'Reactivar', icon: 'cilReload', visible: (item) => item.status !== 'Approved' },
      ],
    },
  ];

  public pendingCols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre', sortable: true },
    { key: 'email', label: 'Email', sortable: true },
    { key: 'specialty', label: 'Especialidad', sortable: true },
    { key: 'licenseNumber', label: 'Matrícula', sortable: true },
    { key: 'createdAt', label: 'Fecha de solicitud', type: 'date', sortable: true },
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'approve', label: 'Aprobar', icon: 'cilCheck' },
        { action: 'reject', label: 'Rechazar', icon: 'cilX' },
      ],
    },
  ];

  ngOnInit(): void {
    this.loadPendingCount();
  }

  onFilterLoaded(): void {
    if (!this.isInitialized) {
      this.isInitialized = true;
      this.loadProfessionals();
    }
  }

  switchTab(tab: 'active' | 'validations'): void {
    this.activeTab = tab;
    this.currentPage = 1;
    if (tab === 'active') {
      this.loadProfessionals();
    } else {
      this.loadPendingProfessionals();
    }
  }

  onInstitutionFilterChange(institutionId: number | undefined): void {
    this.selectedInstitutionId = institutionId;
    if (this.activeTab === 'active') {
      this.currentPage = 1;
      this.loadProfessionals();
    }
  }

  onStatusFilterChange(status: string): void {
    this.statusFilter = status;
    this.currentPage = 1;
    this.loadProfessionals();
  }

  clearFilters(): void {
    this.statusFilter = '';
    this.currentPage = 1;
    this.loadProfessionals();
  }

  onSort(event: { sortBy: string; sortDirection: 'ASC' | 'DESC' }): void {
    const sortMap: Record<string, string> = {
      'fullName': 'LastName',
      'specialty': 'Specialty',
      'licenseNumber': 'LicenseNumber',
      'status': 'Status',
      'email': 'Email',
      'createdAt': 'CreatedAt',
    };
    this.sortBy = sortMap[event.sortBy] ?? event.sortBy;
    this.sortDirection = event.sortDirection;
    this.currentPage = 1;
    if (this.activeTab === 'active') {
      this.loadProfessionals();
    } else {
      this.loadPendingProfessionals();
    }
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    if (this.activeTab === 'active') {
      this.loadProfessionals();
    } else {
      this.loadPendingProfessionals();
    }
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    if (this.activeTab === 'active') {
      this.loadProfessionals(term);
    } else {
      this.loadPendingProfessionals(term);
    }
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate([AppRoutes.Admin.Professionals + '/new']);
    } else if (action === 'export') {
      this.exportToCsv();
    }
  }

  getHeaderButtons(): { action: string; label: string }[] {
    const buttons: { action: string; label: string }[] = [];
    if (this.canCreate) buttons.push({ action: 'new', label: 'Agregar' });
    if (this.professionals.length) buttons.push({ action: 'export', label: 'Exportar' });
    return buttons;
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'view':
        this.router.navigate([AppRoutes.Admin.Professionals, event.item.id]);
        break;
      case 'reset-password':
        this.itemToResetPassword = event.item;
        this.showResetPasswordModal = true;
        break;
      case 'persons':
        this.router.navigate([AppRoutes.Admin.Professionals, event.item.id], { queryParams: { tab: 'personas' } });
        break;
      case 'institutions':
        this.router.navigate([AppRoutes.Admin.Professionals, event.item.id], { queryParams: { tab: 'instituciones' } });
        break;
      case 'edit':
        this.router.navigate([AppRoutes.Admin.Professionals, event.item.id, 'edit']);
        break;
      case 'deactivate':
        this.itemToDeactivate = event.item;
        this.showConfirmModal = true;
        break;
      case 'approve':
        this.itemToValidate = event.item;
        this.isApproveAction = true;
        this.showValidateModal = true;
        break;
      case 'reject':
        this.itemToValidate = event.item;
        this.isApproveAction = false;
        this.showValidateModal = true;
        break;
      case 'history':
        this.loadStatusHistory(event.item.id);
        break;
      case 'reactivate':
        this.itemToReactivate = event.item;
        this.showReactivateModal = true;
        break;
    }
  }

  confirmDeactivate(observation: string): void {
    if (!this.itemToDeactivate) return;

    this.isDeactivateLoading = true;

    this.professionalsService.deactivateProfessional(this.itemToDeactivate.id, { observation }).subscribe({
      next: () => {
        this.isDeactivateLoading = false;
        this.toastService.success('Profesional desactivado exitosamente');
        this.showConfirmModal = false;
        this.itemToDeactivate = null;
        this.loadProfessionals();
      },
      error: (err) => {
        this.isDeactivateLoading = false;
        this.toastService.error(err?.userMessage || 'Error al desactivar el profesional');
        this.showConfirmModal = false;
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
    this.itemToDeactivate = null;
  }

  onValidationConfirm(observation: string): void {
    if (!this.itemToValidate) return;

    this.isValidationLoading = true;

    const request: ValidateProfessionalRequest = {
      isApproved: this.isApproveAction,
      observation: this.isApproveAction ? undefined : observation,
    };

    this.professionalsService.validateProfessional(this.itemToValidate.id, request).subscribe({
      next: () => {
        this.isValidationLoading = false;
        this.toastService.success(
          this.isApproveAction
            ? 'Profesional aprobado exitosamente. Se ha enviado un email con las credenciales.'
            : 'Profesional rechazado exitosamente. Se ha notificado al solicitante.'
        );
        this.showValidateModal = false;
        this.itemToValidate = null;
        this.loadPendingProfessionals();
        this.loadPendingCount();
      },
      error: (err) => {
        this.isValidationLoading = false;
        this.toastService.error(err?.userMessage || 'Error al procesar la validación');
      },
    });
  }

  cancelValidation(): void {
    this.showValidateModal = false;
    this.itemToValidate = null;
  }

  loadProfessionals(search?: string): void {
    this.loading = true;
    this.professionalsService
      .getProfessionals({
        page: this.currentPage,
        pageSize: this.pageSize,
        search,
        institutionId: this.selectedInstitutionId,
        status: this.statusFilter || undefined,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
      })
      .subscribe({
        next: (response) => {
          this.professionals = response.data;
          this.totalItems = response.totalRecords;
          this.loading = false;
        },
        error: () => {
          this.toastService.error('Error al obtener profesionales');
          this.loading = false;
        },
      });
  }

  loadPendingProfessionals(search?: string): void {
    this.loading = true;
    this.professionalsService
      .getPendingProfessionals({
        page: this.currentPage,
        pageSize: this.pageSize,
        search,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
      })
      .subscribe({
        next: (response) => {
          this.pendingProfessionals = response.data;
          this.pendingCount = response.totalRecords;
          this.loading = false;
        },
        error: () => {
          this.toastService.error('Error al obtener solicitudes pendientes');
          this.loading = false;
        },
      });
  }

  loadPendingCount(): void {
    this.professionalsService
      .getPendingProfessionals({
        page: 1,
        pageSize: 1,
      })
      .subscribe({
        next: (response) => {
          this.pendingCount = response.totalRecords;
        },
        error: () => {
          this.pendingCount = 0;
        },
      });
  }

  onModalVisibleChange(visible: boolean): void {
    if (!visible) {
      this.cancelValidation();
    }
  }

  loadStatusHistory(professionalId: string): void {
    this.statusHistoryLoading = true;
    this.showHistoryModal = true;
    this.professionalsService.getStatusHistory(professionalId).subscribe({
      next: (data) => {
        this.statusHistory = data;
        this.statusHistoryLoading = false;
      },
      error: () => {
        this.toastService.error('Error al obtener historial de estados');
        this.statusHistoryLoading = false;
      },
    });
  }

  confirmResetPassword(): void {
    const item = this.itemToResetPassword;
    if (!item) return;
    if (!item.userId) {
      this.toastService.error('El profesional no tiene usuario asociado');
      this.cancelResetPassword();
      return;
    }
    this.userService.resetPassword(item.userId).subscribe({
      next: (result) => {
        this.tempPassword = result.temporaryPassword;
        this.tempPasswordEmail = result.userEmail;
        this.showResetPasswordModal = false;
        this.itemToResetPassword = null;
        this.showPasswordModal = true;
        this.toastService.success('Contraseña reseteada exitosamente');
      },
      error: () => {
        this.toastService.error('Error al resetear la contraseña');
        this.cancelResetPassword();
      },
    });
  }

  cancelResetPassword(): void {
    this.showResetPasswordModal = false;
    this.itemToResetPassword = null;
  }

  copyPassword(): void {
    navigator.clipboard.writeText(this.tempPassword).then(() => {
      this.toastService.success('Contraseña copiada al portapapeles');
    });
  }

  closePasswordModal(): void {
    this.showPasswordModal = false;
    this.tempPassword = '';
    this.tempPasswordEmail = '';
  }

  confirmReactivate(): void {
    if (!this.itemToReactivate) return;
    this.isReactivateLoading = true;
    this.professionalsService.reactivateProfessional(this.itemToReactivate.id).subscribe({
      next: () => {
        this.isReactivateLoading = false;
        this.toastService.success('Profesional reactivado exitosamente');
        this.showReactivateModal = false;
        this.itemToReactivate = null;
        this.loadProfessionals();
      },
      error: () => {
        this.isReactivateLoading = false;
        this.toastService.error('Error al reactivar el profesional');
      },
    });
  }

  cancelReactivate(): void {
    this.showReactivateModal = false;
    this.itemToReactivate = null;
  }

  exportToCsv(): void {
    const data = this.activeTab === 'active' ? this.professionals : this.pendingProfessionals;
    if (!data.length) return;
    const headers = Object.keys(data[0]);
    const csvContent = [
      headers.join(','),
      ...data.map(row =>
        headers.map(h => {
          const val = String((row as any)[h] ?? '');
          return `"${val.replaceAll('"', '""')}"`;
        }).join(',')
      ),
    ].join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `profesionales_${this.activeTab}_${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

}
