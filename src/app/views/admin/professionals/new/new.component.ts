import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { CreateProfessionalRequest } from '../../../../models';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
} from '@coreui/angular';
import { ProfessionalResponse } from '../../../../models';

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
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
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

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.maxLength(20)]],
    phone: ['', [Validators.maxLength(20)]],
    specialty: ['', [Validators.maxLength(100)]],
    licenseNumber: ['', [Validators.maxLength(50)]],
    birthDate: ['', [Validators.required, NewComponent.validDate, NewComponent.notFutureDate]],
    address: ['', [Validators.maxLength(200)]],
  });

  get f() {
    return this.form.controls;
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

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid) return;

    const raw = this.form.value;
    const request: CreateProfessionalRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      ...(raw.phone && { phone: raw.phone }),
      ...(raw.specialty && { specialty: raw.specialty }),
      ...(raw.licenseNumber && { licenseNumber: raw.licenseNumber }),
      ...(raw.birthDate && { birthDate: this.toIsoDate(raw.birthDate) }),
      ...(raw.address && { address: raw.address }),
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

  private toIsoDate(ddmmyyyy: string): string {
    const [day, month, year] = ddmmyyyy.split('/');
    return `${year}-${month}-${day}T00:00:00`;
  }

  goBack(): void {
    this.router.navigate(['/admin/professionals']);
  }
}
