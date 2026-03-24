import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InstitutionsService, ToastService } from '@services';
import { CreateInstitutionRequest } from '../../../../models';
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
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly toastService = inject(ToastService);

  submitted = false;
  serverError = '';

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    address: ['', [Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(20)]],
    email: ['', [Validators.email, Validators.maxLength(100)]],
  });

  get f() {
    return this.form.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid) return;

    const raw = this.form.value;
    const request: CreateInstitutionRequest = {
      name: raw.name,
      ...(raw.address && { address: raw.address }),
      ...(raw.phone && { phone: raw.phone }),
      ...(raw.email && { email: raw.email }),
    };

    this.institutionsService.create(request).subscribe({
      next: () => {
        this.toastService.success('Institucion creada exitosamente');
        this.router.navigate(['/admin/institutions']);
      },
      error: (err) => {
        this.serverError = err?.error?.message || 'Error al crear la institucion';
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/institutions']);
  }
}
