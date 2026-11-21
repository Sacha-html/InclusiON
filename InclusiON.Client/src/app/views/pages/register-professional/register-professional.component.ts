import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProfessionalsService, InstitutionsService, CatalogsService } from '@services';
import { AppRoutes } from '@shared/constants';
import { InstitutionResponse, RegisterProfessionalRequest, ErrorCode } from '@models';
import { validDate, notFutureDate, minAge, toIsoDate, toInputDate } from '@shared/utils';

import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  FormSelectDirective,
  RowComponent,
  AlertComponent,
  ModalModule,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';
import { OnlyNumbersDirective } from '@shared/directives';

@Component({
  selector: 'app-register-professional',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ContainerComponent,
    CardComponent,
    CardBodyComponent,
    RowComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    FormFeedbackComponent,
    FormSelectDirective,
    ButtonDirective,
    AlertComponent,
    ModalModule,
    IconDirective,
    AccessibilityPanelComponent,
    OnlyNumbersDirective,
  ],
  templateUrl: './register-professional.component.html',
  styleUrl: './register-professional.component.scss',
})
export class RegisterProfessionalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly destroyRef = inject(DestroyRef);

  specialties: { id: number; name: string }[] = [];

  readonly minBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 100, today.getMonth(), today.getDate()));
  })();

  readonly maxBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 18, today.getMonth(), today.getDate()));
  })();

  institutions: InstitutionResponse[] = [];
  submitted = false;
  serverError = '';
  isLoading = false;
  showSuccessModal = false;

  form: FormGroup;

  constructor() {
    const fb = inject(FormBuilder);

    this.form = fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      documentNumber: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(8), Validators.pattern(/^[0-9]+$/)]],
      phone: ['', [Validators.maxLength(20), Validators.pattern(/^[0-9]+$/)]],
      specialtyId: ['', [Validators.required]],
      licenseNumber: ['', [Validators.required, Validators.maxLength(50)]],
      birthDate: ['', [Validators.required, validDate, notFutureDate, minAge(18)]],
      institutionId: [''],
    });
  }

  get f() {
    return this.form.controls;
  }

  showFieldError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control || !control.invalid) return false;
    if (this.submitted) return true;
    if (control.errors?.['emailExists'] || control.errors?.['documentExists'] || control.errors?.['licenseExists']) return true;
    return control.touched;
  }

  ngOnInit(): void {
    this.catalogsService.getSpecialties()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: data => this.specialties = data.filter(specialty => specialty.isActive === true),
        error: () => this.specialties = [],
      });
    this.institutionsService.getAll().subscribe({
      next: (data) => this.institutions = data.filter(i => i.isActive),
      error: () => this.institutions = [],
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    this.form.markAllAsTouched();
    this.attemptSubmit();
  }

  private attemptSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading = true;

    const raw = this.form.value;
    const request: RegisterProfessionalRequest = {
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim(),
      email: raw.email.trim(),
      documentNumber: raw.documentNumber ? raw.documentNumber.trim() : undefined,
      phone: raw.phone ? raw.phone.trim() : undefined,
      specialtyId: +raw.specialtyId,
      specialty: this.specialties.find(x => x.id === +raw.specialtyId)?.name ?? '',
      licenseNumber: raw.licenseNumber ? raw.licenseNumber.trim() : undefined,
      birthDate: raw.birthDate ? toIsoDate(raw.birthDate) : undefined,
      institutionId: raw.institutionId ? +raw.institutionId : undefined,
    };

    this.professionalsService.registerProfessional(request).subscribe({
      next: () => {
        this.form.reset();
        this.submitted = false;
        this.isLoading = false;
        this.showSuccessModal = true;
      },
      error: (err) => {
        this.isLoading = false;
        this.serverError = err?.userMessage || 'Error al enviar la solicitud de registro.';
        if (err?.errorCode === ErrorCode.EmailAlreadyExists) {
          this.f['email'].setErrors({ emailExists: true, message: this.serverError });
        } else if (err?.errorCode === ErrorCode.DocumentAlreadyExists) {
          this.f['documentNumber'].setErrors({ documentExists: true, message: this.serverError });
        } else if (err?.errorCode === ErrorCode.LicenseNumberAlreadyExists) {
          this.f['licenseNumber'].setErrors({ licenseExists: true, message: this.serverError });
        }
      },
    });
  }

  goToLogin(): void {
    this.router.navigate([AppRoutes.AdminLogin], { queryParams: { role: 'professional' } });
  }
}
