import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { AuthService, ProfessionalsService, ToastService, UserManagementService, CatalogsService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import { AppRoutes } from '@shared/constants';
import { ProfessionalListItemResponse, ProfessionalResponse } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { InstitutionFilterComponent } from '@shared/components/institution-filter/institution-filter.component';
import { ModalModule, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent, FormLabelDirective, FormSelectDirective, ButtonDirective, SpinnerComponent, TableDirective, BadgeComponent, AlertComponent, GridModule } from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

const PROFESSIONAL_CSV_HEADERS = [
  'Nombre',
  'Apellido',
  'Documento',
  'Teléfono',
  'Especialidad',
  'Matrícula',
  'Email',
  'Estado',
] as const;

const PROFESSIONAL_STATUS_LABELS: Record<string, string> = {
  pending: 'Pendiente',
  approved: 'Aprobado',
  terminated: 'Dado de baja',
  suspended: 'Suspendido',
  rejected: 'Rechazado',
};

function csvEscape(value: unknown): string {
  return `"${String(value ?? '').replaceAll('"', '""')}"`;
}

export function getProfessionalStatusLabel(status?: string): string {
  const normalizedStatus = status?.trim().toLowerCase();
  return normalizedStatus ? PROFESSIONAL_STATUS_LABELS[normalizedStatus] ?? status! : '';
}

export function buildProfessionalsCsv(data: ProfessionalListItemResponse[]): string {
  const rows = data.map(professional => [
    professional.firstName,
    professional.lastName,
    professional.documentNumber,
    professional.phone,
    professional.specialty,
    professional.licenseNumber,
    professional.email,
    getProfessionalStatusLabel(professional.status),
  ]);

  return '\uFEFF' + [
    PROFESSIONAL_CSV_HEADERS.map(csvEscape).join(';'),
    ...rows.map(row => row.map(csvEscape).join(';')),
  ].join('\r\n');
}

@Component({
  selector: 'app-list',
  standalone: true,
  imports: [
    DatePipe,
    DataTableComponent,
    ConfirmModalComponent,
    InstitutionFilterComponent,
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
  private readonly catalogsService = inject(CatalogsService);
  private readonly destroyRef = inject(DestroyRef);

  canCreate = this.authService.hasPermission(Permissions.Professionals.Create);

  specialties: { id: number; name: string }[] = [];
  specialtiesLoadError = false;

  selectedInstitutionId: number | undefined;
  private isInitialized = false;
  statusFilter = '';
  specialtyFilter = '';

  professionals: ProfessionalListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  sortBy = 'lastName';
  sortDirection: 'ASC' | 'DESC' = 'ASC';
  loading = false;
  private professionalsRequestId = 0;
  private searchTerm = '';

  showConfirmModal = false;
  showHistoryModal = false;
  showReactivateModal = false;
  isReactivateLoading = false;
  isDeactivateLoading = false;
  itemToDeactivate: ProfessionalListItemResponse | null = null;
  itemToReactivate: ProfessionalListItemResponse | null = null;
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
      ],
    },
  ];

  ngOnInit(): void {
    this.catalogsService.getSpecialties()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.specialties = data.filter(specialty => specialty.isActive === true);
          this.specialtiesLoadError = false;
        },
        error: (error) => {
          this.specialties = [];
          this.specialtiesLoadError = true;
          console.error('Failed to load active professional specialties', error);
          this.toastService.error('No se pudieron cargar las especialidades. Revisá el endpoint /Catalogs/specialties.');
        },
      });
  }

  onFilterLoaded(): void {
    if (!this.isInitialized) {
      this.isInitialized = true;
      this.loadProfessionals();
    }
  }

  onInstitutionFilterChange(institutionId: number | undefined): void {
    this.selectedInstitutionId = institutionId;
    this.currentPage = 1;
    this.loadProfessionals();
  }

  onStatusFilterChange(status: string): void {
    this.statusFilter = status;
    this.currentPage = 1;
    this.loadProfessionals();
  }

  onSpecialtyFilterChange(specialty: string): void {
    this.specialtyFilter = specialty;
    this.currentPage = 1;
    this.loadProfessionals();
  }

  clearFilters(): void {
    this.statusFilter = '';
    this.specialtyFilter = '';
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
    this.loadProfessionals();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadProfessionals();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadProfessionals(term);
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
      next: (response) => {
        this.isDeactivateLoading = false;
        this.toastService.success('Profesional desactivado exitosamente');
        this.showConfirmModal = false;
        this.itemToDeactivate = null;
        this.replaceProfessionalFromMutation(response, 'terminated');
        this.loadProfessionals();
      },
      error: (err) => {
        this.isDeactivateLoading = false;
        if (err?.errorCode === 710 || err?.errorCode === 'HAS_PENDING_REPORTS') {
          this.toastService.error('Este profesional tiene informes pendientes. Debe reasignarlos o finalizarlos antes de proceder con la baja.');
          
          /* TODO: Reemplazar este aviso por un Componente Modal interactivo que permita
             seleccionar un nuevo profesional y transferir los reportes.
             this.showTransferReportsModal = true;
          */
        } else {
          this.toastService.error(err?.userMessage || 'Error al desactivar el profesional');
        }
        this.showConfirmModal = false;
      },
    });
  }

  cancelDeactivate(): void {
    this.showConfirmModal = false;
    this.itemToDeactivate = null;
  }

  loadProfessionals(search?: string): void {
    if (search !== undefined) this.searchTerm = search;
    const requestId = ++this.professionalsRequestId;
    this.loading = true;
    this.professionalsService
      .getProfessionals({
        page: this.currentPage,
        pageSize: this.pageSize,
        search: this.searchTerm || undefined,
        institutionId: this.selectedInstitutionId,
        status: this.statusFilter || undefined,
        specialty: this.specialtyFilter || undefined,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
      })
      .subscribe({
        next: (response) => {
          if (requestId !== this.professionalsRequestId) return;
          this.professionals = response.data;
          this.totalItems = response.totalRecords;
          this.loading = false;
        },
        error: () => {
          if (requestId !== this.professionalsRequestId) return;
          this.toastService.error('Error al obtener profesionales');
          this.loading = false;
        },
      });
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
      next: (response) => {
        this.isReactivateLoading = false;
        this.toastService.success('Profesional reactivado exitosamente');
        this.showReactivateModal = false;
        this.itemToReactivate = null;
        this.replaceProfessionalFromMutation(response, 'approved');
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

  private replaceProfessionalFromMutation(response: ProfessionalResponse, fallbackStatus: string): void {
    const statusKeys: Record<string, string> = {
      'Aprobado': 'approved',
      'Dado de baja': 'terminated',
      'Suspendido': 'suspended',
      'Rechazado': 'rejected',
      'Pendiente': 'pending',
    };
    const status = response.statusName
      ? (statusKeys[response.statusName] ?? response.statusName.toLowerCase())
      : fallbackStatus;
    this.professionals = this.professionals.map(item => item.id === response.id
      ? { ...item, isActive: response.isActive, status }
      : item);
  }

  exportToCsv(): void {
    if (!this.professionals.length) return;
    const csvContent = buildProfessionalsCsv(this.professionals);
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `profesionales_${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

}
