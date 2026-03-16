import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CatalogsService, PersonsService } from '@services';
import { CatalogItem, AutonomyLevelItem, LoginMethodItem, PersonResponse, UpdatePersonRequest } from '../../../../models';
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
    birthDate: ['', [Validators.required, EditComponent.validDate, EditComponent.notFutureDate]],
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
        next: (response) => {
          this.person = response.data;
          this.patchForm(response.data);
        },
        error: () => this.router.navigate(['/admin/persons']),
      });
    }
  }

  static validDate(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const regex = /^\d{2}\/\d{2}\/\d{4}$/;
    if (!regex.test(control.value)) return { invalidDate: true };
    const [day, month, year] = control.value.split('/').map(Number);
    const date = new Date(year, month - 1, day);
    if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
      return { invalidDate: true };
    }
    return null;
  }

  static notFutureDate(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const regex = /^\d{2}\/\d{2}\/\d{4}$/;
    if (!regex.test(control.value)) return null;
    const [day, month, year] = control.value.split('/').map(Number);
    const date = new Date(year, month - 1, day);
    if (date > new Date()) return { futureDate: true };
    return null;
  }

  private patchForm(p: PersonResponse): void {
    this.form.patchValue({
      firstName: p.firstName,
      lastName: p.lastName,
      birthDate: this.toDisplayDate(p.birthDate),
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

  private toDisplayDate(iso: string | undefined | null): string {
    if (!iso) return '';
    const d = new Date(iso);
    if (isNaN(d.getTime())) return '';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid || !this.person) return;

    const raw = this.form.value;
    const request: UpdatePersonRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      birthDate: this.toIsoDate(raw.birthDate),
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

  private toIsoDate(ddmmyyyy: string): string {
    const [day, month, year] = ddmmyyyy.split('/');
    return `${year}-${month}-${day}T00:00:00`;
  }

  goBack(): void {
    if (this.person) {
      this.router.navigate(['/admin/persons', this.person.id]);
    } else {
      this.router.navigate(['/admin/persons']);
    }
  }
}
