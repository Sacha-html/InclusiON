import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { AppRoutes, SPECIALTIES } from '@shared/constants';
import { CreateProfessionalRequest } from '@models';
import { validDate, notFutureDate, minAge, toIsoDate, toInputDate, uniqueEmailValidator, uniqueLicenseValidator } from '@shared/utils';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  FormSelectDirective,
  RowComponent,
} from '@coreui/angular';
import { ProfessionalResponse } from '@models';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';
import { OnlyNumbersDirective } from '@shared/directives';

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
    FormSelectDirective,
    ButtonDirective,
    PasswordModalComponent,
    OnlyNumbersDirective,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly professionalsService = inject(ProfessionalsService);

  readonly specialties = SPECIALTIES;

  readonly minBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 100, today.getMonth(), today.getDate()));
  })();

  readonly maxBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 18, today.getMonth(), today.getDate()));
  })();

  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdProfessional: ProfessionalResponse | null = null;

  form: FormGroup;

  constructor() {
    const fb = inject(FormBuilder);
    const professionalsService = inject(ProfessionalsService);

    this.form = fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email], [uniqueEmailValidator(email => professionalsService.checkEmail(email))]],
      documentNumber: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(8), Validators.pattern(/^[0-9]+$/)]],
      phone: ['', [Validators.maxLength(20)]],
      specialty: ['', [Validators.maxLength(100)]],
      licenseNumber: ['', [Validators.maxLength(50)], [uniqueLicenseValidator(license => professionalsService.checkLicenseNumber(license))]],
      birthDate: ['', [Validators.required, validDate, notFutureDate, minAge(18)]],
    });
  }

  get f() {
    return this.form.controls;
  }

  showFieldError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control || !control.invalid) return false;
    if (this.submitted) return true;
    if (control.errors?.['emailExists'] || control.errors?.['licenseExists']) return true;
    return control.touched;
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    this.form.markAllAsTouched();

    if (this.form.pending) {
      this.form.statusChanges.subscribe(status => {
        if (status !== 'PENDING') {
          this.attemptSubmit();
        }
      });
      return;
    }

    this.attemptSubmit();
  }

  private attemptSubmit(): void {
    if (this.form.invalid) return;

    if (this.f['email'].errors?.['emailExists'] || this.f['licenseNumber'].errors?.['licenseExists']) {
      return;
    }

    const raw = this.form.value;
    const request: CreateProfessionalRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      ...(raw.phone && { phone: raw.phone }),
      ...(raw.specialty && { specialty: raw.specialty }),
      ...(raw.licenseNumber && { licenseNumber: raw.licenseNumber }),
      ...(raw.birthDate && { birthDate: toIsoDate(raw.birthDate) }),
    };

    this.professionalsService.createProfessional(request).subscribe({
      next: (response) => {
        this.createdProfessional = response;
        this.showPasswordModal = true;
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al crear el profesional';
      },
    });
  }

  closeModalAndNavigate(): void {
    this.showPasswordModal = false;
    if (this.createdProfessional) {
      this.router.navigate([AppRoutes.Admin.Professionals, this.createdProfessional.id]);
    }
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Professionals]);
  }
}
