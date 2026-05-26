import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AssignmentsService, CatalogsService, ProfessionalsService, ReportsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ReportListItemResponse, ReportStatus, GetReportsRequest, CatalogItem, ProfessionalPersonResponse } from '@models';
import { ReportStatus as ReportStatusLabels } from '@shared/constants/status-labels';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import {
  ButtonDirective,
  FormLabelDirective,
  FormSelectDirective,
  FormControlDirective,
  GridModule,
} from '@coreui/angular';

@Component({
  selector: 'app-reports-list',
  imports: [
    RouterModule,
    FormsModule,
    DataTableComponent,
    ConfirmModalComponent,
    ButtonDirective,
    FormLabelDirective,
    FormSelectDirective,
    FormControlDirective,
    GridModule,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly reportsService = inject(ReportsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly toastService = inject(ToastService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  reports = signal<ReportListItemResponse[]>([]);
  persons = signal<ProfessionalPersonResponse[]>([]);
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  totalRecords = signal(0);
  totalPages = signal(0);
  searchTerm = signal('');
  sortBy = signal('createdAt');
  sortDirection = signal('DESC');

  // ID del profesional autenticado — se carga en ngOnInit antes de loadReports
  private professionalId = '';

  // Filtros
  selectedPersonIds: string[] = [];
  statusFilter = '';
  typeFilter = '';
  dateFrom = '';
  dateTo = '';

  // Modal de envío
  showSubmitModal = false;
  reportToSubmit: ReportListItemResponse | null = null;
  isSubmitting = false;

  reportTypes = signal<CatalogItem[]>([]);

  columns: TableColumn[] = [
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
actions: [
        { action: 'view', label: 'Ver', icon: 'cilSearch' },
        { action: 'edit', label: 'Editar', icon: 'cilNotes', visible: (item) => item.status === ReportStatus.Draft },
        { action: 'submit', label: 'Enviar', icon: 'cilSend', visible: (item) => item.status === ReportStatus.Draft },
      ],
    },
    { key: 'reportDate', label: 'Fecha', type: 'date', sortable: true },
    { key: 'title', label: 'Título', sortable: true },
    { key: 'personName', label: 'Persona', sortable: true },
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
  ];

  headerButtons = [
    { action: 'create', label: 'Agregar', icon: 'cilPlus', routerLink: AppRoutes.Pro.Reports + '/new' },
  ];

  ngOnInit(): void {
    // Aplicar filtros desde queryParams (ej: llegando desde el dashboard o detalle de persona)
    const statusParam = this.route.snapshot.queryParamMap.get('status');
    if (statusParam) this.statusFilter = statusParam;
    const personIdParam = this.route.snapshot.queryParamMap.get('personId');
    if (personIdParam) this.selectedPersonIds = [personIdParam];

    // Cargar perfil primero para filtrar reportes solo del profesional autenticado
    this.professionalsService.getMyProfile().subscribe({
      next: (profile) => {
        this.professionalId = profile.id;
        this.loadReports();
        this.catalogsService.getReportTypes().subscribe(types => this.reportTypes.set(types));
        this.assignmentsService.getPersonsByProfessional(profile.id).subscribe({
          next: (persons) => this.persons.set(persons.filter(p => p.isActive)),
        });
      },
    });
  }

  loadReports(): void {
    if (!this.professionalId) return;
    this.isLoading.set(true);

    const request: GetReportsRequest = {
      page: this.currentPage(),
      pageSize: this.pageSize(),
      search: this.searchTerm() || undefined,
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      professionalId: this.professionalId,
      personIds: this.selectedPersonIds.length ? this.selectedPersonIds : undefined,
      status: this.statusFilter || undefined,
      reportTypeId: this.typeFilter ? +this.typeFilter : undefined,
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined,
    };

    this.reportsService.getReports(request).subscribe({
      next: (response) => {
        this.reports.set(response.data);
        this.totalRecords.set(response.totalRecords);
        this.totalPages.set(response.totalPages);
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

  clearFilters(): void {
    this.selectedPersonIds = [];
    this.statusFilter = '';
    this.typeFilter = '';
    this.dateFrom = '';
    this.dateTo = '';
    this.currentPage.set(1);
    this.loadReports();
  }

  onSearch(term: string): void {
    this.searchTerm.set(term);
    this.currentPage.set(1);
    this.loadReports();
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  onSort(event: { sortBy: string; sortDirection: string }): void {
    this.sortBy.set(event.sortBy);
    this.sortDirection.set(event.sortDirection);
    this.loadReports();
  }

  onRowAction(event: { action: string; item: ReportListItemResponse }): void {
    if (event.action === 'view') {
      this.router.navigate([AppRoutes.Pro.Reports, event.item.encryptedId]);
    } else if (event.action === 'edit') {
      this.router.navigate([AppRoutes.Pro.Reports, event.item.encryptedId, 'edit']);
    } else if (event.action === 'submit') {
      this.reportToSubmit = event.item;
      this.showSubmitModal = true;
    }
  }

  onHeaderAction(action: string): void {
    if (action === 'create') {
      this.router.navigate([AppRoutes.Pro.Reports + '/new']);
    }
  }

  confirmSubmit(): void {
    if (!this.reportToSubmit) return;
    this.isSubmitting = true;
    this.reportsService.submitReport(this.reportToSubmit.encryptedId).subscribe({
      next: () => {
        this.toastService.success('Reporte enviado al administrador para revisión.');
        this.showSubmitModal = false;
        this.reportToSubmit = null;
        this.isSubmitting = false;
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al enviar el reporte.');
        this.isSubmitting = false;
      },
    });
  }

  cancelSubmit(): void {
    this.showSubmitModal = false;
    this.reportToSubmit = null;
  }
}
