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
  templateUrl: './person-skills.component.html',
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
