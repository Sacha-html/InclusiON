import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CatalogsService, CatalogAdminService, ToastService } from '@services';
import { ActiveStatus } from '@shared/constants/status-labels';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn, ActionItem, HeaderButton } from '@shared/components/data-table/data-table.models';
import { Observable } from 'rxjs';

import {
  ButtonDirective, SpinnerComponent,
  ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
  FormControlDirective, FormLabelDirective, FormSelectDirective,
  FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
} from '@coreui/angular';

type CatalogType = 'disability-types' | 'autonomy-levels' | 'activity-categories' | 'skill-areas' | 'template-types' | 'login-methods';

interface FieldConfig {
  key: string;
  label: string;
  type: 'text' | 'number' | 'checkbox' | 'color' | 'select';
  required?: boolean;
  default?: any;
  editOnly?: boolean;
  options?: () => { id: number; name: string }[];
}

interface CatalogConfig {
  title: string;
  canCreate: boolean;
  columns: TableColumn[];
  fields: FieldConfig[];
  load: () => Observable<any[]>;
  create?: (v: any) => Observable<any>;
  update: (id: number, v: any) => Observable<any>;
  deactivate?: (id: number) => Observable<any>;
}

@Component({
  selector: 'app-catalogs',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DataTableComponent,
    ButtonDirective, SpinnerComponent,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    FormControlDirective, FormLabelDirective, FormSelectDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  ],
  templateUrl: './catalogs.component.html',
  styleUrl: './catalogs.component.scss',
})
export class CatalogsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly catalogsService = inject(CatalogsService);
  private readonly adminService = inject(CatalogAdminService);
  private readonly toastService = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  catalogType: CatalogType = 'disability-types';
  items: any[] = [];
  isLoading = true;
  isSaving = false;
  showModal = false;
  modalTitle = '';
  editingId: number | null = null;
  form: FormGroup | null = null;

  showDeactivateModal = false;
  deactivatingItem: any | null = null;
  isDeactivating = false;

  private skillAreasCache: { id: number; name: string }[] = [];

  readonly headerButtons: HeaderButton[] = [
    { action: 'create', label: 'Agregar', color: 'primary' },
  ];

  get tableColumns(): TableColumn[] {
    const actions: ActionItem[] = [
      { action: 'edit', label: 'Editar', icon: 'cil-pencil' },
    ];
    if (this.config.deactivate) {
      actions.push({
        action: 'deactivate',
        label: 'Dar de baja',
        icon: 'cil-x-circle',
        visible: (item: any) => item.isActive !== false,
      });
    }
    return [
      ...this.config.columns,
      { key: '', label: 'Acciones', type: 'actions', actions },
    ];
  }

  onHeaderAction(action: string): void {
    if (action === 'create') this.openNew();
  }

  onRowAction(event: { action: string; item: any }): void {
    if (event.action === 'edit') this.openEdit(event.item);
    if (event.action === 'deactivate') this.openDeactivate(event.item);
  }

  private configs: Record<CatalogType, CatalogConfig> = {
    'disability-types': {
      title: 'Tipos de Discapacidad',
      canCreate: true,
      columns: [
        { key: 'name',        label: 'Nombre' },
        { key: 'description', label: 'Descripcion' },
        { key: 'isActive',    label: 'Estado', type: 'badge', badgeMap: {
          'true':  { color: 'success', label: ActiveStatus.Activo   },
          'false': { color: 'danger',  label: ActiveStatus.Inactivo },
        }},
      ],
      fields: [
        { key: 'name', label: 'Nombre', type: 'text', required: true },
        { key: 'description', label: 'Descripcion', type: 'text' },
        { key: 'isActive', label: 'Activo', type: 'checkbox', default: true, editOnly: true },
      ],
      load: () => this.catalogsService.getDisabilityTypes(),
      create: (v) => this.adminService.createDisabilityType(v),
      update: (id, v) => this.adminService.updateDisabilityType(id, v),
      deactivate: (id) => this.adminService.patchDisabilityTypeStatus(id, false),
    },
    'autonomy-levels': {
      title: 'Niveles de Autonomia',
      canCreate: true,
      columns: [
        { key: 'name',                label: 'Nombre' },
        { key: 'description',         label: 'Descripcion' },
        { key: 'requiresSupervision', label: 'Supervision', type: 'badge', badgeMap: {
          'true':  { color: 'warning', label: 'Si' },
          'false': { color: 'success', label: 'No' },
        }},
        { key: 'displayOrder', label: 'Orden' },
        { key: 'isActive',     label: 'Estado', type: 'badge', badgeMap: {
          'true':  { color: 'success', label: ActiveStatus.Activo   },
          'false': { color: 'danger',  label: ActiveStatus.Inactivo },
        }},
      ],
      fields: [
        { key: 'name', label: 'Nombre', type: 'text', required: true },
        { key: 'description', label: 'Descripcion', type: 'text' },
        { key: 'requiresSupervision', label: 'Requiere Supervision', type: 'checkbox', default: false },
        { key: 'displayOrder', label: 'Orden', type: 'number', default: 0 },
      ],
      load: () => this.catalogsService.getAutonomyLevels(),
      create: (v) => this.adminService.createAutonomyLevel(v),
      update: (id, v) => this.adminService.updateAutonomyLevel(id, v),
      deactivate: (id) => this.adminService.patchAutonomyLevelStatus(id, false),
    },
    'activity-categories': {
      title: 'Categorias de Actividad',
      canCreate: true,
      columns: [
        { key: 'name',        label: 'Nombre' },
        { key: 'description', label: 'Descripcion' },
        { key: 'isActive',    label: 'Estado', type: 'badge', badgeMap: {
          'true':  { color: 'success', label: ActiveStatus.Activo   },
          'false': { color: 'danger',  label: ActiveStatus.Inactivo },
        }},
      ],
      fields: [
        { key: 'name', label: 'Nombre', type: 'text', required: true },
        { key: 'description', label: 'Descripcion', type: 'text' },
        { key: 'isActive', label: 'Activo', type: 'checkbox', default: true, editOnly: true },
      ],
      load: () => this.catalogsService.getActivityCategories(),
      create: (v) => this.adminService.createActivityCategory(v),
      update: (id, v) => this.adminService.updateActivityCategory(id, v),
      deactivate: (id) => this.adminService.patchActivityCategoryStatus(id, false),
    },
    'skill-areas': {
      title: 'Areas de Habilidad',
      canCreate: true,
      columns: [
        { key: 'name',         label: 'Nombre' },
        { key: 'icon',         label: 'Icono' },
        { key: 'color',        label: 'Color',  type: 'color' },
        { key: 'displayOrder', label: 'Orden' },
        { key: 'isActive',     label: 'Estado', type: 'badge', badgeMap: {
          'true':  { color: 'success', label: ActiveStatus.Activo   },
          'false': { color: 'danger',  label: ActiveStatus.Inactivo },
        }},
      ],
      fields: [
        { key: 'name', label: 'Nombre', type: 'text', required: true },
        { key: 'description', label: 'Descripcion', type: 'text' },
        { key: 'icon', label: 'Icono', type: 'text' },
        { key: 'color', label: 'Color', type: 'color', default: '#000000' },
        { key: 'displayOrder', label: 'Orden', type: 'number', default: 0 },
      ],
      load: () => this.catalogsService.getSkillAreas(),
      create: (v) => this.adminService.createSkillArea(v),
      update: (id, v) => this.adminService.updateSkillArea(id, v),
      deactivate: (id) => this.adminService.patchSkillAreaStatus(id, false),
    },
    'template-types': {
      title: 'Tipos de Template',
      canCreate: true,
      columns: [
        { key: 'name',               label: 'Nombre' },
        { key: 'code',               label: 'Codigo',      type: 'code' },
        { key: 'skillAreaName',      label: 'Area' },
        { key: 'supportsPictograms', label: 'Pictogramas', type: 'badge', badgeMap: {
          'true':  { color: 'success',   label: 'Si' },
          'false': { color: 'secondary', label: 'No' },
        }},
        { key: 'supportsAudio',      label: 'Audio',       type: 'badge', badgeMap: {
          'true':  { color: 'success',   label: 'Si' },
          'false': { color: 'secondary', label: 'No' },
        }},
        { key: 'isActive',           label: 'Estado',      type: 'badge', badgeMap: {
          'true':  { color: 'success', label: ActiveStatus.Activo   },
          'false': { color: 'danger',  label: ActiveStatus.Inactivo },
        }},
      ],
      fields: [
        { key: 'name', label: 'Nombre', type: 'text', required: true },
        { key: 'code', label: 'Codigo', type: 'text', required: true },
        { key: 'skillAreaId', label: 'Area de Habilidad', type: 'select', options: () => this.skillAreasCache },
        { key: 'supportsPictograms', label: 'Soporta Pictogramas', type: 'checkbox', default: false },
        { key: 'supportsAudio', label: 'Soporta Audio', type: 'checkbox', default: false },
      ],
      load: () => this.catalogsService.getActivityTemplateTypes(),
      create: (v) => this.adminService.createActivityTemplateType(v),
      update: (id, v) => this.adminService.updateActivityTemplateType(id, v),
      deactivate: (id) => this.adminService.patchActivityTemplateTypeStatus(id, false),
    },
    'login-methods': {
      title: 'Metodos de Login',
      canCreate: false,
      columns: [
        { key: 'name',        label: 'Nombre' },
        { key: 'code',        label: 'Codigo', type: 'code' },
        { key: 'description', label: 'Descripcion' },
      ],
      fields: [
        { key: 'name', label: 'Nombre', type: 'text', required: true },
        { key: 'description', label: 'Descripcion', type: 'text' },
        { key: 'displayOrder', label: 'Orden', type: 'number', default: 0 },
      ],
      load: () => this.catalogsService.getLoginMethods(),
      update: (id, v) => this.adminService.updateLoginMethod(id, v),
    },
  };

  get config(): CatalogConfig {
    return this.configs[this.catalogType];
  }

  get visibleFields(): FieldConfig[] {
    return this.config.fields.filter(f => !f.editOnly || this.editingId);
  }

  ngOnInit(): void {
    this.catalogsService.getSkillAreas().subscribe({
      next: (areas) => this.skillAreasCache = areas,
    });

    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const type = params.get('type') as CatalogType;
      if (type) this.catalogType = type;
      this.loadData();
    });
  }

  private loadData(): void {
    this.isLoading = true;
    this.config.load().subscribe({
      next: (data) => { this.items = data; this.isLoading = false; },
      error: () => { this.isLoading = false; this.toastService.error('Error al cargar el catálogo'); },
    });
  }

  private buildForm(values?: any): void {
    const group: any = {};
    for (const field of this.config.fields) {
      const value = values?.[field.key] ?? field.default ?? (field.type === 'checkbox' ? false : '');
      group[field.key] = field.required ? [value, Validators.required] : [value];
    }
    this.form = this.fb.group(group);
  }

  openNew(): void {
    this.editingId = null;
    this.modalTitle = `Nuevo - ${this.config.title}`;
    this.buildForm();
    this.showModal = true;
  }

  openEdit(item: any): void {
    this.editingId = item.id;
    this.modalTitle = `Editar - ${this.config.title}`;
    this.buildForm(item);
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingId = null;
  }

  save(): void {
    if (!this.form || this.form.invalid) return;
    this.isSaving = true;
    const value = this.form.value;
    const obs = this.editingId
      ? this.config.update(this.editingId, value)
      : this.config.create?.(value);

    if (!obs) { this.isSaving = false; return; }

    obs.subscribe({
      next: () => {
        this.isSaving = false;
        this.toastService.success(this.editingId ? 'Actualizado' : 'Creado');
        this.closeModal();
        this.catalogsService.clearCache();
        this.loadData();
      },
      error: () => {
        this.isSaving = false;
        this.toastService.error('Error al guardar');
      },
    });
  }

  openDeactivate(item: any): void {
    this.deactivatingItem = item;
    this.showDeactivateModal = true;
  }

  cancelDeactivate(): void {
    this.showDeactivateModal = false;
    this.deactivatingItem = null;
  }

  confirmDeactivate(): void {
    if (!this.deactivatingItem || !this.config.deactivate) return;
    this.isDeactivating = true;

    this.config.deactivate(this.deactivatingItem.id).subscribe({
      next: () => {
        this.isDeactivating = false;
        this.showDeactivateModal = false;
        this.deactivatingItem = null;
        this.toastService.success('Dado de baja exitosamente.');
        this.catalogsService.clearCache();
        this.loadData();
      },
      error: (err: any) => {
        this.isDeactivating = false;
        this.showDeactivateModal = false;
        this.deactivatingItem = null;
        const msg = err?.userMessage ?? 'Error al dar de baja.';
        this.toastService.error(msg);
      },
    });
  }
}
