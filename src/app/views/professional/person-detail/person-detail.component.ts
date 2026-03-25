import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { DiagnosesService } from '../../../services/diagnoses.service';
import {
  DiagnosisListItemResponse,
  DiagnosisResponse,
} from '../../../models/responses/diagnosis.response';
import { CreateDiagnosisRequest } from '../../../models/requests/diagnoses/create-diagnosis.request';
import {
  PersonResponse,
  PersonSkillProfileResponse,
  SkillAreaItem,
  UpdatePersonRequest,
} from '@models';
import { formatDate, toDisplayDate, toIsoDate } from '@shared/utils';
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
  selector: 'app-person-detail',
  standalone: true,
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
    DatePipe,
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
  private readonly diagnosesService = inject(DiagnosesService);

  person: PersonResponse | null = null;
  activeTab: 'datos' | 'funcional' | 'habilidades' | 'diagnosticos' = 'datos';

  // Diagnoses
  diagnoses: DiagnosisListItemResponse[] = [];
  showDiagnosisModal = false;
  editingDiagnosis: DiagnosisResponse | null = null;
  diagnosisForm: CreateDiagnosisRequest = this.emptyDiagnosisForm();

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
  selectedSkillAreaIds: Set<number> = new Set();
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
      birthDate: this.editPersonalData.birthDate
        ? toIsoDate(this.editPersonalData.birthDate)
        : undefined,
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
    this.selectedSkillAreaIds = new Set();
    this.catalogsService.getSkillAreas().subscribe({
      next: (areas) => {
        const activeIds = new Set(
          this.skillProfile
            .filter((sp) => sp.isActive)
            .map((sp) => sp.skillAreaId),
        );
        this.allSkillAreas = (areas ?? []).filter((a) => !activeIds.has(a.id));
        this.showAddSkillAreaModal = true;
      },
    });
  }

  closeAddSkillAreaModal(): void {
    this.showAddSkillAreaModal = false;
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
            this.toastService.success(
              `${completed} área(s) de habilidad agregada(s)`,
            );
          }
        },
        error: () => {
          errors++;
          if (completed + errors === ids.length) {
            this.skillAreaLoading = false;
            this.showAddSkillAreaModal = false;
            this.loadSkillProfile();
            if (completed > 0) {
              this.toastService.warning(
                `${completed} agregada(s), ${errors} con error`,
              );
            } else {
              this.toastService.error('Error al agregar áreas de habilidad');
            }
          }
        },
      });
    }
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

  // ── Diagnoses ──────────────────────────────────────────

  loadDiagnoses(): void {
    if (!this.person) return;
    this.diagnosesService.getByPerson(this.person.id).subscribe({
      next: (data) => (this.diagnoses = data),
      error: () => this.toastService.error('Error al cargar diagnósticos'),
    });
  }

  openNewDiagnosis(): void {
    this.editingDiagnosis = null;
    this.diagnosisForm = this.emptyDiagnosisForm();
    this.showDiagnosisModal = true;
  }

  openEditDiagnosis(item: DiagnosisListItemResponse): void {
    this.diagnosesService.getById(item.id).subscribe({
      next: (d) => {
        this.editingDiagnosis = d;
        this.diagnosisForm = {
          diagnosisDate: d.diagnosisDate.substring(0, 10),
          primaryDiagnosis: d.primaryDiagnosis,
          initialObservations: d.initialObservations ?? '',
          identifiedCapabilities: d.identifiedCapabilities ?? '',
          identifiedChallenges: d.identifiedChallenges ?? '',
          requiredSupports: d.requiredSupports ?? '',
          pedagogicalObjectives: d.pedagogicalObjectives ?? '',
          recommendedStrategies: d.recommendedStrategies ?? '',
        };
        this.showDiagnosisModal = true;
      },
      error: () => this.toastService.error('Error al cargar el diagnóstico'),
    });
  }

  saveDiagnosis(): void {
    if (!this.person || !this.diagnosisForm.primaryDiagnosis) return;

    if (this.editingDiagnosis) {
      this.diagnosesService
        .update(this.editingDiagnosis.id, this.diagnosisForm)
        .subscribe({
          next: () => {
            this.toastService.success('Diagnóstico actualizado exitosamente');
            this.showDiagnosisModal = false;
            this.loadDiagnoses();
          },
          error: () =>
            this.toastService.error('Error al actualizar el diagnóstico'),
        });
    } else {
      this.diagnosesService
        .create(this.person.id, this.diagnosisForm)
        .subscribe({
          next: () => {
            this.toastService.success('Diagnóstico creado exitosamente');
            this.showDiagnosisModal = false;
            this.loadDiagnoses();
          },
          error: () => this.toastService.error('Error al crear el diagnóstico'),
        });
    }
  }

  closeDiagnosisModal(): void {
    this.showDiagnosisModal = false;
    this.editingDiagnosis = null;
  }

  private emptyDiagnosisForm(): CreateDiagnosisRequest {
    return {
      diagnosisDate: new Date().toISOString().substring(0, 10),
      primaryDiagnosis: '',
      initialObservations: '',
      identifiedCapabilities: '',
      identifiedChallenges: '',
      requiredSupports: '',
      pedagogicalObjectives: '',
      recommendedStrategies: '',
    };
  }
}
