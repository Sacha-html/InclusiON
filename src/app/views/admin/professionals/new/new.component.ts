import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { CreateProfessionalRequest } from '../../../../models';
import { validDate, notFutureDate, toIsoDate, uniqueEmailValidator, uniqueLicenseValidator } from '@shared/utils';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  RowComponent,
} from '@coreui/angular';
import { ProfessionalResponse } from '../../../../models';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';

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
    ButtonDirective,
    PasswordModalComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly professionalsService = inject(ProfessionalsService);

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
      documentNumber: ['', [Validators.maxLength(20)]],
      phone: ['', [Validators.maxLength(20)]],
      specialty: ['', [Validators.maxLength(100)]],
      licenseNumber: ['', [Validators.maxLength(50)], [uniqueLicenseValidator(license => professionalsService.checkLicenseNumber(license))]],
      birthDate: ['', [Validators.required, validDate, notFutureDate]],
    });
  }

  get f() {
    return this.form.controls;
  }

  showFieldError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control) return false;
    if (this.submitted) return true;
    if (control.errors?.['emailExists'] || control.errors?.['licenseExists']) return true;
    return control.touched && control.invalid;
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

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
        this.serverError = err?.error?.message || 'Error al crear el profesional';
      },
    });
  }

  closeModalAndNavigate(): void {
    this.showPasswordModal = false;
    if (this.createdProfessional) {
      this.router.navigate(['/admin/professionals', this.createdProfessional.id]);
    }
  }

  goBack(): void {
    this.router.navigate(['/admin/professionals']);
  }
}
