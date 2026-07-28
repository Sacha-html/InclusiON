import { Component, Input, Output, EventEmitter, inject, OnInit, OnChanges, signal } from '@angular/core';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { RoadmapService } from '@services/roadmap.service';
import { PersonSkillProfileResponse, SkillAreaItem, SkillRadarPointResponse } from '@models';
import { SkillChipComponent, SkillRadarChartComponent } from '@shared/components';
import {
  ButtonDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-skills',
  standalone: true,
  imports: [
    SkillChipComponent,
    SkillRadarChartComponent,
    ButtonDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
  ],
  templateUrl: './professional-skills.component.html',
})
export class ProfessionalSkillsComponent implements OnInit, OnChanges {
  @Input({ required: true }) personId!: string;
  @Input() skillProfile: PersonSkillProfileResponse[] = [];
  @Output() skillProfileChange = new EventEmitter<PersonSkillProfileResponse[]>();

  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly toastService = inject(ToastService);
  private readonly roadmapService = inject(RoadmapService);

  allSkillAreas = signal<SkillAreaItem[]>([]);
  showModal = signal(false);
  selectedIds = new Set<number>();
  loading = signal(false);
  currentSkillProfile = signal<PersonSkillProfileResponse[]>([]);
  radarPoints = signal<SkillRadarPointResponse[]>([]);
  radarLoading = signal(false);

  ngOnInit(): void {
    this.currentSkillProfile.set(this.skillProfile);
    this.loadRadar();
  }

  ngOnChanges(): void {
    this.currentSkillProfile.set(this.skillProfile);
  }

  private loadRadar(): void {
    this.radarLoading.set(true);
    this.roadmapService.getSkillRadar(this.personId).subscribe({
      next: (data) => { this.radarPoints.set(data); this.radarLoading.set(false); },
      error: ()    => { this.radarLoading.set(false); },  // radar is optional — no toast
    });
  }

  private loadAllSkillAreas(): void {
    this.catalogsService.getSkillAreas().subscribe({
      next: (areas) => {
        const activeIds = new Set(
          this.skillProfile
            .filter((sp) => sp.isActive)
            .map((sp) => sp.skillAreaId)
        );
        this.allSkillAreas.set((areas ?? []).filter((a) => !activeIds.has(a.id)));
      },
      error: () => this.toastService.error('Error al cargar las áreas de habilidad'),
    });
  }

  openModal(): void {
    this.selectedIds = new Set();
    this.loadAllSkillAreas();
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  toggle(id: number): void {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
  }

  confirm(): void {
    if (this.selectedIds.size === 0) return;
    this.loading.set(true);
    const ids = Array.from(this.selectedIds);
    let completed = 0;
    let errors = 0;

    for (const areaId of ids) {
      this.personsService.addSkillArea(this.personId, areaId).subscribe({
        next: () => {
          completed++;
          if (completed + errors === ids.length) {
            this.loading.set(false);
            this.showModal.set(false);
            this.loadSkillProfile();
            this.toastService.success(`${completed} área(s) de habilidad agregada(s)`);
          }
        },
        error: () => {
          errors++;
          if (completed + errors === ids.length) {
            this.loading.set(false);
            this.showModal.set(false);
            this.loadSkillProfile();
            if (completed > 0) {
              this.toastService.warning(`${completed} agregada(s), ${errors} con error`);
            } else {
              this.toastService.error('Error al agregar áreas de habilidad');
            }
          }
        },
      });
    }
  }

  deactivate(areaId: number): void {
    this.personsService.deactivateSkillArea(this.personId, areaId).subscribe({
      next: () => {
        this.loadSkillProfile();
        this.toastService.success('Area de habilidad removida');
      },
      error: () => this.toastService.error('Error al desactivar el área de habilidad'),
    });
  }

  private loadSkillProfile(): void {
    this.personsService.getSkillProfile(this.personId).subscribe({
      next: (data) => {
        const updated = data ?? [];
        this.currentSkillProfile.set(updated);
        this.skillProfileChange.emit(updated);
      },
    });
  }
}
