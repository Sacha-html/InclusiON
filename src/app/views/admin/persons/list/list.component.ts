import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, CatalogsService, PersonsService, ProfessionalsService, ToastService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import { AppRoutes } from '@shared/constants/app-routes';
import { LoginMethodItem, PersonListItemResponse, ProfessionalListItemResponse, UpdateLoginMethodRequest } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { InstitutionFilterComponent } from '@shared/components/institution-filter/institution-filter.component';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { FormsModule } from '@angular/forms';
import {
  AlertComponent,
  ButtonDirective,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  FormSelectDirective,
  GridModule,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  ModalTitleDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-list',
  imports: [
    DataTableComponent,
    InstitutionFilterComponent,
    ReactiveFormsModule,
    FormsModule,
    FormControlDirective,
    FormFeedbackComponent,
    FormLabelDirective,
    FormSelectDirective,
    GridModule,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ConfirmModalComponent,
    AlertComponent,
    ButtonDirective,
    ModalTitleDirective,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent {
  readonly #personsService = inject(PersonsService);
  readonly #authService = inject(AuthService);
  readonly #catalogsService = inject(CatalogsService);
  readonly #professionalsService = inject(ProfessionalsService);
  readonly #toastService = inject(ToastService);
  readonly #fb = inject(FormBuilder);
  readonly #router = inject(Router);

  canCreate = this.#authService.hasPermission(Permissions.Persons.Create);

  selectedInstitutionId: number | undefined;
  representativeSearch = '';
  statusFilter = '';

  isLoading = false;
  persons: PersonListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  sortBy = 'LastName';
  sortDirection: 'ASC' | 'DESC' = 'ASC';

  // Modal login method
  showLoginMethodModal = false;
  showLoginMethodConfirm = false;
  selectedPerson: PersonListItemResponse | null = null;
  loginMethods: LoginMethodItem[] = [];
  supervisors: ProfessionalListItemResponse[] = [];
  loginMethodForm: FormGroup = this.#fb.group({
    loginMethodId: [null, Validators.required],
    pin: ['', [Validators.pattern(/^\d{4}$/)]],
    supervisorUserId: [null],
  });
  loginMethodSubmitted = false;
  loginMethodError = '';
  loginMethodSuccess = '';
  temporaryPassword = '';

  get selectedLoginMethod(): LoginMethodItem | undefined {
    const id = this.loginMethodForm.get('loginMethodId')?.value;
    return id ? this.loginMethods.find(m => m.id === +id) : undefined;
  }

  public cols: TableColumn[] = [
    { key: 'fullName', label: 'Nombre completo', sortable: true },
    { key: 'representativeNames', label: 'Responsables' },
    { key: 'disabilityTypeName', label: 'Tipo de discapacidad' },
    { key: 'autonomyLevelName', label: 'Nivel de autonomía' },
    { key: 'age', label: 'Edad', type: 'number', sortable: true },
    {
      key: 'isActive', label: 'Estado', type: 'badge',
      badgeMap: {
        'true':  { color: 'success', label: 'Activo'   },
        'false': { color: 'danger',  label: 'Inactivo' },
      },
    },
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver', icon: 'cilSearch' },
        { action: 'edit', label: 'Editar', icon: 'cilNotes', visible: (item) => item.isActive },
        { action: 'login-method', label: 'Método login', icon: 'cilLockLocked', visible: (item) => item.isActive },
      ],
    },
  ];

  constructor() {
    this.#catalogsService.getLoginMethods().subscribe({
      next: (data) => this.loginMethods = data,
    });
  }

  onInstitutionFilterChange(institutionId: number | undefined): void {
    this.selectedInstitutionId = institutionId;
    this.currentPage = 1;
    this.loadPersons();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadPersons();
  }

  onSearch(term: string): void {
    this.currentPage = 1;
    this.loadPersons(term);
  }

  onSort(event: { sortBy: string; sortDirection: 'ASC' | 'DESC' }): void {
    const sortMap: Record<string, string> = {
      'fullName': 'LastName',
      'age': 'BirthDate',
    };
    this.sortBy = sortMap[event.sortBy] ?? event.sortBy;
    this.sortDirection = event.sortDirection;
    this.currentPage = 1;
    this.loadPersons();
  }

  onRepresentativeSearch(): void {
    this.currentPage = 1;
    this.loadPersons();
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadPersons();
  }

  clearFilters(): void {
    this.representativeSearch = '';
    this.statusFilter = '';
    this.currentPage = 1;
    this.loadPersons();
  }

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.#router.navigate([AppRoutes.Admin.Persons + '/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'view':
        this.#router.navigate([AppRoutes.Admin.Persons, event.item.id]);
        break;
      case 'edit':
        this.#router.navigate([AppRoutes.Admin.Persons, event.item.id, 'edit']);
        break;
      case 'login-method':
        this.openLoginMethodModal(event.item);
        break;
    }
  }

  openLoginMethodModal(person: PersonListItemResponse): void {
    this.selectedPerson = person;
    this.loginMethodSubmitted = false;
    this.loginMethodError = '';
    this.loginMethodSuccess = '';
    this.temporaryPassword = '';
    this.loginMethodForm.reset();
    this.showLoginMethodModal = true;

    // Si la persona tiene discapacidad, cargar solo profesionales asignados a ella
    // que pueden supervisar login
    if (person.disabilityTypeId) {
      this.#personsService.getProfessionalsByPerson(person.id).subscribe({
        next: (assignments) => {
          this.supervisors = assignments
            .filter(a => a.canSuperviseLogin && a.isActive)
            .map(a => ({
              id: a.professionalId,
              userId: a.professionalId,
              firstName: a.personFirstName.split(' ')[0],
              lastName: a.personLastName,
              fullName: a.personFullName,
              email: '',
              specialty: '',
              isActive: true,
              status: 'Active',
            }));
        },
      });
    } else {
      // Cargar lista de profesionales como posibles supervisores
      if (this.supervisors.length === 0) {
        this.#professionalsService.getProfessionals({ pageSize: 200 }).subscribe({
          next: (data) => this.supervisors = data.data,
        });
      }
    }
  }

  onLoginMethodModalVisibleChange(visible: boolean): void {
    // Solo cerrar si no estamos transitando hacia el modal de confirmación
    if (!visible && !this.showLoginMethodConfirm) {
      this.closeLoginMethodModal();
    }
  }

  closeLoginMethodModal(): void {
    this.showLoginMethodModal = false;
    this.showLoginMethodConfirm = false;
    this.selectedPerson = null;
  }

  onLoginMethodSubmit(): void {
    this.loginMethodSubmitted = true;
    this.loginMethodError = '';
    this.loginMethodSuccess = '';

    if (this.loginMethodForm.invalid || !this.selectedPerson) return;

    // Validacion extra: supervisor requerido para metodo Asistido
    if (this.selectedLoginMethod?.requiresSupervisor && !this.loginMethodForm.value.supervisorUserId) {
      return;
    }

    const raw = this.loginMethodForm.value;
    const request: UpdateLoginMethodRequest = {
      loginMethodId: +raw.loginMethodId,
      ...(raw.pin && { pin: raw.pin }),
      ...(raw.supervisorUserId && { supervisorUserId: raw.supervisorUserId }),
    };

    this.#authService.updateUserLoginMethod(this.selectedPerson.userId, request).subscribe({
      next: (response) => {
        this.loginMethodSuccess = 'Método de login actualizado correctamente.';
        this.temporaryPassword = response.data?.temporaryPassword ?? '';
        this.showLoginMethodModal = true; // Reabrir para mostrar el resultado
        this.loadPersons();
      },
      error: (err) => {
        this.loginMethodError = err?.userMessage || 'Error al actualizar el método de login.';
        this.showLoginMethodModal = true; // Reabrir para mostrar el error
      },
    });
  }

  copyTemporaryPassword(): void {
    if (!this.temporaryPassword) return;
    navigator.clipboard.writeText(this.temporaryPassword).then(() => {
      this.#toastService.success('Contraseña copiada al portapapeles');
    });
  }

  loadPersons(search?: string): void {
    this.isLoading = true;
    const isActive = this.statusFilter === 'true' ? true
                   : this.statusFilter === 'false' ? false
                   : undefined;

    this.#personsService
      .getPersons({
        page: this.currentPage,
        pageSize: this.pageSize,
        search,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
        institutionId: this.selectedInstitutionId,
        isActive,
        representativeSearch: this.representativeSearch || undefined,
      })
      .subscribe({
        next: (response) => {
          this.persons = response.data;
          this.totalItems = response.totalRecords;
          this.isLoading = false;
        },
        error: () => {
          this.#toastService.error('Error al obtener personas');
          this.isLoading = false;
        },
      });
  }
}
