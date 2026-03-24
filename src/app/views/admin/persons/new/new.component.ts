import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CatalogsService, PersonsService } from '@services';
import { CatalogItem, AutonomyLevelItem, LoginMethodItem, CreatePersonRequest } from '../../../../models';
import { validDate, notFutureDate, toIsoDate } from '@shared/utils';
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
  selector: 'app-new',
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
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);

  submitted = false;
  serverError = '';

  disabilityTypes: CatalogItem[] = [];
  autonomyLevels: AutonomyLevelItem[] = [];
  loginMethods: LoginMethodItem[] = [];

  form: FormGroup = this.fb.group({
    // Datos personales
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    documentNumber: ['', [Validators.maxLength(20)]],
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
    loginMethodId: [null],
    pin: ['', [Validators.pattern(/^\d{4}$/)]],
    avatarColor: ['#2196F3'],
  });

  get f() {
    return this.form.controls;
  }

  get selectedLoginMethod(): LoginMethodItem | undefined {
    const id = this.form.get('loginMethodId')?.value;
    return id ? this.loginMethods.find(m => m.id === +id) : undefined;
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
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid) return;

    const raw = this.form.value;
    const request: CreatePersonRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      birthDate: toIsoDate(raw.birthDate),
      usesAAC: raw.usesAAC ?? false,
      usesSignLanguage: raw.usesSignLanguage ?? false,
      requiresLargeFont: raw.requiresLargeFont ?? false,
      requiresHighContrast: raw.requiresHighContrast ?? false,
      visualNoiseSensitivity: raw.visualNoiseSensitivity ?? false,
      soundSensitivity: raw.soundSensitivity ?? false,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      ...(raw.disabilityTypeId && { disabilityTypeId: +raw.disabilityTypeId }),
      ...(raw.attentionLevel && { attentionLevel: +raw.attentionLevel }),
      ...(raw.communicationLevel && { communicationLevel: +raw.communicationLevel }),
      ...(raw.motorSkillLevel && { motorSkillLevel: +raw.motorSkillLevel }),
      ...(raw.interestsAndMotivators && { interestsAndMotivators: raw.interestsAndMotivators }),
      ...(raw.learningStyle && { learningStyle: raw.learningStyle }),
      ...(raw.availableResources && { availableResources: raw.availableResources }),
      ...(raw.additionalTherapies && { additionalTherapies: raw.additionalTherapies }),
      ...(raw.autonomyLevelId && { autonomyLevelId: +raw.autonomyLevelId }),
      ...(raw.loginMethodId && { loginMethodId: +raw.loginMethodId }),
      ...(raw.pin && { pin: raw.pin }),
      ...(raw.avatarColor && { avatarColor: raw.avatarColor }),
    };

    this.personsService.createPerson(request).subscribe({
      next: (person) => {
        this.router.navigate(['/admin/persons', person.id]);
      },
      error: (err) => {
        this.serverError = err?.error?.message || 'Error al crear la persona';
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/persons']);
  }
}
