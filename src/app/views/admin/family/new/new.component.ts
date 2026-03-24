import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FamilyService } from '@services';
import { CreateFamilyRequest, FamilyResponse } from '../../../../models';
import {
  ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent,
} from '@coreui/angular';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';

@Component({
  selector: 'app-family-new',
  imports: [
    ReactiveFormsModule, CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective,
    PasswordModalComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly familyService = inject(FamilyService);

  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdFamily: FamilyResponse | null = null;

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.maxLength(20)]],
    phone: ['', [Validators.maxLength(20)]],
    relationship: [''],
  });

  get f() { return this.form.controls; }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    if (this.form.invalid) return;

    const raw = this.form.value;
    const request: CreateFamilyRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      ...(raw.phone && { phone: raw.phone }),
      ...(raw.relationship && { relationship: raw.relationship }),
    };

    this.familyService.createFamily(request).subscribe({
      next: (response) => {
        this.createdFamily = response;
        this.showPasswordModal = true;
      },
      error: (err) => {
        this.serverError = err?.error?.message || 'Error al crear el familiar';
      },
    });
  }

  closeModalAndNavigate(): void {
    this.showPasswordModal = false;
    if (this.createdFamily) {
      this.router.navigate(['/admin/family', this.createdFamily.id]);
    }
  }

  goBack(): void {
    this.router.navigate(['/admin/family']);
  }
}
