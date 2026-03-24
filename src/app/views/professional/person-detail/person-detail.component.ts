import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { PersonResponse, PersonSkillProfileResponse, SkillAreaItem, UpdatePersonRequest } from '@models';
import { formatDate, toDisplayDate, toIsoDate } from '@shared/utils';
import {
  BadgeComponent, ButtonDirective, CardBodyComponent, CardComponent,
  CardHeaderComponent, ColComponent, FormControlDirective, FormLabelDirective,
  FormSelectDirective, FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  ModalBodyComponent, ModalComponent, ModalFooterComponent, ModalHeaderComponent,
  RowComponent, SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-person-detail',
  standalone: true,
  imports: [
    BadgeComponent, CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormSelectDirective, FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    ButtonDirective, FormsModule, SpinnerComponent,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
  ],
  templateUrl: './person-detail.component.html',
  styleUrl: './person-detail.component.scss',
})
export class PersonDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly toastService = inject(ToastService);

  person: PersonResponse | null = null;
  activeTab: 'datos' | 'funcional' | 'habilidades' = 'datos';

  // Edit personal data
  isEditingData = false;
  isSavingData = false;
  editPersonalData = {
    firstName: '',
    lastName: '',
    documentNumber: '',
    birthDate: '',
  };

  // Edit functional profile
  isEditing = false;
  isSaving = false;
  editData = {
    attentionLevel: 0,
    communicationLevel: 0,
    motorSkillLevel: 0,
    usesAAC: false,
    usesSignLanguage: false,
    interestsAndMotivators: '',
    learningStyle: '',
    availableResources: '',
    additionalTherapies: '',
    requiresLargeFont: false,
    requiresHighContrast: false,
    visualNoiseSensitivity: false,
    soundSensitivity: false,
  };

  // Skill profile
  skillProfile: PersonSkillProfileResponse[] = [];
  allSkillAreas: SkillAreaItem[] = [];
  showAddSkillAreaModal = false;
  selectedSkillAreaId: number | null = null;
  skillAreaError = '';
  skillAreaLoading = false;

  levels = [
    { value: 0, label: 'Sin evaluar' },
    { value: 1, label: '1 - Muy bajo' },
    { value: 2, label: '2 - Bajo' },
    { value: 3, label: '3 - Medio' },
    { value: 4, label: '4 - Alto' },
    { value: 5, label: '5 - Muy alto' },
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (person) => {
          this.person = person;
          this.loadSkillProfile();
        },
        error: () => this.router.navigate(['/pro/persons']),
      });
    }
  }

  // --- Personal data editing ---

  startEditingData(): void {
    if (!this.person) return;
    this.editPersonalData = {
      firstName: this.person.firstName,
      lastName: this.person.lastName,
      documentNumber: this.person.documentNumber ?? '',
      birthDate: toDisplayDate(this.person.birthDate),
    };
    this.isEditingData = true;
  }

  cancelEditingData(): void {
    this.isEditingData = false;
  }

  savePersonalData(): void {
    if (!this.person) return;
    this.isSavingData = true;

    const request: UpdatePersonRequest = {
      firstName: this.editPersonalData.firstName,
      lastName: this.editPersonalData.lastName,
      documentNumber: this.editPersonalData.documentNumber || undefined,
      birthDate: this.editPersonalData.birthDate ? toIsoDate(this.editPersonalData.birthDate) : undefined,
    };

    this.personsService.updatePerson(this.person.id, request).subscribe({
      next: (person) => {
        this.person = person;
        this.isEditingData = false;
        this.isSavingData = false;
        this.toastService.success('Datos personales actualizados');
      },
      error: () => {
        this.isSavingData = false;
        this.toastService.error('Error al actualizar datos');
      },
    });
  }

  // --- Functional profile editing ---

  startEditing(): void {
    if (!this.person) return;
    this.editData = {
      attentionLevel: this.person.attentionLevel ?? 0,
      communicationLevel: this.person.communicationLevel ?? 0,
      motorSkillLevel: this.person.motorSkillLevel ?? 0,
      usesAAC: this.person.usesAAC ?? false,
      usesSignLanguage: this.person.usesSignLanguage ?? false,
      interestsAndMotivators: this.person.interestsAndMotivators ?? '',
      learningStyle: this.person.learningStyle ?? '',
      availableResources: this.person.availableResources ?? '',
      additionalTherapies: this.person.additionalTherapies ?? '',
      requiresLargeFont: this.person.requiresLargeFont ?? false,
      requiresHighContrast: this.person.requiresHighContrast ?? false,
      visualNoiseSensitivity: this.person.visualNoiseSensitivity ?? false,
      soundSensitivity: this.person.soundSensitivity ?? false,
    };
    this.isEditing = true;
  }

  cancelEditing(): void {
    this.isEditing = false;
  }

  saveProfile(): void {
    if (!this.person) return;
    this.isSaving = true;

    const request: UpdatePersonRequest = {
      attentionLevel: this.editData.attentionLevel || undefined,
      communicationLevel: this.editData.communicationLevel || undefined,
      motorSkillLevel: this.editData.motorSkillLevel || undefined,
      usesAAC: this.editData.usesAAC,
      usesSignLanguage: this.editData.usesSignLanguage,
      interestsAndMotivators: this.editData.interestsAndMotivators || undefined,
      learningStyle: this.editData.learningStyle || undefined,
      availableResources: this.editData.availableResources || undefined,
      additionalTherapies: this.editData.additionalTherapies || undefined,
      requiresLargeFont: this.editData.requiresLargeFont,
      requiresHighContrast: this.editData.requiresHighContrast,
      visualNoiseSensitivity: this.editData.visualNoiseSensitivity,
      soundSensitivity: this.editData.soundSensitivity,
    };

    this.personsService.updatePerson(this.person.id, request).subscribe({
      next: (person) => {
        this.person = person;
        this.isEditing = false;
        this.isSaving = false;
        this.toastService.success('Perfil funcional actualizado');
      },
      error: () => {
        this.isSaving = false;
        this.toastService.error('Error al actualizar el perfil');
      },
    });
  }

  loadSkillProfile(): void {
    if (!this.person) return;
    this.personsService.getSkillProfile(this.person.id).subscribe({
      next: (data) => (this.skillProfile = data ?? []),
    });
  }

  openAddSkillAreaModal(): void {
    this.skillAreaError = '';
    this.selectedSkillAreaId = null;
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
  }

  confirmAddSkillArea(): void {
    if (!this.person || !this.selectedSkillAreaId) return;
    this.skillAreaLoading = true;
    this.personsService.addSkillArea(this.person.id, this.selectedSkillAreaId).subscribe({
      next: () => {
        this.skillAreaLoading = false;
        this.showAddSkillAreaModal = false;
        this.loadSkillProfile();
        this.toastService.success('Area de habilidad agregada');
      },
      error: (err) => {
        this.skillAreaLoading = false;
        this.skillAreaError = err?.error?.message ?? 'Error al agregar';
      },
    });
  }

  deactivateSkillArea(areaId: number): void {
    if (!this.person) return;
    this.personsService.deactivateSkillArea(this.person.id, areaId).subscribe({
      next: () => {
        this.loadSkillProfile();
        this.toastService.success('Area de habilidad removida');
      },
    });
  }

  formatDate = formatDate;

  formatLevel(level: number | null | undefined): string {
    return level != null && level > 0 ? `${level} / 5` : 'Sin evaluar';
  }

  formatBoolean(value: boolean): string {
    return value ? 'Si' : 'No';
  }

  goBack(): void {
    this.router.navigate(['/pro/persons']);
  }
}
