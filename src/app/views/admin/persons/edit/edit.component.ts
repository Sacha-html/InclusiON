import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CatalogsService, PersonsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { CatalogItem, AutonomyLevelItem, LoginMethodItem, PersonResponse, UpdateLoginMethodResponse, UpdatePersonRequest } from '@models';
import { validDate, notFutureDate, toIsoDate, toDisplayDate } from '@shared/utils';
import { AvatarColorPickerComponent } from '@shared/components';
import { ChangeLoginMethodModalComponent } from './change-login-method-modal.component';
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
    AvatarColorPickerComponent,
    ChangeLoginMethodModalComponent,
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
  private readonly toastService = inject(ToastService);

  person: PersonResponse | null = null;
  submitted = false;
  serverError = '';
  showLoginMethodModal = false;

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
    colorBlindnessType: [null],
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
        error: () => this.router.navigate([AppRoutes.Admin.Persons]),
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
      colorBlindnessType: p.colorBlindnessType ?? null,
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
      colorBlindnessType: raw.colorBlindnessType ?? null,
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
        this.router.navigate([AppRoutes.Admin.Persons, this.person!.id]);
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al actualizar la persona';
      },
    });
  }

  openLoginMethodModal(): void {
    this.showLoginMethodModal = true;
  }

  closeLoginMethodModal(): void {
    this.showLoginMethodModal = false;
  }

  onLoginMethodUpdated(response: UpdateLoginMethodResponse): void {
    if (this.person) {
      this.person = { ...this.person, loginMethodId: response.loginMethodId, loginMethodName: response.loginMethodName };
    }
    if (response.temporaryPassword) {
      this.toastService.warning(
        'Recordá compartir la contraseña temporal con la persona. Solo se muestra una vez.',
        'Método actualizado'
      );
    } else {
      this.toastService.success(`Ahora la persona ingresa con: ${response.loginMethodName}`);
    }
  }

  goBack(): void {
    if (this.person) {
      this.router.navigate([AppRoutes.Admin.Persons, this.person.id]);
    } else {
      this.router.navigate([AppRoutes.Admin.Persons]);
    }
  }
}
