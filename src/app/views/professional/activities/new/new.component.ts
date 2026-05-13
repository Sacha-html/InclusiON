import { afterNextRender, Component, inject, Injector, OnInit, signal, ViewChild, ViewContainerRef } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ActivitiesService } from '@services/activities.service';
import { CatalogsService } from '@services/catalogs.service';
import { ToastService } from '@services';
import { ActivityCategoryItem, ActivityTemplateTypeItem, SkillAreaItem } from '@models';
import { CreateActivityRequest, ActivityListItemResponse } from '@models';
import { CONTENT_EDITOR_REGISTRY } from './editors/content-editor-registry';
import { ContentEditorBaseComponent } from './editors/content-editor-base.component';
import { AssignActivityModalComponent } from '../assign-modal/assign-activity-modal.component';
import { AppRoutes } from '@shared/constants/app-routes';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ButtonDirective, ColComponent, RowComponent,
  FormControlDirective, FormSelectDirective, FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  SpinnerComponent, ProgressComponent, AlertComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-activities-new',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    ButtonDirective, ColComponent, RowComponent,
    FormControlDirective, FormSelectDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    SpinnerComponent, ProgressComponent, AlertComponent,
    AssignActivityModalComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly catalogsService   = inject(CatalogsService);
  private readonly toastService      = inject(ToastService);
  private readonly router            = inject(Router);
  private readonly injector          = inject(Injector);

  @ViewChild('editorHost', { read: ViewContainerRef }) private editorHost!: ViewContainerRef;

  // Wizard state
  currentStep = signal(1);
  totalSteps  = 3;

  // Catalogs
  categories    = signal<ActivityCategoryItem[]>([]);
  skillAreas    = signal<SkillAreaItem[]>([]);
  templateTypes = signal<ActivityTemplateTypeItem[]>([]);

  // Step 1 — metadata
  meta = {
    title: '',
    description: '',
    instructions: '',
    categoryId: 0,
    skillAreaId: 0,
    complexityLevel: 1,
    estimatedDurationMinutes: 15,
    templateTypeId: 0,
    requiresSupervision: false,
    hasVisualSupport: true,
    hasAudioSupport: false,
    usesEasyReading: false,
    usesPictograms: true,
    resourcesUrl: '',
  };

  // Step 2 — dynamic editor
  editorContentJson  = signal('{}');
  isEditorValid      = signal(false);
  editorUnavailable  = signal(false);

  isLoading = signal(false);
  savedActivity = signal<ActivityListItemResponse | null>(null);
  showAssignModal = false;

  get selectedTemplateName(): string {
    return this.templateTypes().find(t => t.id === +this.meta.templateTypeId)?.name ?? '';
  }

  get selectedTemplateCode(): string {
    return this.templateTypes().find(t => t.id === +this.meta.templateTypeId)?.code ?? '';
  }

  get step1Valid(): boolean {
    return !!this.meta.title.trim() && this.meta.categoryId > 0 && this.meta.templateTypeId > 0;
  }

  get step2Valid(): boolean {
    return true;
  }

  get step3Valid(): boolean {
    return this.isEditorValid();
  }

  ngOnInit(): void {
    this.catalogsService.getActivityCategories().subscribe({ next: c => this.categories.set(c) });
    this.catalogsService.getSkillAreas().subscribe({ next: s => this.skillAreas.set(s) });
    this.catalogsService.getActivityTemplateTypes().subscribe({ next: t => this.templateTypes.set(t) });
  }

  private mountEditor(): void {
    if (!this.editorHost) return;
    this.editorHost.clear();
    const code = this.selectedTemplateCode;
    const EditorClass = CONTENT_EDITOR_REGISTRY[code];
    if (!EditorClass) {
      this.editorUnavailable.set(true);
      this.isEditorValid.set(false);
      return;
    }
    this.editorUnavailable.set(false);
    const ref = this.editorHost.createComponent<ContentEditorBaseComponent>(EditorClass);
    // Pasa el JSON actual para preservar contenido si el usuario volvió atrás
    ref.setInput('initialJson', this.editorContentJson() || '{}');
    ref.instance.contentChange.subscribe((json: string) => this.editorContentJson.set(json));
    ref.instance.validChange.subscribe((valid: boolean) => this.isEditorValid.set(valid));
    ref.changeDetectorRef.detectChanges();
  }

  nextStep(): void {
    if (this.currentStep() < this.totalSteps) {
      const nextStepNum = this.currentStep() + 1;
      this.currentStep.set(nextStepNum);
      if (nextStepNum === 3) {
        afterNextRender(() => this.mountEditor(), { injector: this.injector });
      }
    }
  }

  prevStep(): void {
    if (this.currentStep() > 1) {
      if (this.currentStep() === 3 && this.editorHost) this.editorHost.clear();
      this.currentStep.set(this.currentStep() - 1);
    }
  }

  submit(): void {
    if (!this.step1Valid || !this.step3Valid) return;
    this.isLoading.set(true);

    const request: CreateActivityRequest = {
      title:                    this.meta.title.trim(),
      description:              this.meta.description.trim() || undefined,
      instructions:             this.meta.instructions.trim() || undefined,
      categoryId:               +this.meta.categoryId,
      skillAreaId:              this.meta.skillAreaId ? +this.meta.skillAreaId : undefined,
      complexityLevel:          this.meta.complexityLevel,
      estimatedDurationMinutes: this.meta.estimatedDurationMinutes,
      requiresSupervision:      this.meta.requiresSupervision,
      hasVisualSupport:         this.meta.hasVisualSupport,
      hasAudioSupport:          this.meta.hasAudioSupport,
      usesEasyReading:          this.meta.usesEasyReading,
      usesPictograms:           this.meta.usesPictograms,
      resourcesUrl:             this.meta.resourcesUrl.trim() || undefined,
      templateTypeId:           +this.meta.templateTypeId,
      contentJson:              this.editorContentJson(),
    };

    this.activitiesService.create(request).subscribe({
      next: (activity) => {
        this.toastService.success('Actividad creada exitosamente.');
        this.savedActivity.set({
          id: activity.id,
          encryptedId: activity.encryptedId,
          title: activity.title,
          templateTypeCode: this.selectedTemplateCode,
          templateTypeName: this.selectedTemplateName,
          isActive: true,
          isStandardActivity: false,
          createdAt: new Date().toISOString(),
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Error al crear la actividad.');
        this.isLoading.set(false);
      },
    });
  }

  openAssignModal(): void { this.showAssignModal = true; }
  onAssigned(): void      { this.router.navigate([AppRoutes.Pro.Activities]); }
  skipAssign(): void      { this.router.navigate([AppRoutes.Pro.Activities]); }

  cancel(): void {
    this.router.navigate([AppRoutes.Pro.Activities]);
  }
}
