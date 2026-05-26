import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InvitationsService, ToastService } from '@services';
import { InvitationResponse } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ButtonDirective, FormLabelDirective, FormSelectDirective, GridModule } from '@coreui/angular';

@Component({
  selector: 'app-admin-invitations',
  standalone: true,
  imports: [FormsModule, DataTableComponent, ButtonDirective, FormLabelDirective, FormSelectDirective, GridModule],
  templateUrl: './invitations.component.html',
  styleUrl: './invitations.component.scss',
})
export class InvitationsComponent implements OnInit {
  private readonly invitationsService = inject(InvitationsService);
  private readonly toastService = inject(ToastService);

  invitations: InvitationResponse[] = [];
  totalItems = 0;
  pageSize = 10;
  currentPage = 1;
  isLoading = false;

  searchTerm  = '';
  statusFilter = '';

  public cols: TableColumn[] = [
    { key: 'email',                     label: 'Email',        sortable: true },
    { key: 'firstName',                 label: 'Nombre',       sortable: true },
    { key: 'relationship',              label: 'Parentesco' },
    { key: 'personName',                label: 'Persona' },
    { key: 'createdByProfessionalName', label: 'Profesional' },
    {
      key: 'status', label: 'Estado', type: 'badge', sortable: true,
      badgeMap: {
        'Enviada':  { color: 'info',    label: 'Enviada'  },
        'Aceptada': { color: 'success', label: 'Aceptada' },
        'Expirada': { color: 'danger',  label: 'Expirada' },
      },
    },
    { key: 'createdAt',                 label: 'Fecha',        type: 'date',  sortable: true },
  ];

  ngOnInit(): void {
    this.load();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.load();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.load();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.load();
  }

  clearFilters(): void {
    this.statusFilter = '';
    this.searchTerm   = '';
    this.currentPage  = 1;
    this.load();
  }

  private load(): void {
    this.isLoading = true;
    this.invitationsService.getAll(
      this.currentPage,
      this.pageSize,
      this.searchTerm  || undefined,
      this.statusFilter || undefined,
    ).subscribe({
      next: (response) => {
        this.invitations = response.data;
        this.totalItems  = response.totalRecords;
        this.isLoading   = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar las invitaciones');
      },
    });
  }
}
