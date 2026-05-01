import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ActivitiesService } from '@services/activities.service';
import { ToastService } from '@services';
import { CatalogsService } from '@services/catalogs.service';
import { ActivityListItemResponse } from '@models/responses/activity.response';
import { GetActivitiesRequest } from '@models/requests/activities';
import { ActivityCategoryItem, ActivityTemplateTypeItem } from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { AssignActivityModalComponent } from '../assign-modal/assign-activity-modal.component';
import {
  ButtonDirective,
  FormSelectDirective,
  GridModule,
} from '@coreui/angular';

@Component({
  selector: 'app-activities-list',
  standalone: true,
  imports: [
    RouterModule,
    FormsModule,
    DataTableComponent,
    ConfirmModalComponent,
    AssignActivityModalComponent,
    ButtonDirective,
    FormSelectDirective,
    GridModule,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly catalogsService   = inject(CatalogsService);
  private readonly toastService      = inject(ToastService);
  private readonly router            = inject(Router);

  activities    = signal<ActivityListItemResponse[]>([]);
  categories    = signal<ActivityCategoryItem[]>([]);
  templateTypes = signal<ActivityTemplateTypeItem[]>([]);
  isLoading     = signal(true);
  currentPage   = signal(1);
  pageSize      = signal(10);
  totalRecords  = signal(0);
  totalPages    = signal(0);
  searchTerm    = signal('');

  categoryFilter     = '';
  templateTypeFilter = '';
  statusFilter       = '';
  standardFilter     = '';

  // Desactivar
  showDeactivateModal   = false;
  itemToDeactivate: ActivityListItemResponse | null = null;

  // Asignar
  showAssignModal   = false;
  itemToAssign: ActivityListItemResponse | null = null;

  columns: TableColumn[] = [
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'assign',     label: 'Asignar',     icon: 'cil-send',  visible: (item) => item.isActive },
        { action: 'edit',       label: 'Editar',      icon: 'cil-notes', visible: (item) => !item.isStandardActivity },
        { action: 'deactivate', label: 'Desactivar',  icon: 'cil-ban',   visible: (item) => item.isActive },
        { action: 'activate',   label: 'Activar',     icon: 'cil-check', visible: (item) => !item.isActive },
      ],
    },
    { key: 'title',            label: 'Título',       sortable: true },
    { key: 'templateTypeName', label: 'Tipo',         sortable: true },
    { key: 'categoryName',     label: 'Categoría',    sortable: true },
    { key: 'complexityLevel',  label: 'Complejidad',  sortable: true },
    { key: 'estimatedDurationMinutes', label: 'Duración (min)', sortable: true },
    {
      key: 'isActive',
      label: 'Estado',
      type: 'badge',
      sortable: true,
      badgeMap: {
        true:  { color: 'success',   label: 'Activa'   },
        false: { color: 'secondary', label: 'Inactiva' },
      },
    },
    {
      key: 'isStandardActivity',
      label: 'Origen',
      type: 'badge',
      badgeMap: {
        true:  { color: 'info',    label: 'Estándar' },
        false: { color: 'primary', label: 'Propia'   },
      },
    },
  ];

  headerButtons = [
    { action: 'create', label: 'Nueva actividad', icon: 'cilPlus', routerLink: '/pro/activities/new' },
  ];

  ngOnInit(): void {
    this.loadCatalogs();
    this.loadActivities();
  }

  private loadCatalogs(): void {
    this.catalogsService.getActivityCategories().subscribe({
      next: (cats) => this.categories.set(cats),
    });
    this.catalogsService.getActivityTemplateTypes().subscribe({
      next: (types) => this.templateTypes.set(types),
    });
  }

  loadActivities(): void {
    this.isLoading.set(true);
    const request: GetActivitiesRequest = {
      page:           this.currentPage(),
      pageSize:       this.pageSize(),
      search:         this.searchTerm() || undefined,
      categoryId:     this.categoryFilter     ? +this.categoryFilter     : undefined,
      templateTypeId: this.templateTypeFilter ? +this.templateTypeFilter : undefined,
      isActive:       this.statusFilter  !== '' ? this.statusFilter  === 'true' : undefined,
      isStandard:     this.standardFilter !== '' ? this.standardFilter === 'true' : undefined,
      sortBy:         'title',
      sortDirection:  'asc',
    };

    this.activitiesService.getActivities(request).subscribe({
      next: (response) => {
        this.activities.set(response.data);
        this.totalRecords.set(response.totalRecords);
        this.totalPages.set(response.totalPages);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onSearch(term: string): void     { this.searchTerm.set(term); this.currentPage.set(1); this.loadActivities(); }
  onPageChange(page: number): void { this.currentPage.set(page); this.loadActivities(); }
  onSort(_: unknown): void         { this.loadActivities(); }
  onFilterChange(): void           { this.currentPage.set(1); this.loadActivities(); }

  clearFilters(): void {
    this.categoryFilter = '';
    this.templateTypeFilter = '';
    this.statusFilter = '';
    this.standardFilter = '';
    this.currentPage.set(1);
    this.loadActivities();
  }

  onHeaderAction(action: string): void {
    if (action === 'create') this.router.navigate(['/pro/activities/new']);
  }

  onRowAction(event: { action: string; item: ActivityListItemResponse }): void {
    switch (event.action) {
      case 'assign':
        this.itemToAssign = event.item;
        this.showAssignModal = true;
        break;
      case 'edit':
        this.router.navigate(['/pro/activities', event.item.id, 'edit']);
        break;
      case 'deactivate':
        this.itemToDeactivate = event.item;
        this.showDeactivateModal = true;
        break;
      case 'activate':
        this.toggleStatus(event.item, true);
        break;
    }
  }

  // ── Desactivar ────────────────────────────────────────────────────────────
  confirmDeactivate(): void {
    if (!this.itemToDeactivate) return;
    this.toggleStatus(this.itemToDeactivate, false);
    this.showDeactivateModal = false;
  }

  cancelDeactivate(): void {
    this.showDeactivateModal = false;
    this.itemToDeactivate = null;
  }

  private toggleStatus(item: ActivityListItemResponse, isActive: boolean): void {
    this.activitiesService.setStatus(item.id, isActive).subscribe({
      next: () => {
        this.toastService.success(isActive ? 'Actividad activada.' : 'Actividad desactivada.');
        this.loadActivities();
      },
      error: () => {
        this.toastService.error('No se pudo cambiar el estado. Verificá si tiene asignaciones activas.');
      },
    });
  }

  // ── Asignar ───────────────────────────────────────────────────────────────
  onAssigned(): void {
    // Opcional: podría recargar la lista. Por ahora solo cierra el modal.
  }
}
