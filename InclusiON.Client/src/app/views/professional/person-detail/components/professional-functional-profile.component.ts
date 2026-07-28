import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PersonsService, ToastService } from '@services';
import { PersonResponse, UpdatePersonRequest } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  ColComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormControlDirective,
  FormLabelDirective,
  FormSelectDirective,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-functional-profile',
  standalone: true,
  imports: [
    BadgeComponent,
    ButtonDirective,
    ColComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormControlDirective,
    FormLabelDirective,
    FormSelectDirective,
    RowComponent,
    SpinnerComponent,
    FormsModule,
  ],
  templateUrl: './professional-functional-profile.component.html',
})
export class ProfessionalFunctionalProfileComponent {
  @Input({ required: true }) person!: PersonResponse;
  @Output() personChange = new EventEmitter<PersonResponse>();

  private readonly personsService = inject(PersonsService);
  private readonly toastService = inject(ToastService);

  isEditing = signal(false);
  isSaving = signal(false);

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

  /** Percentage of informational profile fields filled (7 total). */
  get profileCompletion(): number {
    const p = this.person;
    const filled = [
      (p.attentionLevel ?? 0) > 0,
      (p.communicationLevel ?? 0) > 0,
      (p.motorSkillLevel ?? 0) > 0,
      !!(p.interestsAndMotivators?.trim()),
      !!(p.learningStyle?.trim()),
      !!(p.availableResources?.trim()),
      !!(p.additionalTherapies?.trim()),
    ].filter(Boolean).length;
    return Math.round((filled / 7) * 100);
  }

  get profileCompletionColor(): string {
    const pct = this.profileCompletion;
    if (pct >= 80) return 'success';
    if (pct >= 40) return 'warning';
    return 'danger';
  }

  levels = [
    { value: 0, label: 'Sin evaluar' },
    { value: 1, label: '1 - Muy bajo' },
    { value: 2, label: '2 - Bajo' },
    { value: 3, label: '3 - Medio' },
    { value: 4, label: '4 - Alto' },
    { value: 5, label: '5 - Muy alto' },
  ];

  formatLevel(level: number | null | undefined): string {
    return level != null && level > 0 ? `${level} / 5` : 'Sin evaluar';
  }

  formatBoolean(value: boolean): string {
    return value ? 'Si' : 'No';
  }

  startEditing(): void {
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
    this.isEditing.set(true);
  }

  cancel(): void {
    this.isEditing.set(false);
  }

  save(): void {
    this.isSaving.set(true);
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
        this.personChange.emit(person);
        this.isEditing.set(false);
        this.isSaving.set(false);
        this.toastService.success('Perfil funcional actualizado');
      },
      error: () => {
        this.isSaving.set(false);
        this.toastService.error('Error al actualizar el perfil');
      },
    });
  }
}
