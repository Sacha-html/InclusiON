import { Component, Input, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { RoadmapService } from '@services/roadmap.service';
import { ActivitiesService } from '@services/activities.service';
import { CatalogsService } from '@services/catalogs.service';
import { AuthService, ToastService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import { RoadmapResponse, RoadmapAreaResponse, RoadmapActivityResponse, ActivityListItemResponse, SkillAreaItem, AddRoadmapActivityRequest } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-professional-roadmap-tab',
  standalone: true,
  imports: [
    FormsModule,
    DragDropModule,
    CardComponent, CardHeaderComponent, CardBodyComponent,
    RowComponent, ColComponent,
    BadgeComponent,
    ButtonDirective,
    SpinnerComponent,
    FormControlDirective, FormSelectDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    ConfirmModalComponent,
    EmptyStateComponent,
  ],
  templateUrl: './professional-roadmap-tab.component.html',
  styleUrl: './professional-roadmap-tab.component.scss',
})
export class ProfessionalRoadmapTabComponent implements OnInit {
  @Input({ required: true }) personId!: string;

  private readonly roadmapService  = inject(RoadmapService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly authService     = inject(AuthService);
  private readonly toastService    = inject(ToastService);

  roadmap       = signal<RoadmapResponse | null>(null);
  loading       = signal(false);
  skillAreas    = signal<SkillAreaItem[]>([]);
  activities    = signal<ActivityListItemResponse[]>([]);

  canCreate = this.authService.hasPermission(Permissions.Roadmap.Create);
  canUpdate = this.authService.hasPermission(Permissions.Roadmap.Update);
  canDelete = this.authService.hasPermission(Permissions.Roadmap.Delete);

  // ── Create roadmap ──────────────────────────────────────────────────
  showCreateModal  = false;
  createNotes      = '';
  creating         = false;

  // ── Edit notes ──────────────────────────────────────────────────────
  showEditNotesModal = false;
  editNotes          = '';
  savingNotes        = false;

  // ── Add area ────────────────────────────────────────────────────────
  showAddAreaModal   = false;
  selectedSkillAreaId: number | null = null;
  addingArea         = false;

  usedAreaIds = computed(() => new Set((this.roadmap()?.areas ?? []).map(a => a.skillAreaId)));

  availableSkillAreas = computed(() =>
    this.skillAreas().filter(a => !this.usedAreaIds().has(a.id))
  );

  // ── Add activity ────────────────────────────────────────────────────
  showAddActivityModal = false;
  targetArea: RoadmapAreaResponse | null = null;
  loadingActivities    = false;
  selectedActivityId: number | null = null;
  activityConfig: AddRoadmapActivityRequest = this.defaultActivityConfig();
  addingActivity       = false;

  usedActivityIdsInArea = computed(() =>
    new Set(this.targetArea?.activities.map(a => a.activityId) ?? [])
  );

  availableActivities = computed(() =>
    this.activities().filter(a => !this.usedActivityIdsInArea().has(a.id))
  );

  // ── Remove area ─────────────────────────────────────────────────────
  confirmRemoveArea: RoadmapAreaResponse | null = null;
  removingArea = false;

  // ── Remove activity ─────────────────────────────────────────────────
  confirmRemoveActivity: { area: RoadmapAreaResponse; activity: RoadmapActivityResponse } | null = null;
  removingActivity = false;

  // ── Unlock activity ─────────────────────────────────────────────────
  unlockingActivityId: string | null = null;

  ngOnInit(): void {
    this.loadRoadmap();
  }

  private loadRoadmap(): void {
    this.loading.set(true);
    this.roadmapService.getRoadmap(this.personId).subscribe({
      next:  (data) => { this.roadmap.set(data); this.loading.set(false); },
      error: (err)  => {
        this.loading.set(false);
        if (err?.status !== 404) {
          this.toastService.error('Error al cargar hoja de ruta');
        }
      },
    });
  }

  // ── Create ──────────────────────────────────────────────────────────

  openCreateModal(): void {
    this.createNotes = '';
    this.showCreateModal = true;
  }

  submitCreate(): void {
    this.creating = true;
    this.roadmapService.createRoadmap(this.personId, this.createNotes || null).subscribe({
      next: (data) => {
        this.roadmap.set(data);
        this.toastService.success('Hoja de ruta creada');
        this.showCreateModal = false;
        this.creating = false;
      },
      error: (err) => {
        this.creating = false;
        const msg = err?.userMessage ?? 'Error al crear hoja de ruta';
        this.toastService.error(msg);
      },
    });
  }

  // ── Edit notes ──────────────────────────────────────────────────────

  openEditNotesModal(): void {
    this.editNotes = this.roadmap()?.notes ?? '';
    this.showEditNotesModal = true;
  }

  submitEditNotes(): void {
    this.savingNotes = true;
    this.roadmapService.updateNotes(this.personId, this.editNotes || null).subscribe({
      next: (data) => {
        this.roadmap.set(data);
        this.toastService.success('Notas actualizadas');
        this.showEditNotesModal = false;
        this.savingNotes = false;
      },
      error: (err) => {
        this.savingNotes = false;
        const msg = err?.userMessage ?? 'Error al guardar notas';
        this.toastService.error(msg);
      },
    });
  }

  // ── Add area ────────────────────────────────────────────────────────

  openAddAreaModal(): void {
    this.selectedSkillAreaId = null;
    if (this.skillAreas().length === 0) {
      this.catalogsService.getSkillAreas().subscribe({
        next:  (data) => this.skillAreas.set(data),
        error: ()     => this.toastService.error('Error al cargar áreas de habilidad'),
      });
    }
    this.showAddAreaModal = true;
  }

  submitAddArea(): void {
    if (!this.selectedSkillAreaId) return;
    const nextOrder = (this.roadmap()?.areas.length ?? 0) + 1;
    this.addingArea = true;
    this.roadmapService.addArea(this.personId, this.selectedSkillAreaId, nextOrder).subscribe({
      next: (area) => {
        this.roadmap.update(r => r ? { ...r, areas: [...r.areas, area] } : r);
        this.toastService.success('Área agregada');
        this.showAddAreaModal = false;
        this.addingArea = false;
      },
      error: (err) => {
        this.addingArea = false;
        if (err?.status === 409) {
          this.toastService.error('Esta área ya está en la hoja de ruta');
        } else {
          const msg = err?.userMessage ?? 'Error al agregar área';
          this.toastService.error(msg);
        }
      },
    });
  }

  // ── Add activity ─────────────────────────────────────────────────────

  openAddActivityModal(area: RoadmapAreaResponse): void {
    this.targetArea            = area;
    this.selectedActivityId    = null;
    this.activityConfig        = this.defaultActivityConfig();
    this.activityConfig.sequenceOrder = area.activities.length + 1;
    this.showAddActivityModal  = true;
    if (this.activities().length === 0) {
      this.loadingActivities = true;
      this.activitiesService.getActivities({ page: 1, pageSize: 100, isActive: true }).subscribe({
        next:  (data) => { this.activities.set(data.data); this.loadingActivities = false; },
        error: ()     => { this.loadingActivities = false; this.toastService.error('Error al cargar actividades'); },
      });
    }
  }

  submitAddActivity(): void {
    if (!this.targetArea || !this.selectedActivityId) return;
    this.addingActivity = true;
    const request: AddRoadmapActivityRequest = {
      ...this.activityConfig,
      activityId: this.selectedActivityId,
    };
    this.roadmapService.addActivity(this.personId, this.targetArea.encryptedId, request).subscribe({
      next: (activity) => {
        const areaId = this.targetArea!.encryptedId;
        this.roadmap.update(r => {
          if (!r) return r;
          return {
            ...r,
            areas: r.areas.map(a =>
              a.encryptedId === areaId ? { ...a, activities: [...a.activities, activity] } : a
            ),
          };
        });
        this.toastService.success('Actividad agregada');
        this.showAddActivityModal = false;
        this.addingActivity = false;
      },
      error: (err) => {
        this.addingActivity = false;
        if (err?.status === 409) {
          this.toastService.error('Esta actividad ya está en el área');
        } else {
          const msg = err?.userMessage ?? 'Error al agregar actividad';
          this.toastService.error(msg);
        }
      },
    });
  }

  // ── Remove area ──────────────────────────────────────────────────────

  openConfirmRemoveArea(area: RoadmapAreaResponse): void {
    this.confirmRemoveArea = area;
  }

  submitRemoveArea(): void {
    if (!this.confirmRemoveArea) return;
    const areaId = this.confirmRemoveArea.encryptedId;
    this.removingArea = true;
    this.roadmapService.removeArea(this.personId, areaId).subscribe({
      next: () => {
        this.roadmap.update(r => r ? { ...r, areas: r.areas.filter(a => a.encryptedId !== areaId) } : r);
        this.toastService.success('Área eliminada');
        this.confirmRemoveArea = null;
        this.removingArea = false;
      },
      error: (err) => {
        this.removingArea = false;
        const msg = err?.userMessage ?? 'Error al eliminar área';
        this.toastService.error(msg);
      },
    });
  }

  // ── Remove activity ──────────────────────────────────────────────────

  openConfirmRemoveActivity(area: RoadmapAreaResponse, activity: RoadmapActivityResponse): void {
    this.confirmRemoveActivity = { area, activity };
  }

  submitRemoveActivity(): void {
    if (!this.confirmRemoveActivity) return;
    const { area, activity } = this.confirmRemoveActivity;
    this.removingActivity = true;
    this.roadmapService.removeActivity(this.personId, area.encryptedId, activity.encryptedId).subscribe({
      next: () => {
        this.roadmap.update(r => {
          if (!r) return r;
          return {
            ...r,
            areas: r.areas.map(a =>
              a.encryptedId === area.encryptedId
                ? { ...a, activities: a.activities.filter(act => act.encryptedId !== activity.encryptedId) }
                : a
            ),
          };
        });
        this.toastService.success('Actividad eliminada');
        this.confirmRemoveActivity = null;
        this.removingActivity = false;
      },
      error: (err) => {
        this.removingActivity = false;
        const msg = err?.userMessage ?? 'Error al eliminar actividad';
        this.toastService.error(msg);
      },
    });
  }

  // ── Unlock activity ──────────────────────────────────────────────────

  unlockActivity(area: RoadmapAreaResponse, activity: RoadmapActivityResponse): void {
    this.unlockingActivityId = activity.encryptedId;
    this.roadmapService.unlockActivity(this.personId, area.encryptedId, activity.encryptedId).subscribe({
      next: (updated) => {
        const areaId = area.encryptedId;
        this.roadmap.update(r => {
          if (!r) return r;
          return {
            ...r,
            areas: r.areas.map(a =>
              a.encryptedId === areaId
                ? { ...a, activities: a.activities.map(act => act.encryptedId === updated.encryptedId ? updated : act) }
                : a
            ),
          };
        });
        this.toastService.success('Actividad desbloqueada');
        this.unlockingActivityId = null;
      },
      error: (err) => {
        this.unlockingActivityId = null;
        if (err?.status === 409) {
          this.toastService.error('La actividad ya estaba desbloqueada');
        } else {
          const msg = err?.userMessage ?? 'Error al desbloquear actividad';
          this.toastService.error(msg);
        }
      },
    });
  }

  // ── Reorder (drag-drop) ──────────────────────────────────────────────

  onDropActivity(event: CdkDragDrop<RoadmapActivityResponse[]>, area: RoadmapAreaResponse): void {
    if (event.previousIndex === event.currentIndex) return;

    // Optimistic update: reorder locally and reassign sequenceOrder
    const updatedAreas = this.roadmap()!.areas.map(a => {
      if (a.encryptedId !== area.encryptedId) return a;
      const activities = [...a.activities];
      moveItemInArray(activities, event.previousIndex, event.currentIndex);
      return {
        ...a,
        activities: activities.map((act, idx) => ({ ...act, sequenceOrder: idx + 1 })),
      };
    });
    this.roadmap.update(r => r ? { ...r, areas: updatedAreas } : r);

    // Persist
    const updatedArea = updatedAreas.find(a => a.encryptedId === area.encryptedId)!;
    const items = updatedArea.activities.map(act => ({ id: act.id, sequenceOrder: act.sequenceOrder }));

    this.roadmapService.reorderActivities(this.personId, area.encryptedId, items).subscribe({
      error: (err) => {
        const msg = err?.userMessage ?? 'Error al reordenar. Recargando...';
        this.toastService.error(msg);
        this.loadRoadmap(); // revert to server state
      },
    });
  }

  // ── Helpers ──────────────────────────────────────────────────────────

  difficultyLabel(level: number): string {
    return level === 1 ? 'Fácil' : level === 2 ? 'Medio' : 'Difícil';
  }

  difficultyColor(level: number): string {
    return level === 1 ? 'success' : level === 2 ? 'warning' : 'danger';
  }

  private defaultActivityConfig(): AddRoadmapActivityRequest {
    return {
      activityId:             0,
      sequenceOrder:          1,
      unlockThresholdPercent: 60,
      timeLimitSeconds:       null,
      maxAttempts:            null,
      showHints:              true,
      difficultyLevel:        1,
    };
  }
}
