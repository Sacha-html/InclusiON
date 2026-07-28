import { Component, Input, OnChanges, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { PersonsService, ToastService } from '@services';
import { PersonResponse } from '@models';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormLabelDirective,
  FormSelectDirective,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-accessibility',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonDirective,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormLabelDirective,
    FormSelectDirective,
    SpinnerComponent,
  ],
  template: `
<c-card class="mb-3">
  <c-card-header>
    <strong>Ajustes de Accesibilidad</strong>
  </c-card-header>
  <c-card-body>
    <p class="text-body-secondary mb-4">
      Estas preferencias se aplican automáticamente cuando la persona inicia sesión en su interfaz AAC.
    </p>

    <form [formGroup]="form">
      <!-- Color blindness select -->
      <div class="mb-4">
        <label cLabel for="proColorBlindnessType" class="fw-semibold mb-1">Tipo de daltonismo</label>
        <select cSelect id="proColorBlindnessType" formControlName="colorBlindnessType" style="max-width: 280px;">
          <option value="">Ninguno</option>
          <option value="deuteranopia">Deuteranopia (rojo-verde, más común)</option>
          <option value="protanopia">Protanopia (rojo-verde)</option>
          <option value="tritanopia">Tritanopia (azul-amarillo)</option>
        </select>
      </div>

      <div class="d-flex flex-column gap-3">
        <c-form-check>
          <input cFormCheckInput type="checkbox" id="proRequiresHighContrast"
                 formControlName="requiresHighContrast" />
          <label cFormCheckLabel for="proRequiresHighContrast" class="fw-medium">
            Alto contraste
            <small class="d-block text-body-secondary fw-normal">
              Activa el perfil de máximo contraste en la interfaz
            </small>
          </label>
        </c-form-check>

        <c-form-check>
          <input cFormCheckInput type="checkbox" id="proRequiresLargeFont"
                 formControlName="requiresLargeFont" />
          <label cFormCheckLabel for="proRequiresLargeFont" class="fw-medium">
            Fuente grande
            <small class="d-block text-body-secondary fw-normal">
              Aumenta el tamaño de texto en la interfaz
            </small>
          </label>
        </c-form-check>

        <c-form-check>
          <input cFormCheckInput type="checkbox" id="proVisualNoiseSensitivity"
                 formControlName="visualNoiseSensitivity" />
          <label cFormCheckLabel for="proVisualNoiseSensitivity" class="fw-medium">
            Sensibilidad al ruido visual
            <small class="d-block text-body-secondary fw-normal">
              Reduce animaciones y elementos visuales complejos
            </small>
          </label>
        </c-form-check>

        <c-form-check>
          <input cFormCheckInput type="checkbox" id="proSoundSensitivity"
                 formControlName="soundSensitivity" />
          <label cFormCheckLabel for="proSoundSensitivity" class="fw-medium">
            Sensibilidad al sonido
            <small class="d-block text-body-secondary fw-normal">
              Desactiva alertas sonoras y texto a voz automático
            </small>
          </label>
        </c-form-check>
      </div>
    </form>
  </c-card-body>
</c-card>

<div class="d-flex justify-content-end gap-2">
  <button cButton color="secondary" variant="outline" (click)="reset()" [disabled]="isSaving()">
    Restaurar
  </button>
  <button cButton color="primary" (click)="save()" [disabled]="isSaving()">
    @if (isSaving()) {
      <c-spinner size="sm" class="me-1" aria-hidden="true"></c-spinner>
      Guardando...
    } @else {
      Guardar cambios
    }
  </button>
</div>
  `,
})
export class ProfessionalAccessibilityComponent implements OnChanges {
  @Input({ required: true }) person!: PersonResponse;

  private readonly personsService = inject(PersonsService);
  private readonly toastService   = inject(ToastService);
  private readonly fb             = inject(FormBuilder);

  isSaving = signal(false);

  form: FormGroup = this.fb.group({
    requiresLargeFont:      [false],
    requiresHighContrast:   [false],
    visualNoiseSensitivity: [false],
    soundSensitivity:       [false],
    colorBlindnessType:     [''],
  });

  private readonly defaults = {
    requiresLargeFont:      false,
    requiresHighContrast:   false,
    visualNoiseSensitivity: false,
    soundSensitivity:       false,
    colorBlindnessType:     '',
  };

  ngOnChanges(): void {
    if (this.person) {
      this.form.patchValue({
        requiresLargeFont:      this.person.requiresLargeFont,
        requiresHighContrast:   this.person.requiresHighContrast,
        visualNoiseSensitivity: this.person.visualNoiseSensitivity,
        soundSensitivity:       this.person.soundSensitivity,
        colorBlindnessType:     this.person.colorBlindnessType ?? '',
      });
    }
  }

  save(): void {
    this.isSaving.set(true);
    const val = this.form.value;

    this.personsService.updateAccessibility(this.person.id, {
      requiresLargeFont:      val.requiresLargeFont,
      requiresHighContrast:   val.requiresHighContrast,
      visualNoiseSensitivity: val.visualNoiseSensitivity,
      soundSensitivity:       val.soundSensitivity,
      colorBlindnessType:     val.colorBlindnessType || null,
    }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toastService.success('Configuración de accesibilidad guardada');
      },
      error: () => {
        this.isSaving.set(false);
        this.toastService.error('Error al guardar la configuración');
      },
    });
  }

  reset(): void {
    this.form.patchValue(this.defaults);
    this.toastService.info('Valores restaurados. Haz clic en Guardar para confirmar.');
  }
}
