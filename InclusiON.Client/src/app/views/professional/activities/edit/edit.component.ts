import { afterNextRender, Component, inject, Injector, OnInit, signal, ViewChild, ViewContainerRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ActivitiesService } from '@services/activities.service';
import { CatalogsService } from '@services/catalogs.service';
import { ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ActivityCategoryItem, ActivityTemplateTypeItem, SkillAreaItem, UpdateActivityRequest, ActivityListItemResponse } from '@models';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ButtonDirective, ColComponent, RowComponent,
  FormControlDirective, FormSelectDirective,
  FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  SpinnerComponent, AlertComponent,
} from '@coreui/angular';
import { CONTENT_EDITOR_REGISTRY } from '../new/editors/content-editor-registry';
import { ContentEditorBaseComponent } from '../new/editors/content-editor-base.component';

@Component({
  selector: 'app-activities-edit',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    ButtonDirective, ColComponent, RowComponent,
    FormControlDirective, FormSelectDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    SpinnerComponent, AlertComponent,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly catalogsService   = inject(CatalogsService);
  private readonly toastService      = inject(ToastService);
  private readonly router            = inject(Router);
  private readonly route             = inject(ActivatedRoute);
  private readonly injector          = inject(Injector);

  @ViewChild('editorHost', { read: ViewContainerRef }) private editorHost!: ViewContainerRef;

  activityId = '';
  templateTypeCode = '';

  categories    = signal<ActivityCategoryItem[]>([]);
  skillAreas    = signal<SkillAreaItem[]>([]);
  templateTypes = signal<ActivityTemplateTypeItem[]>([]);
  isLoadingData = signal(true);
  isLoading     = signal(false);

  meta = {
    title: '',
    description: '',
    instructions: '',
    categoryId: 0,
    skillAreaId: 0,
    complexityLevel: 1,
    estimatedDurationMinutes: 15,
    requiresSupervision: false,
    hasVisualSupport: true,
    hasAudioSupport: false,
    usesEasyReading: false,
    usesPictograms: true,
    resourcesUrl: '',
    isTemplate: false,
  };

  editorContentJson  = signal('{}');
  isEditorValid      = signal(false);
  editorUnavailable  = signal(false);

  similarActivities = signal<ActivityListItemResponse[]>([]);
  similarLoading = signal(false);

  get isValid(): boolean {
    return !!this.meta.title.trim() && !!this.meta.categoryId && this.isEditorValid();
  }

  ngOnInit(): void {
    this.activityId = this.route.snapshot.paramMap.get('id')!;

    this.catalogsService.getActivityCategories().subscribe({ next: c => this.categories.set(c) });
    this.catalogsService.getSkillAreas().subscribe({ next: s => this.skillAreas.set(s) });
    this.catalogsService.getActivityTemplateTypes().subscribe({ next: t => this.templateTypes.set(t) });

    this.activitiesService.getById(this.activityId).subscribe({
      next: (activity) => {
        this.templateTypeCode = activity.templateTypeCode;
        this.meta = {
          title:                    activity.title,
          description:              activity.description ?? '',
          instructions:             activity.instructions ?? '',
          categoryId:               activity.categoryId,
          skillAreaId:              activity.skillAreaId ?? 0,
          complexityLevel:          activity.complexityLevel ?? 1,
          estimatedDurationMinutes: activity.estimatedDurationMinutes ?? 15,
          requiresSupervision:      activity.requiresSupervision,
          hasVisualSupport:         activity.hasVisualSupport,
          hasAudioSupport:          activity.hasAudioSupport,
          usesEasyReading:          activity.usesEasyReading,
          usesPictograms:           activity.usesPictograms,
          resourcesUrl:             activity.resourcesUrl ?? '',
          isTemplate:               activity.isTemplate,
        };
        this.editorContentJson.set(activity.contentJson ?? '{}');
        this.isLoadingData.set(false);
        afterNextRender(() => this.mountEditor(), { injector: this.injector });
        this.loadSimilarActivities();
      },
      error: () => {
        this.toastService.error('Error al cargar la actividad.');
        this.router.navigate([AppRoutes.Pro.Activities]);
      },
    });
  }

  private mountEditor(): void {
    if (!this.editorHost) return;
    this.editorHost.clear();
    const code = this.templateTypeCode;
    const EditorClass = CONTENT_EDITOR_REGISTRY[code];
    if (!EditorClass) {
      this.editorUnavailable.set(true);
      this.isEditorValid.set(false);
      return;
    }
    this.editorUnavailable.set(false);
    const ref = this.editorHost.createComponent<ContentEditorBaseComponent>(EditorClass);
    ref.setInput('initialJson', this.editorContentJson() || '{}');
    ref.instance.contentChange.subscribe((json: string) => this.editorContentJson.set(json));
    ref.instance.validChange.subscribe((valid: boolean) => this.isEditorValid.set(valid));
    ref.changeDetectorRef.detectChanges();
  }

  submit(): void {
    if (!this.isValid) return;
    this.isLoading.set(true);

    const request: UpdateActivityRequest = {
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
      contentJson:              this.editorContentJson(),
      isTemplate:               this.meta.isTemplate,
    };

    this.activitiesService.update(this.activityId, request).subscribe({
      next: () => {
        this.toastService.success('Actividad actualizada.');
        this.router.navigate([AppRoutes.Pro.Activities]);
      },
      error: () => {
        this.toastService.error('Error al actualizar la actividad.');
        this.isLoading.set(false);
      },
    });
  }

  cancel(): void { this.router.navigate([AppRoutes.Pro.Activities]); }

  private loadSimilarActivities(): void {
    this.similarLoading.set(true);
    this.activitiesService.getSimilarActivities(this.activityId, 5).subscribe({
      next: (results) => {
        this.similarActivities.set(results);
        this.similarLoading.set(false);
      },
      error: () => {
        this.similarLoading.set(false);
      },
    });
  }
}
