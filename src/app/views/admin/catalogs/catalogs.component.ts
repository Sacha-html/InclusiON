import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CatalogsService, CatalogAdminService, ToastService } from '@services';
import {
  CatalogItem,
  AutonomyLevelItem,
  LoginMethodItem,
  ActivityCategoryItem,
  SkillAreaItem,
  ActivityTemplateTypeItem,
} from '@models';

import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  TableDirective, ButtonDirective, BadgeComponent, SpinnerComponent,
  ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
  FormControlDirective, FormLabelDirective, FormSelectDirective,
  FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  RowComponent, ColComponent,
} from '@coreui/angular';

type CatalogType = 'disability-types' | 'autonomy-levels' | 'activity-categories' | 'skill-areas' | 'template-types' | 'login-methods';

@Component({
  selector: 'app-catalogs',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    TableDirective, ButtonDirective, BadgeComponent, SpinnerComponent,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    FormControlDirective, FormLabelDirective, FormSelectDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    RowComponent, ColComponent,
  ],
  templateUrl: './catalogs.component.html',
  styleUrl: './catalogs.component.scss',
})
export class CatalogsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogsService = inject(CatalogsService);
  private readonly adminService = inject(CatalogAdminService);
  private readonly toastService = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  catalogType: CatalogType = 'disability-types';

  disabilityTypes: CatalogItem[] = [];
  autonomyLevels: AutonomyLevelItem[] = [];
  activityCategories: ActivityCategoryItem[] = [];
  skillAreas: SkillAreaItem[] = [];
  templateTypes: ActivityTemplateTypeItem[] = [];
  loginMethods: LoginMethodItem[] = [];

  isLoading = true;
  isSaving = false;
  showModal = false;
  modalTitle = '';
  editingId: number | null = null;

  disabilityForm!: FormGroup;
  autonomyForm!: FormGroup;
  categoryForm!: FormGroup;
  skillAreaForm!: FormGroup;
  templateTypeForm!: FormGroup;
  loginMethodForm!: FormGroup;

  get pageTitle(): string {
    const titles: Record<CatalogType, string> = {
      'disability-types': 'Tipos de Discapacidad',
      'autonomy-levels': 'Niveles de Autonomia',
      'activity-categories': 'Categorias de Actividad',
      'skill-areas': 'Areas de Habilidad',
      'template-types': 'Tipos de Template',
      'login-methods': 'Metodos de Login',
    };
    return titles[this.catalogType] || 'Catalogos';
  }

  get canCreate(): boolean {
    return this.catalogType !== 'login-methods';
  }

  ngOnInit(): void {
    this.initForms();
    this.route.paramMap.subscribe(params => {
      const type = params.get('type') as CatalogType;
      if (type) this.catalogType = type;
      this.loadData();
    });
  }

  private initForms(): void {
    this.disabilityForm = this.fb.group({ name: ['', Validators.required], description: [''], isActive: [true] });
    this.autonomyForm = this.fb.group({ name: ['', Validators.required], description: [''], requiresSupervision: [false], displayOrder: [0], isActive: [true] });
    this.categoryForm = this.fb.group({ name: ['', Validators.required], description: [''], isActive: [true] });
    this.skillAreaForm = this.fb.group({ name: ['', Validators.required], description: [''], icon: [''], color: ['#000000'], displayOrder: [0] });
    this.templateTypeForm = this.fb.group({ name: ['', Validators.required], code: ['', Validators.required], skillAreaId: [null], supportsPictograms: [false], supportsAudio: [false] });
    this.loginMethodForm = this.fb.group({ name: ['', Validators.required], description: [''], displayOrder: [0] });
  }

  private loadData(): void {
    this.isLoading = true;
    switch (this.catalogType) {
      case 'disability-types':
        this.catalogsService.getDisabilityTypes().subscribe({ next: d => { this.disabilityTypes = d; this.isLoading = false; }, error: () => this.isLoading = false });
        break;
      case 'autonomy-levels':
        this.catalogsService.getAutonomyLevels().subscribe({ next: d => { this.autonomyLevels = d; this.isLoading = false; }, error: () => this.isLoading = false });
        break;
      case 'activity-categories':
        this.catalogsService.getActivityCategories().subscribe({ next: d => { this.activityCategories = d; this.isLoading = false; }, error: () => this.isLoading = false });
        break;
      case 'skill-areas':
        this.catalogsService.getSkillAreas().subscribe({ next: d => { this.skillAreas = d; this.isLoading = false; }, error: () => this.isLoading = false });
        break;
      case 'template-types':
        this.catalogsService.getSkillAreas().subscribe({ next: areas => {
          this.skillAreas = areas;
          this.catalogsService.getActivityTemplateTypes().subscribe({ next: d => { this.templateTypes = d; this.isLoading = false; }, error: () => this.isLoading = false });
        }, error: () => this.isLoading = false });
        break;
      case 'login-methods':
        this.catalogsService.getLoginMethods().subscribe({ next: d => { this.loginMethods = d; this.isLoading = false; }, error: () => this.isLoading = false });
        break;
    }
  }

  openNew(): void {
    this.editingId = null;
    this.modalTitle = `Nuevo - ${this.pageTitle}`;
    const resets: Record<string, () => void> = {
      'disability-types': () => this.disabilityForm.reset({ name: '', description: '', isActive: true }),
      'autonomy-levels': () => this.autonomyForm.reset({ name: '', description: '', requiresSupervision: false, displayOrder: 0, isActive: true }),
      'activity-categories': () => this.categoryForm.reset({ name: '', description: '', isActive: true }),
      'skill-areas': () => this.skillAreaForm.reset({ name: '', description: '', icon: '', color: '#000000', displayOrder: 0 }),
      'template-types': () => this.templateTypeForm.reset({ name: '', code: '', skillAreaId: null, supportsPictograms: false, supportsAudio: false }),
    };
    resets[this.catalogType]?.();
    this.showModal = true;
  }

  openEdit(item: any): void {
    this.editingId = item.id;
    this.modalTitle = `Editar - ${this.pageTitle}`;
    switch (this.catalogType) {
      case 'disability-types': this.disabilityForm.patchValue({ name: item.name, description: item.description || '', isActive: true }); break;
      case 'autonomy-levels': this.autonomyForm.patchValue({ name: item.name, description: item.description || '', requiresSupervision: item.requiresSupervision, displayOrder: item.displayOrder, isActive: true }); break;
      case 'activity-categories': this.categoryForm.patchValue({ name: item.name, description: item.description || '', isActive: item.isActive }); break;
      case 'skill-areas': this.skillAreaForm.patchValue({ name: item.name, description: item.description || '', icon: item.icon || '', color: item.color || '#000000', displayOrder: item.displayOrder }); break;
      case 'template-types': this.templateTypeForm.patchValue({ name: item.name, code: item.code, skillAreaId: item.skillAreaId, supportsPictograms: item.supportsPictograms, supportsAudio: item.supportsAudio }); break;
      case 'login-methods': this.loginMethodForm.patchValue({ name: item.name, description: item.description || '', displayOrder: item.displayOrder }); break;
    }
    this.showModal = true;
  }

  closeModal(): void { this.showModal = false; this.editingId = null; }

  getActiveForm(): FormGroup {
    const forms: Record<string, FormGroup> = {
      'disability-types': this.disabilityForm, 'autonomy-levels': this.autonomyForm,
      'activity-categories': this.categoryForm, 'skill-areas': this.skillAreaForm,
      'template-types': this.templateTypeForm, 'login-methods': this.loginMethodForm,
    };
    return forms[this.catalogType] || this.disabilityForm;
  }

  save(): void {
    const form = this.getActiveForm();
    if (form.invalid) return;
    this.isSaving = true;
    const v = form.value;
    let obs;
    switch (this.catalogType) {
      case 'disability-types': obs = this.editingId ? this.adminService.updateDisabilityType(this.editingId, v) : this.adminService.createDisabilityType(v); break;
      case 'autonomy-levels': obs = this.editingId ? this.adminService.updateAutonomyLevel(this.editingId, v) : this.adminService.createAutonomyLevel(v); break;
      case 'activity-categories': obs = this.editingId ? this.adminService.updateActivityCategory(this.editingId, v) : this.adminService.createActivityCategory(v); break;
      case 'skill-areas': obs = this.editingId ? this.adminService.updateSkillArea(this.editingId, v) : this.adminService.createSkillArea(v); break;
      case 'template-types': obs = this.editingId ? this.adminService.updateActivityTemplateType(this.editingId, v) : this.adminService.createActivityTemplateType(v); break;
      case 'login-methods': obs = this.editingId ? this.adminService.updateLoginMethod(this.editingId, v) : null; break;
      default: obs = null;
    }
    if (!obs) { this.isSaving = false; return; }
    obs.subscribe({
      next: () => { this.isSaving = false; this.toastService.success(this.editingId ? 'Actualizado' : 'Creado'); this.closeModal(); this.catalogsService.clearCache(); this.loadData(); },
      error: () => { this.isSaving = false; this.toastService.error('Error al guardar'); },
    });
  }
}
