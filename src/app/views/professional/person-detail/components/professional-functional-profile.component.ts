import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PersonsService, ToastService } from '@services';
import { PersonResponse, UpdatePersonRequest } from '@models';
import {
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
  template: `
    @if (!isEditing()) {
      <div class="d-flex justify-content-end mb-3">
        <button cButton color="primary" size="sm" (click)="startEditing()">Editar perfil</button>
      </div>

      <h6 class="mb-3">Niveles funcionales</h6>
      <c-row class="mb-3">
        <c-col sm="4">
          <label cLabel>Atencion</label>
          <input cFormControl [value]="formatLevel(person.attentionLevel)" readonly />
        </c-col>
        <c-col sm="4">
          <label cLabel>Comunicacion</label>
          <input cFormControl [value]="formatLevel(person.communicationLevel)" readonly />
        </c-col>
        <c-col sm="4">
          <label cLabel>Motricidad</label>
          <input cFormControl [value]="formatLevel(person.motorSkillLevel)" readonly />
        </c-col>
      </c-row>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Usa CAA</label>
          <input cFormControl [value]="formatBoolean(person.usesAAC)" readonly />
        </c-col>
        <c-col sm="6">
          <label cLabel>Usa lengua de señas</label>
          <input cFormControl [value]="formatBoolean(person.usesSignLanguage)" readonly />
        </c-col>
      </c-row>

      <h6 class="mt-4 mb-3">Preferencias</h6>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Intereses y motivadores</label>
          <textarea cFormControl [value]="person.interestsAndMotivators ?? 'Sin especificar'" readonly rows="2"></textarea>
        </c-col>
        <c-col sm="6">
          <label cLabel>Estilo de aprendizaje</label>
          <textarea cFormControl [value]="person.learningStyle ?? 'Sin especificar'" readonly rows="2"></textarea>
        </c-col>
      </c-row>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Recursos disponibles</label>
          <textarea cFormControl [value]="person.availableResources ?? 'Sin especificar'" readonly rows="2"></textarea>
        </c-col>
        <c-col sm="6">
          <label cLabel>Terapias adicionales</label>
          <textarea cFormControl [value]="person.additionalTherapies ?? 'Sin especificar'" readonly rows="2"></textarea>
        </c-col>
      </c-row>

      <h6 class="mt-4 mb-3">Accesibilidad</h6>
      <c-row class="mb-3">
        <c-col sm="3"><label cLabel>Fuente grande</label><input cFormControl [value]="formatBoolean(person.requiresLargeFont)" readonly /></c-col>
        <c-col sm="3"><label cLabel>Alto contraste</label><input cFormControl [value]="formatBoolean(person.requiresHighContrast)" readonly /></c-col>
        <c-col sm="3"><label cLabel>Ruido visual</label><input cFormControl [value]="formatBoolean(person.visualNoiseSensitivity)" readonly /></c-col>
        <c-col sm="3"><label cLabel>Sonido</label><input cFormControl [value]="formatBoolean(person.soundSensitivity)" readonly /></c-col>
      </c-row>

    } @else {
      <h6 class="mb-3">Niveles funcionales</h6>
      <c-row class="mb-3">
        <c-col sm="4">
          <label cLabel>Atencion</label>
          <select cSelect [(ngModel)]="editData.attentionLevel">
            @for (l of levels; track l.value) { <option [ngValue]="l.value">{{ l.label }}</option> }
          </select>
        </c-col>
        <c-col sm="4">
          <label cLabel>Comunicacion</label>
          <select cSelect [(ngModel)]="editData.communicationLevel">
            @for (l of levels; track l.value) { <option [ngValue]="l.value">{{ l.label }}</option> }
          </select>
        </c-col>
        <c-col sm="4">
          <label cLabel>Motricidad</label>
          <select cSelect [(ngModel)]="editData.motorSkillLevel">
            @for (l of levels; track l.value) { <option [ngValue]="l.value">{{ l.label }}</option> }
          </select>
        </c-col>
      </c-row>
      <c-row class="mb-3">
        <c-col sm="6">
          <c-form-check>
            <input cFormCheckInput type="checkbox" id="edit-usesAAC" [(ngModel)]="editData.usesAAC" />
            <label cFormCheckLabel for="edit-usesAAC">Usa CAA</label>
          </c-form-check>
        </c-col>
        <c-col sm="6">
          <c-form-check>
            <input cFormCheckInput type="checkbox" id="edit-usesSignLanguage" [(ngModel)]="editData.usesSignLanguage" />
            <label cFormCheckLabel for="edit-usesSignLanguage">Usa lengua de señas</label>
          </c-form-check>
        </c-col>
      </c-row>

      <h6 class="mt-4 mb-3">Preferencias</h6>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Intereses y motivadores</label>
          <textarea cFormControl [(ngModel)]="editData.interestsAndMotivators" rows="2"></textarea>
        </c-col>
        <c-col sm="6">
          <label cLabel>Estilo de aprendizaje</label>
          <textarea cFormControl [(ngModel)]="editData.learningStyle" rows="2"></textarea>
        </c-col>
      </c-row>
      <c-row class="mb-3">
        <c-col sm="6">
          <label cLabel>Recursos disponibles</label>
          <textarea cFormControl [(ngModel)]="editData.availableResources" rows="2"></textarea>
        </c-col>
        <c-col sm="6">
          <label cLabel>Terapias adicionales</label>
          <textarea cFormControl [(ngModel)]="editData.additionalTherapies" rows="2"></textarea>
        </c-col>
      </c-row>

      <h6 class="mt-4 mb-3">Accesibilidad</h6>
      <c-row class="mb-3">
        <c-col sm="3">
          <c-form-check><input cFormCheckInput type="checkbox" id="edit-largeFont" [(ngModel)]="editData.requiresLargeFont" /><label cFormCheckLabel for="edit-largeFont">Fuente grande</label></c-form-check>
        </c-col>
        <c-col sm="3">
          <c-form-check><input cFormCheckInput type="checkbox" id="edit-highContrast" [(ngModel)]="editData.requiresHighContrast" /><label cFormCheckLabel for="edit-highContrast">Alto contraste</label></c-form-check>
        </c-col>
        <c-col sm="3">
          <c-form-check><input cFormCheckInput type="checkbox" id="edit-visualNoise" [(ngModel)]="editData.visualNoiseSensitivity" /><label cFormCheckLabel for="edit-visualNoise">Ruido visual</label></c-form-check>
        </c-col>
        <c-col sm="3">
          <c-form-check><input cFormCheckInput type="checkbox" id="edit-sound" [(ngModel)]="editData.soundSensitivity" /><label cFormCheckLabel for="edit-sound">Sonido</label></c-form-check>
        </c-col>
      </c-row>

      <div class="d-flex gap-2 mt-3">
        <button cButton color="primary" (click)="save()" [disabled]="isSaving()">
          @if (isSaving()) { <c-spinner size="sm" class="me-1"></c-spinner> }
          Guardar
        </button>
        <button cButton color="secondary" (click)="cancel()">Cancelar</button>
      </div>
    }
  `,
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
