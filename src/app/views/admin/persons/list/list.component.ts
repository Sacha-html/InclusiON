import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminInstitutionsService, AuthService, CatalogsService, PersonsService } from '@services';
import { AdminInstitutionResponse, LoginMethodItem, PersonListItemResponse, UpdateLoginMethodRequest } from '../../../../models';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';
import {
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-list',
  imports: [
    DataTableComponent,
    ReactiveFormsModule,
    FormsModule,
    FormControlDirective,
    FormFeedbackComponent,
    FormLabelDirective,
    FormSelectDirective,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly personsService = inject(PersonsService);
  private readonly adminInstitutionsService = inject(AdminInstitutionsService);
  private readonly authService = inject(AuthService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  canCreate = this.authService.hasPermission('persons:create');

  adminInstitutions: AdminInstitutionResponse[] = [];
  selectedInstitutionId: number | undefined;
  isGlobalAdmin = true;

  persons: PersonListItemResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;

  // Modal login method
  showLoginMethodModal = false;
  selectedPerson: PersonListItemResponse | null = null;
  loginMethods: LoginMethodItem[] = [];
  loginMethodForm: FormGroup = this.fb.group({
    loginMethodId: [null, Validators.required],
    pin: ['', [Validators.pattern(/^\d{4}$/)]],
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
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'view', label: 'Ver detalle' },
        { action: 'edit', label: 'Editar', visible: (item) => item.isActive },
        { action: 'login-method', label: 'Cambiar método de login', visible: (item) => item.isActive },
      ],
    },
    { key: 'fullName', label: 'Nombre completo' },
    { key: 'disabilityTypeName', label: 'Tipo de discapacidad' },
    { key: 'autonomyLevelName', label: 'Nivel de autonomía' },
    { key: 'age', label: 'Edad', type: 'number' },
    { key: 'isActive', label: 'Estado', type: 'badge' },
  ];

  ngOnInit(): void {
    this.catalogsService.getLoginMethods().subscribe({
      next: (data) => this.loginMethods = data,
    });
    this.adminInstitutionsService.getMyInstitutions().subscribe({
      next: (institutions) => {
        this.adminInstitutions = institutions;
        if (institutions.length > 0) {
          this.isGlobalAdmin = false;
          this.selectedInstitutionId = institutions[0].institutionId;
        }
        this.loadPersons();
      },
      error: () => {
        this.loadPersons();
      },
    });
  }

  onInstitutionFilterChange(): void {
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

  onHeaderAction(action: string): void {
    if (action === 'new') {
      this.router.navigate(['/admin/persons/new']);
    }
  }

  onRowAction(event: { action: string; item: any }): void {
    switch (event.action) {
      case 'view':
        this.router.navigate(['/admin/persons', event.item.id]);
        break;
      case 'edit':
        this.router.navigate(['/admin/persons', event.item.id, 'edit']);
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
  }

  closeLoginMethodModal(): void {
    this.showLoginMethodModal = false;
    this.selectedPerson = null;
  }

  onLoginMethodSubmit(): void {
    this.loginMethodSubmitted = true;
    this.loginMethodError = '';
    this.loginMethodSuccess = '';

    if (this.loginMethodForm.invalid || !this.selectedPerson) return;

    const raw = this.loginMethodForm.value;
    const request: UpdateLoginMethodRequest = {
      loginMethodId: +raw.loginMethodId,
      ...(raw.pin && { pin: raw.pin }),
    };

    this.authService.updateUserLoginMethod(this.selectedPerson.userId, request).subscribe({
      next: (response) => {
        this.loginMethodSuccess = 'Método de login actualizado correctamente.';
        this.temporaryPassword = response.data?.temporaryPassword ?? '';
        this.loadPersons();
      },
      error: (err) => {
        this.loginMethodError = err?.error?.message || 'Error al actualizar el método de login.';
      },
    });
  }

  private loadPersons(search?: string): void {
    this.personsService
      .getPersons({ page: this.currentPage, pageSize: this.pageSize, search, sortBy: 'lastName', sortDirection: 'ASC', institutionId: this.selectedInstitutionId })
      .subscribe({
        next: (response) => {
          this.persons = response.data.data;
          this.totalItems = response.data.totalRecords;
        },
        error: (error) => {
          console.error('Error al obtener personas:', error);
        },
      });
  }
}
