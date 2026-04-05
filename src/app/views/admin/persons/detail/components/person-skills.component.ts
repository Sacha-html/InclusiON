import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { PersonResponse, PersonSkillProfileResponse, SkillAreaItem } from '@models';
import {
  ButtonDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-person-skills',
  standalone: true,
  imports: [
    ButtonDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    SpinnerComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
  ],
  template: `
    <h5 class="mb-3">Perfil de Habilidades</h5>

    <div class="d-flex flex-wrap gap-2 mb-3">
      @for (sp of skillProfile; track sp.skillAreaId) {
        <span class="skill-chip d-inline-flex align-items-center gap-1 px-3 py-1 rounded-pill"
              [style.background-color]="sp.color ?? '#6c757d'"
              [style.color]="'#fff'">
          @if (sp.icon) {
            <span class="material-icons skill-chip-icon">{{ sp.icon }}</span>
          }
          {{ sp.skillAreaName }}
          @if (person.isActive) {
            <button cButtonClose (click)="removeSkillArea.emit(sp.skillAreaId)"
                    [attr.aria-label]="'Quitar ' + sp.skillAreaName">
            </button>
          }
        </span>
      }
      @if (skillProfile.length === 0) {
        <span class="text-body-secondary">Sin areas de habilidad asignadas.</span>
      }
    </div>

    @if (person.isActive) {
      <button cButton color="primary" size="sm" (click)="openAddModal.emit()"
              aria-label="Agregar area de habilidad">
        Agregar Area
      </button>
    }

    <!-- Modal Agregar Áreas de Habilidad -->
    <c-modal [visible]="showAddModal" (visibleChange)="!$event && closeModal.emit()"
             aria-labelledby="add-skill-area-modal-title">
      <c-modal-header>
        <strong id="add-skill-area-modal-title">Agregar Áreas de Habilidad</strong>
      </c-modal-header>
      <c-modal-body>
        @if (error) {
          <div class="alert alert-danger">{{ error }}</div>
        }
        @if (availableSkillAreas.length === 0) {
          <p class="text-body-secondary">No hay áreas disponibles para agregar.</p>
        } @else {
          <p class="text-body-secondary mb-3">Seleccioná las áreas que deseas agregar:</p>
          @for (area of availableSkillAreas; track area.id) {
            <c-form-check class="mb-2">
              <input cFormCheckInput type="checkbox" [id]="'admin-skill-' + area.id"
                     [checked]="selectedIds.has(area.id)"
                     (change)="toggleSkillArea(area.id)" />
              <label cFormCheckLabel [for]="'admin-skill-' + area.id">
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
        <button cButton color="secondary" (click)="closeModal.emit()">Cancelar</button>
        <button cButton color="primary" (click)="confirmAdd.emit(getSelectedIds())"
                [disabled]="selectedIds.size === 0 || loading">
          @if (loading) { <c-spinner size="sm" class="me-1"></c-spinner> }
          Agregar ({{ selectedIds.size }})
        </button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class PersonSkillsComponent implements OnChanges {
  @Input({ required: true }) person!: PersonResponse;
  @Input() skillProfile: PersonSkillProfileResponse[] = [];
  @Input() availableSkillAreas: SkillAreaItem[] = [];
  @Input() showAddModal = false;
  @Input() loading = false;
  @Input() error = '';

  @Output() removeSkillArea = new EventEmitter<number>();
  @Output() openAddModal = new EventEmitter<void>();
  @Output() closeModal = new EventEmitter<void>();
  @Output() confirmAdd = new EventEmitter<number[]>();

  selectedIds: Set<number> = new Set();

  ngOnChanges(): void {
    if (this.showAddModal) {
      const activeIds = new Set(this.skillProfile.filter(sp => sp.isActive).map(sp => sp.skillAreaId));
      this.availableSkillAreas = this.availableSkillAreas.filter(a => !activeIds.has(a.id));
      this.selectedIds = new Set();
    }
  }

  toggleSkillArea(id: number): void {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
  }

  getSelectedIds(): number[] {
    return Array.from(this.selectedIds);
  }
}
