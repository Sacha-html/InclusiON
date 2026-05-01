import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ActivitiesService } from '@services/activities.service';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import { CatalogsService } from '@services/catalogs.service';
import { ToastService } from '@services';
import { ActivityCategoryItem, ActivityTemplateTypeItem, SkillAreaItem } from '@models';
import { CreateActivityRequest } from '@models/requests/activities';
import { SelectFigureContent, SelectFigureItem } from '@models/responses/activity.response';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ButtonDirective, ColComponent, RowComponent,
  FormControlDirective, FormSelectDirective, FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  SpinnerComponent, ProgressComponent,
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
    SpinnerComponent, ProgressComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly arasaacService    = inject(ArasaacService);
  private readonly catalogsService   = inject(CatalogsService);
  private readonly toastService      = inject(ToastService);
  private readonly router            = inject(Router);

  // Wizard state
  currentStep = signal(1);
  totalSteps  = 2;

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

  // Step 2 — SELECT_FIGURE content
  content: SelectFigureContent = {
    instruction: '',
    correctItemId: '',
    items: [],
  };

  // ARASAAC search
  arasaacSearch   = '';
  arasaacResults  = signal<ArasaacPictogram[]>([]);
  isSearching     = signal(false);
  private search$ = new Subject<string>();

  isLoading = signal(false);

  get selectedTemplateName(): string {
    return this.templateTypes().find(t => t.id === +this.meta.templateTypeId)?.name ?? '';
  }

  get isSelectFigure(): boolean {
    const code = this.templateTypes().find(t => t.id === +this.meta.templateTypeId)?.code ?? '';
    return code === 'SELECT_FIGURE';
  }

  get step1Valid(): boolean {
    return !!this.meta.title.trim() && this.meta.categoryId > 0 && this.meta.templateTypeId > 0;
  }

  get step2Valid(): boolean {
    if (!this.isSelectFigure) return true;
    return (
      !!this.content.instruction.trim() &&
      this.content.items.length >= 2 &&
      !!this.content.correctItemId &&
      this.content.items.some(i => i.id === this.content.correctItemId)
    );
  }

  ngOnInit(): void {
    this.catalogsService.getActivityCategories().subscribe({ next: c => this.categories.set(c) });
    this.catalogsService.getSkillAreas().subscribe({ next: s => this.skillAreas.set(s) });
    this.catalogsService.getActivityTemplateTypes().subscribe({ next: t => this.templateTypes.set(t) });

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

  onArasaacSearchChange(term: string): void {
    this.search$.next(term);
  }

  addPictogram(pic: ArasaacPictogram): void {
    const id = crypto.randomUUID();
    this.content.items.push({ id, pictogramId: pic.id, label: pic.keyword });
    if (this.content.items.length === 1) {
      this.content.correctItemId = id;
    }
  }

  removeItem(itemId: string): void {
    this.content.items = this.content.items.filter(i => i.id !== itemId);
    if (this.content.correctItemId === itemId) {
      this.content.correctItemId = this.content.items[0]?.id ?? '';
    }
  }

  setCorrect(itemId: string): void {
    this.content.correctItemId = itemId;
  }

  pictogramUrl(id: number): string {
    return this.arasaacService.getPictogramUrl(id);
  }

  nextStep(): void {
    if (this.currentStep() < this.totalSteps) this.currentStep.set(this.currentStep() + 1);
  }

  prevStep(): void {
    if (this.currentStep() > 1) this.currentStep.set(this.currentStep() - 1);
  }

  submit(): void {
    if (!this.step1Valid || !this.step2Valid) return;
    this.isLoading.set(true);

    const contentJson = this.isSelectFigure
      ? JSON.stringify(this.content)
      : JSON.stringify({});

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
      contentJson,
    };

    this.activitiesService.create(request).subscribe({
      next: (activity) => {
        this.toastService.success('Actividad creada exitosamente.');
        this.router.navigate(['/pro/activities']);
      },
      error: () => {
        this.toastService.error('Error al crear la actividad.');
        this.isLoading.set(false);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/pro/activities']);
  }
}
