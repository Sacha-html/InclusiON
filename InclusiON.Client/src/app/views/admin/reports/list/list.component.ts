import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogsService, ReportsService, ToastService } from '@services';
import { PersonsService } from '@services/persons.service';
import { ProfessionalsService } from '@services/professionals.service';
import { InstitutionsService } from '@services/institutions.service';
import { AppRoutes } from '@shared/constants/app-routes';
import { ReportListItemResponse, ReportStatus, GetReportsRequest, CatalogItem, PersonListItemResponse, ProfessionalListItemResponse, InstitutionResponse } from '@models';
import { ReportStatus as ReportStatusLabels } from '@shared/constants/status-labels';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import {
  ButtonDirective,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  GridModule,
  ModalModule,
  SpinnerComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-reports-list',
  standalone: true,
  imports: [
    FormsModule,
    DataTableComponent,
    ConfirmModalComponent,
    ButtonDirective,
    FormControlDirective,
    FormLabelDirective,
    FormSelectDirective,
    GridModule,
    ModalModule,
    SpinnerComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly reportsService       = inject(ReportsService);
  private readonly toastService         = inject(ToastService);
  private readonly catalogsService      = inject(CatalogsService);
  private readonly personsService       = inject(PersonsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly institutionsService  = inject(InstitutionsService);
  private readonly router               = inject(Router);

  reports       = signal<ReportListItemResponse[]>([]);
  persons       = signal<PersonListItemResponse[]>([]);
  professionals = signal<ProfessionalListItemResponse[]>([]);
  institutions  = signal<InstitutionResponse[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalRecords = signal(0);

  sortBy = 'createdAt';
  sortDirection = 'DESC';

  // Filtros
  statusFilter = 'Submitted'; // Por defecto: pendientes de revisión
  typeFilter = '';
  dateFrom = '';
  dateTo = '';
  searchTerm = '';
  selectedPersonIds: string[] = [];
  filterProfessionalId = '';
  filterInstitutionId  = '';

  onlyDeactivatedProfessionals = false;

  // Modales
  showApproveModal = false;
  showRejectModal = false;
  showReassignModal = false;
  showDeleteModal = false;
  selectedProfessionalId = '';
  selectedReport: ReportListItemResponse | null = null;
  isActioning = false;

  reportTypes = signal<CatalogItem[]>([]);

  columns: TableColumn[] = [
    { key: 'reportDate', label: 'Fecha', type: 'date', sortable: true },
    { key: 'title', label: 'Título', sortable: true },
    { key: 'personName', label: 'Persona', sortable: true },
    { key: 'professionalName', label: 'Profesional', sortable: true },
    { key: 'reportTypeName', label: 'Tipo', sortable: true },
    {
      key: 'status',
      label: 'Estado',
      type: 'badge',
      sortable: true,
      badgeMap: {
        [ReportStatus.Draft]:     { color: 'secondary', label: ReportStatusLabels.Borrador },
        [ReportStatus.Submitted]: { color: 'warning',   label: ReportStatusLabels.Enviado },
        [ReportStatus.Approved]:  { color: 'success',   label: ReportStatusLabels.Aprobado },
        [ReportStatus.Rejected]:  { color: 'danger',    label: ReportStatusLabels.Rechazado },
      },
    },
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'view', label: 'Ver', icon: 'cilSearch' },
        {
          action: 'approve',
          label: 'Aprobar',
          icon: 'cilCheckCircle',
          visible: (item: ReportListItemResponse) => !this.onlyDeactivatedProfessionals && item.status === ReportStatus.Submitted,
        },
        {
          action: 'reject',
          label: 'Rechazar',
          icon: 'cilXCircle',
          visible: (item: ReportListItemResponse) => !this.onlyDeactivatedProfessionals && item.status === ReportStatus.Submitted,
        },
        {
          action: 'reassign',
          label: 'Reasignar',
          icon: 'cilShareAlt',
          visible: (item: ReportListItemResponse) => this.onlyDeactivatedProfessionals,
        },
        {
          action: 'delete',
          label: 'Eliminar',
          icon: 'cilTrash',
          visible: (item: ReportListItemResponse) => this.onlyDeactivatedProfessionals,
        },
      ],
    },
  ];

  ngOnInit(): void {
    this.loadReports();
    this.catalogsService.getReportTypes().subscribe(types => this.reportTypes.set(types));
    this.personsService.getPersons({ pageSize: 500 }).subscribe(r => this.persons.set(r.data));
    this.professionalsService.getProfessionals({ pageSize: 500, status: 'active' }).subscribe(r => this.professionals.set(r.data));
    this.institutionsService.getAll().subscribe(list => this.institutions.set(list));
  }

  loadReports(): void {
    this.isLoading.set(true);
    const request: GetReportsRequest = {
      page: this.currentPage(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy,
      sortDirection: this.sortDirection,
      status: this.onlyDeactivatedProfessionals ? undefined : (this.statusFilter || undefined),
      onlyDeactivatedProfessionals: this.onlyDeactivatedProfessionals || undefined,
      reportTypeId: this.typeFilter ? +this.typeFilter : undefined,
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined,
      search: this.searchTerm || undefined,
      personIds:       this.selectedPersonIds.length ? this.selectedPersonIds : undefined,
      professionalId:  this.filterProfessionalId  || undefined,
      institutionId:   this.filterInstitutionId   ? +this.filterInstitutionId : undefined,
    };

    this.reportsService.getReports(request).subscribe({
      next: (response) => {
        this.reports.set(response.data);
        this.totalRecords.set(response.totalRecords);
        this.isLoading.set(false);
      },
      error: () => { this.isLoading.set(false); this.toastService.error('Error al cargar los informes'); },
    });
  }

  onPersonFilterChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedPersonIds = Array.from(select.selectedOptions).map(o => o.value);
    this.currentPage.set(1);
    this.loadReports();
  }
  onStatusFilterChange(): void { this.currentPage.set(1); this.loadReports(); }
  onTypeFilterChange(): void   { this.currentPage.set(1); this.loadReports(); }
  onDateChange(): void         { this.currentPage.set(1); this.loadReports(); }

  onFilterChange(): void { this.currentPage.set(1); this.loadReports(); }

  onOrphanedReportsChange(): void {
    if (this.onlyDeactivatedProfessionals) {
      this.statusFilter = ''; // Desactivar filtro de estado para ver todos los reportes huérfanos
    } else {
      this.statusFilter = 'Submitted'; // Volver al filtro por defecto
    }
    this.currentPage.set(1);
    this.loadReports();
  }

  clearFilters(): void {
    this.statusFilter        = 'Submitted';
    this.typeFilter          = '';
    this.dateFrom            = '';
    this.dateTo              = '';
    this.selectedPersonIds   = [];
    this.filterProfessionalId = '';
    this.filterInstitutionId  = '';
    this.onlyDeactivatedProfessionals = false;
    this.currentPage.set(1);
    this.loadReports();
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage.set(1);
    this.loadReports();
  }

  onSort(event: { sortBy: string; sortDirection: string }): void {
    this.sortBy = event.sortBy;
    this.sortDirection = event.sortDirection;
    this.loadReports();
  }

  onRowAction(event: { action: string; item: ReportListItemResponse }): void {
    this.selectedReport = event.item;
    if (event.action === 'view') {
      this.router.navigate([AppRoutes.Admin.Reports, event.item.encryptedId]);
    } else if (event.action === 'approve') {
      this.showApproveModal = true;
    } else if (event.action === 'reject') {
      this.showRejectModal = true;
    } else if (event.action === 'reassign') {
      this.selectedProfessionalId = '';
      this.showReassignModal = true;
    } else if (event.action === 'delete') {
      this.showDeleteModal = true;
    }
  }

  confirmApprove(): void {
    if (!this.selectedReport) return;
    this.isActioning = true;
    this.reportsService.approveReport(this.selectedReport.encryptedId).subscribe({
      next: () => {
        this.toastService.success('Reporte aprobado. El familiar ya puede consultarlo.');
        this.showApproveModal = false;
        this.selectedReport = null;
        this.isActioning = false;
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al aprobar el reporte.');
        this.isActioning = false;
      },
    });
  }

  confirmReject(comment: string): void {
    if (!this.selectedReport) return;
    this.isActioning = true;
    this.reportsService.rejectReport(this.selectedReport.encryptedId, comment).subscribe({
      next: () => {
        this.toastService.success('Reporte rechazado. El profesional fue notificado.');
        this.showRejectModal = false;
        this.selectedReport = null;
        this.isActioning = false;
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al rechazar el reporte.');
        this.isActioning = false;
      },
    });
  }

  confirmReassign(): void {
    if (!this.selectedReport || !this.selectedProfessionalId) return;
    this.isActioning = true;
    this.reportsService.reassignReport(this.selectedReport.encryptedId, this.selectedProfessionalId).subscribe({
      next: () => {
        this.toastService.success('Reporte reasignado exitosamente.');
        this.showReassignModal = false;
        this.selectedReport = null;
        this.selectedProfessionalId = '';
        this.isActioning = false;
        this.loadReports();
      },
      error: (err) => {
        this.toastService.error(err?.userMessage ?? 'Error al reasignar el reporte.');
        this.isActioning = false;
      }
    });
  }

  confirmDelete(): void {
    if (!this.selectedReport) return;
    this.isActioning = true;
    this.reportsService.deleteReport(this.selectedReport.encryptedId).subscribe({
      next: () => {
        this.toastService.success('Reporte eliminado exitosamente.');
        this.showDeleteModal = false;
        this.selectedReport = null;
        this.isActioning = false;
        this.loadReports();
      },
      error: (err) => {
        this.toastService.error(err?.userMessage ?? 'Error al eliminar el reporte.');
        this.isActioning = false;
      }
    });
  }

  cancelAction(): void {
    this.showApproveModal = false;
    this.showRejectModal = false;
    this.showReassignModal = false;
    this.showDeleteModal = false;
    this.selectedReport = null;
  }
}
