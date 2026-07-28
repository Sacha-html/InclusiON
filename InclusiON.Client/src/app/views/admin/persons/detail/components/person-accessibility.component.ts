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
  selector: 'app-person-accessibility',
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
  templateUrl: './person-accessibility.component.html',
})
export class PersonAccessibilityComponent implements OnChanges {
  @Input({ required: true }) person!: PersonResponse;

  private readonly personsService = inject(PersonsService);
  private readonly toastService   = inject(ToastService);
  private readonly fb             = inject(FormBuilder);

  isSaving = signal(false);

  form: FormGroup = this.fb.group({
    requiresLargeFont:       [false],
    requiresHighContrast:    [false],
    visualNoiseSensitivity:  [false],
    soundSensitivity:        [false],
    colorBlindnessType:      [''],
  });

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

    this.personsService.updateAccessibilityConfig(this.person.id, {
      requiresLargeFont:      val.requiresLargeFont,
      requiresHighContrast:   val.requiresHighContrast,
      visualNoiseSensitivity: val.visualNoiseSensitivity,
      soundSensitivity:       val.soundSensitivity,
      colorBlindnessType:     val.colorBlindnessType ?? '',
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
}
