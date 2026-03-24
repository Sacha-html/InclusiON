import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CatalogsService, PersonsService } from '@services';
import { CatalogItem, AutonomyLevelItem, LoginMethodItem, PersonResponse, UpdatePersonRequest } from '../../../../models';
import { validDate, notFutureDate, toIsoDate, toDisplayDate } from '@shared/utils';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormSelectDirective,
  RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-edit',
  imports: [
    ReactiveFormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    FormFeedbackComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormSelectDirective,
    ButtonDirective,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);

  person: PersonResponse | null = null;
  submitted = false;
  serverError = '';

  disabilityTypes: CatalogItem[] = [];
  autonomyLevels: AutonomyLevelItem[] = [];
  loginMethods: LoginMethodItem[] = [];

  form: FormGroup = this.fb.group({
    // Datos personales
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    birthDate: ['', [Validators.required, validDate, notFutureDate]],
    // Discapacidad
    disabilityTypeId: [null],
    // Perfil funcional
    attentionLevel: [null],
    communicationLevel: [null],
    motorSkillLevel: [null],
    usesAAC: [false],
    usesSignLanguage: [false],
    // Preferencias
    interestsAndMotivators: [''],
    learningStyle: [''],
    availableResources: [''],
    additionalTherapies: [''],
    // Accesibilidad
    requiresLargeFont: [false],
    requiresHighContrast: [false],
    visualNoiseSensitivity: [false],
    soundSensitivity: [false],
    // Configuración de acceso
    autonomyLevelId: [null],
    avatarColor: ['#2196F3'],
  });

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.catalogsService.getDisabilityTypes().subscribe({
      next: (data) => this.disabilityTypes = data,
    });
    this.catalogsService.getAutonomyLevels().subscribe({
      next: (data) => this.autonomyLevels = data,
    });
    this.catalogsService.getLoginMethods().subscribe({
      next: (data) => this.loginMethods = data,
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (person) => {
          this.person = person;
          this.patchForm(person);
        },
        error: () => this.router.navigate(['/admin/persons']),
      });
    }
  }

  private patchForm(p: PersonResponse): void {
    this.form.patchValue({
      firstName: p.firstName,
      lastName: p.lastName,
      birthDate: toDisplayDate(p.birthDate),
      disabilityTypeId: p.disabilityTypeId ?? null,
      attentionLevel: p.attentionLevel ?? null,
      communicationLevel: p.communicationLevel ?? null,
      motorSkillLevel: p.motorSkillLevel ?? null,
      usesAAC: p.usesAAC,
      usesSignLanguage: p.usesSignLanguage,
      interestsAndMotivators: p.interestsAndMotivators ?? '',
      learningStyle: p.learningStyle ?? '',
      availableResources: p.availableResources ?? '',
      additionalTherapies: p.additionalTherapies ?? '',
      requiresLargeFont: p.requiresLargeFont,
      requiresHighContrast: p.requiresHighContrast,
      visualNoiseSensitivity: p.visualNoiseSensitivity,
      soundSensitivity: p.soundSensitivity,
      autonomyLevelId: p.autonomyLevelId ?? null,
      avatarColor: p.avatarColor ?? '#2196F3',
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid || !this.person) return;

    const raw = this.form.value;
    const request: UpdatePersonRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      birthDate: toIsoDate(raw.birthDate),
      usesAAC: raw.usesAAC ?? false,
      usesSignLanguage: raw.usesSignLanguage ?? false,
      requiresLargeFont: raw.requiresLargeFont ?? false,
      requiresHighContrast: raw.requiresHighContrast ?? false,
      visualNoiseSensitivity: raw.visualNoiseSensitivity ?? false,
      soundSensitivity: raw.soundSensitivity ?? false,
      ...(raw.disabilityTypeId && { disabilityTypeId: +raw.disabilityTypeId }),
      ...(raw.attentionLevel && { attentionLevel: +raw.attentionLevel }),
      ...(raw.communicationLevel && { communicationLevel: +raw.communicationLevel }),
      ...(raw.motorSkillLevel && { motorSkillLevel: +raw.motorSkillLevel }),
      ...(raw.interestsAndMotivators && { interestsAndMotivators: raw.interestsAndMotivators }),
      ...(raw.learningStyle && { learningStyle: raw.learningStyle }),
      ...(raw.availableResources && { availableResources: raw.availableResources }),
      ...(raw.additionalTherapies && { additionalTherapies: raw.additionalTherapies }),
      ...(raw.autonomyLevelId && { autonomyLevelId: +raw.autonomyLevelId }),
      ...(raw.avatarColor && { avatarColor: raw.avatarColor }),
    };

    this.personsService.updatePerson(this.person.id, request).subscribe({
      next: () => {
        this.router.navigate(['/admin/persons', this.person!.id]);
      },
      error: (err) => {
        this.serverError = err?.error?.message || 'Error al actualizar la persona';
      },
    });
  }

  goBack(): void {
    if (this.person) {
      this.router.navigate(['/admin/persons', this.person.id]);
    } else {
      this.router.navigate(['/admin/persons']);
    }
  }
}
