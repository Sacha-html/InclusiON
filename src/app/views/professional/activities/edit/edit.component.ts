import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ActivitiesService } from '@services/activities.service';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import { CatalogsService } from '@services/catalogs.service';
import { ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ActivityCategoryItem, ActivityTemplateTypeItem, SkillAreaItem, UpdateActivityRequest, SelectFigureContent } from '@models';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ButtonDirective, ColComponent, RowComponent,
  FormControlDirective, FormSelectDirective,
  FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-activities-edit',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    ButtonDirective, ColComponent, RowComponent,
    FormControlDirective, FormSelectDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    SpinnerComponent,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly arasaacService    = inject(ArasaacService);
  private readonly catalogsService   = inject(CatalogsService);
  private readonly toastService      = inject(ToastService);
  private readonly router            = inject(Router);
  private readonly route             = inject(ActivatedRoute);

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
  };

  content: SelectFigureContent = {
    instruction: '',
    correctItemId: '',
    items: [],
  };

  arasaacSearch  = '';
  arasaacResults = signal<ArasaacPictogram[]>([]);
  isSearching    = signal(false);
  private search$ = new Subject<string>();

  get isSelectFigure(): boolean { return this.templateTypeCode === 'SELECT_FIGURE'; }

  get isValid(): boolean {
    if (!this.meta.title.trim() || !this.meta.categoryId) return false;
    if (this.isSelectFigure) {
      return (
        !!this.content.instruction.trim() &&
        this.content.items.length >= 2 &&
        !!this.content.correctItemId
      );
    }
    return true;
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
        };
        if (this.isSelectFigure && activity.contentJson) {
          try {
            this.content = JSON.parse(activity.contentJson) as SelectFigureContent;
          } catch {
            this.content = { instruction: '', correctItemId: '', items: [] };
          }
        }
        this.isLoadingData.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar la actividad.');
        this.router.navigate([AppRoutes.Pro.Activities]);
      },
    });

    this.search$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term.trim()) return of([]);
        this.isSearching.set(true);
        return this.arasaacService.search(term).pipe(catchError(() => of([])));
      }),
    ).subscribe(results => {
      this.arasaacResults.set(results);
      this.isSearching.set(false);
    });
  }

  onArasaacSearchChange(term: string): void { this.search$.next(term); }

  addPictogram(pic: ArasaacPictogram): void {
    const id = crypto.randomUUID();
    this.content.items.push({ id, pictogramId: pic.id, label: pic.keyword });
    if (this.content.items.length === 1) this.content.correctItemId = id;
  }

  removeItem(itemId: string): void {
    this.content.items = this.content.items.filter(i => i.id !== itemId);
    if (this.content.correctItemId === itemId)
      this.content.correctItemId = this.content.items[0]?.id ?? '';
  }

  setCorrect(itemId: string): void { this.content.correctItemId = itemId; }

  pictogramUrl(id: number): string { return this.arasaacService.getPictogramUrl(id); }

  submit(): void {
    if (!this.isValid) return;
    this.isLoading.set(true);

    const contentJson = this.isSelectFigure ? JSON.stringify(this.content) : JSON.stringify({});

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
      contentJson,
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
}
