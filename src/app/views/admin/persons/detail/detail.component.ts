import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { PersonResponse, PersonSkillProfileResponse, SkillAreaItem } from '../../../../models';
import { formatDate, formatDateTime } from '@shared/utils';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormLabelDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-detail',
  imports: [
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    ButtonDirective,
    FormsModule,
    SpinnerComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ConfirmModalComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly toastService = inject(ToastService);

  person: PersonResponse | null = null;
  showDeactivateModal = false;

  // Skill profile
  skillProfile: PersonSkillProfileResponse[] = [];
  allSkillAreas: SkillAreaItem[] = [];
  showAddSkillAreaModal = false;
  selectedSkillAreaIds: Set<number> = new Set();
  skillAreaError = '';
  skillAreaLoading = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (person) => {
          this.person = person;
          this.loadSkillProfile();
        },
        error: () => this.router.navigate(['/admin/persons']),
      });
    }
  }

  loadSkillProfile(): void {
    if (!this.person) return;
    this.personsService.getSkillProfile(this.person.id).subscribe({
      next: (data) => (this.skillProfile = data ?? []),
    });
  }

  openAddSkillAreaModal(): void {
    this.skillAreaError = '';
    this.selectedSkillAreaIds = new Set();
    this.catalogsService.getSkillAreas().subscribe({
      next: (areas) => {
        const activeIds = new Set(this.skillProfile.filter(sp => sp.isActive).map(sp => sp.skillAreaId));
        this.allSkillAreas = (areas ?? []).filter(a => !activeIds.has(a.id));
        this.showAddSkillAreaModal = true;
      },
    });
  }

  closeAddSkillAreaModal(): void {
    this.showAddSkillAreaModal = false;
    this.skillAreaError = '';
  }

  toggleSkillArea(id: number): void {
    if (this.selectedSkillAreaIds.has(id)) {
      this.selectedSkillAreaIds.delete(id);
    } else {
      this.selectedSkillAreaIds.add(id);
    }
  }

  confirmAddSkillAreas(): void {
    if (!this.person || this.selectedSkillAreaIds.size === 0) return;
    this.skillAreaLoading = true;
    this.skillAreaError = '';
    const ids = Array.from(this.selectedSkillAreaIds);
    let completed = 0;
    let errors = 0;

    for (const areaId of ids) {
      this.personsService.addSkillArea(this.person.id, areaId).subscribe({
        next: () => {
          completed++;
          if (completed + errors === ids.length) {
            this.skillAreaLoading = false;
            this.showAddSkillAreaModal = false;
            this.loadSkillProfile();
          }
        },
        error: () => {
          errors++;
          if (completed + errors === ids.length) {
            this.skillAreaLoading = false;
            this.skillAreaError = `${errors} área(s) no se pudieron agregar.`;
            if (completed > 0) {
              this.loadSkillProfile();
            }
          }
        },
      });
    }
  }

  deactivateSkillArea(areaId: number): void {
    if (!this.person) return;
    this.personsService.deactivateSkillArea(this.person.id, areaId).subscribe({
      next: () => this.loadSkillProfile(),
    });
  }

  goToEdit(): void {
    if (this.person) {
      this.router.navigate(['/admin/persons', this.person.id, 'edit']);
    }
  }

  formatDate = formatDate;
  formatDateTime = formatDateTime;

  formatLevel(level: number | null | undefined): string {
    return level != null ? `${level} / 5` : 'Sin especificar';
  }

  formatBoolean(value: boolean): string {
    return value ? 'Si' : 'No';
  }

  goBack(): void {
    this.router.navigate(['/admin/persons']);
  }

  confirmDeactivate(): void {
    if (!this.person) return;
    this.personsService.deactivatePerson(this.person.id).subscribe({
      next: () => {
        this.toastService.success('Persona desactivada exitosamente');
        this.showDeactivateModal = false;
        this.person!.isActive = false;
      },
      error: () => {
        this.toastService.error('Error al desactivar la persona');
        this.showDeactivateModal = false;
      },
    });
  }
}
