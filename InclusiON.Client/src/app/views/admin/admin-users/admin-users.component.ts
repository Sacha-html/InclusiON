import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminUsersService } from '@services';
import { InstitutionsService } from '@services/institutions.service';
import { AppRoutes } from '@shared/constants/app-routes';
import { AuthService } from '@services/auth.service';
import { AdminUserResponse, InstitutionResponse } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from 'src/app/shared/components/data-table/data-table.models';
import {
  ColComponent,
  RowComponent,
  FormLabelDirective,
  FormSelectDirective,
  ButtonDirective,
  GridModule,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    FormsModule,
    DataTableComponent,
    ColComponent,
    RowComponent,
    FormLabelDirective,
    FormSelectDirective,
    ButtonDirective,
    GridModule,
  ],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
})
export class AdminUsersComponent implements OnInit {
  private readonly adminUsersService    = inject(AdminUsersService);
  private readonly institutionsService  = inject(InstitutionsService);
  private readonly authService          = inject(AuthService);
  private readonly router               = inject(Router);

  admins: AdminUserResponse[]         = [];
  institutions: InstitutionResponse[] = [];
  totalItems  = 0;
  pageSize    = 10;
  currentPage = 1;
  isLoading   = false;

  // Filtros
  filterRole          = '';
  filterIsActive      = '';
  filterInstitutionId = '';
  searchTerm          = '';

  get currentUserId(): string {
    return this.authService.getCurrentUser()?.id ?? '';
  }

  public cols: TableColumn[] = [
    {
      key: 'actions', label: 'Acciones', type: 'actions',
      actions: [
        { action: 'edit', label: 'Editar', icon: 'cilNotes', visible: (item: AdminUserResponse) => item.id === this.currentUserId },
      ],
    },
    { key: 'fullName',     label: 'Nombre',       sortable: true },
    { key: 'email',        label: 'Email',         sortable: true },
    { key: 'isGlobalAdmin', label: 'Tipo',         type: 'badge', badgeMap: {
        'true':  { color: 'primary', label: 'Global' },
        'false': { color: 'info',    label: 'Institucional' },
    }},
    { key: 'isActive',     label: 'Estado',        type: 'badge', sortable: true },
    { key: 'createdAt',    label: 'Fecha',          type: 'date',  sortable: true },
  ];

  ngOnInit(): void {
    this.loadAdmins();
    this.institutionsService.getAll().subscribe(list => this.institutions = list);
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadAdmins();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadAdmins();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadAdmins();
  }

  clearFilters(): void {
    this.filterRole          = '';
    this.filterIsActive      = '';
    this.filterInstitutionId = '';
    this.currentPage         = 1;
    this.loadAdmins();
  }

  onHeaderAction(action: string): void {
    if (action === 'new') this.router.navigate([AppRoutes.Admin.Admins + '/new']);
  }

  onRowAction(event: { action: string; item: AdminUserResponse }): void {
    if (event.action === 'edit') this.router.navigate([AppRoutes.Admin.Admins + '/edit']);
  }

  loadAdmins(): void {
    this.isLoading = true;
    const isActive      = this.filterIsActive      !== '' ? this.filterIsActive      === 'true' : undefined;
    const institutionId = this.filterInstitutionId !== '' ? +this.filterInstitutionId             : undefined;
    const role          = this.filterRole          !== '' ? this.filterRole                        : undefined;

    this.adminUsersService.getAdmins(
      this.currentPage, this.pageSize,
      this.searchTerm || undefined,
      role, isActive, institutionId,
    ).subscribe({
      next: (response) => {
        this.admins     = response.data;
        this.totalItems = response.totalRecords;
        this.isLoading  = false;
      },
      error: () => { this.isLoading = false; },
    });
  }
}
