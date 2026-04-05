import { Component, Input, Output, EventEmitter, inject, OnInit, signal } from '@angular/core';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { PersonSkillProfileResponse, SkillAreaItem } from '@models';
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
    ButtonDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
  ],
  template: `
    <div class="d-flex flex-wrap gap-2 mb-3">
      @for (sp of currentSkillProfile(); track sp.skillAreaId) {
        <span class="skill-chip d-inline-flex align-items-center gap-1 px-3 py-1 rounded-pill"
              [style.background-color]="sp.color ?? '#6c757d'" style="color: #fff;">
          @if (sp.icon) {
            <span class="skill-chip-icon">{{ sp.icon }}</span>
          }
          <span>{{ sp.skillAreaName }}</span>
          <button type="button" class="btn-close btn-close-white ms-1 skill-chip-close"
                  (click)="deactivate(sp.skillAreaId)"
                  [attr.aria-label]="'Quitar ' + sp.skillAreaName"></button>
        </span>
      }
      @if (currentSkillProfile().length === 0) {
        <span class="text-body-secondary">Sin areas de habilidad asignadas.</span>
      }
    </div>
    <button cButton color="primary" size="sm" (click)="openModal()">Agregar Area</button>

    <c-modal [visible]="showModal()" (visibleChange)="showModal.set($event)" (visibleChange)="!$event && closeModal()">
      <c-modal-header><strong>Agregar Áreas de Habilidad</strong></c-modal-header>
      <c-modal-body>
        @if (allSkillAreas().length === 0) {
          <p class="text-body-secondary">No hay áreas disponibles para agregar.</p>
        } @else {
          <p class="text-body-secondary mb-3">Seleccioná las áreas que deseas agregar:</p>
          @for (area of allSkillAreas(); track area.id) {
            <c-form-check class="mb-2">
              <input cFormCheckInput type="checkbox" [id]="'skill-' + area.id"
                     [checked]="selectedIds.has(area.id)"
                     (change)="toggle(area.id)" />
              <label cFormCheckLabel [for]="'skill-' + area.id">
                <strong>{{ area.name }}</strong>
                @if (area.description) {
                  <br /><small class="text-body-secondary">{{ area.description }}</small>
                }
              </label>
            </c-form-check>
          }
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="closeModal()">Cancelar</button>
        <button cButton color="primary" (click)="confirm()" [disabled]="selectedIds.size === 0 || loading()">
          @if (loading()) { <span class="spinner-border spinner-border-sm me-1"></span> }
          Agregar ({{ selectedIds.size }})
        </button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class ProfessionalSkillsComponent implements OnInit {
  @Input({ required: true }) personId!: string;
  @Input() skillProfile: PersonSkillProfileResponse[] = [];
  @Output() skillProfileChange = new EventEmitter<PersonSkillProfileResponse[]>();

  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly toastService = inject(ToastService);

  allSkillAreas = signal<SkillAreaItem[]>([]);
  showModal = signal(false);
  selectedIds = new Set<number>();
  loading = signal(false);
  currentSkillProfile = signal<PersonSkillProfileResponse[]>([]);

  ngOnInit(): void {
    this.currentSkillProfile.set(this.skillProfile);
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
    });
  }

  private loadSkillProfile(): void {
    this.personsService.getSkillProfile(this.personId).subscribe({
      next: (data) => this.skillProfileChange.emit(data ?? []),
    });
  }
}
