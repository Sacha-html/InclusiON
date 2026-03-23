import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogsService, PersonsService } from '@services';
import { PersonResponse, PersonSkillProfileResponse, SkillAreaItem } from '../../../../models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
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
    FormSelectDirective,
    ButtonDirective,
    FormsModule,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);

  person: PersonResponse | null = null;

  // Skill profile
  skillProfile: PersonSkillProfileResponse[] = [];
  allSkillAreas: SkillAreaItem[] = [];
  showAddSkillAreaModal = false;
  selectedSkillAreaId: number | null = null;
  skillAreaError = '';
  skillAreaLoading = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (response) => {
          this.person = response.data;
          this.loadSkillProfile();
        },
        error: () => this.router.navigate(['/admin/persons']),
      });
    }
  }

  loadSkillProfile(): void {
    if (!this.person) return;
    this.personsService.getSkillProfile(this.person.id).subscribe({
      next: (response) => (this.skillProfile = response.data ?? []),
    });
  }

  openAddSkillAreaModal(): void {
    this.skillAreaError = '';
    this.selectedSkillAreaId = null;
    this.catalogsService.getSkillAreas().subscribe({
      next: (areas) => {
        // Filter out already active areas
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

  confirmAddSkillArea(): void {
    if (!this.person || !this.selectedSkillAreaId) return;
    this.skillAreaLoading = true;
    this.skillAreaError = '';
    this.personsService.addSkillArea(this.person.id, this.selectedSkillAreaId).subscribe({
      next: () => {
        this.skillAreaLoading = false;
        this.showAddSkillAreaModal = false;
        this.loadSkillProfile();
      },
      error: (err) => {
        this.skillAreaLoading = false;
        this.skillAreaError = err?.error?.message ?? 'Error al agregar el area de habilidad.';
      },
    });
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

  formatDate(date: string | null | undefined): string {
    if (!date) return 'Sin especificar';
    const d = new Date(date);
    if (isNaN(d.getTime())) return 'Sin especificar';
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  formatDateTime(date: string | null | undefined): string {
    if (!date) return 'Sin especificar';
    const d = new Date(date);
    if (isNaN(d.getTime())) return 'Sin especificar';
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })
      + ' ' + d.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  }

  formatLevel(level: number | null | undefined): string {
    return level != null ? `${level} / 5` : 'Sin especificar';
  }

  formatBoolean(value: boolean): string {
    return value ? 'Si' : 'No';
  }

  goBack(): void {
    this.router.navigate(['/admin/persons']);
  }
}
