import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { InstitutionsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { InstitutionResponse, UpdateInstitutionRequest } from '../../../../models';
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
    ButtonDirective,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly institutionsService = inject(InstitutionsService);
  private readonly toastService = inject(ToastService);

  institution: InstitutionResponse | null = null;
  submitted = false;
  serverError = '';

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    address: ['', [Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(20)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
  });

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.institutionsService.getAll().subscribe({
        next: (data) => {
          this.institution = data.find((i) => i.id.toString() === id) ?? null;
          if (this.institution) {
            this.patchForm(this.institution);
          } else {
            this.router.navigate([AppRoutes.Admin.Institutions]);
          }
        },
        error: () => this.router.navigate([AppRoutes.Admin.Institutions]),
      });
    }
  }

  private patchForm(inst: InstitutionResponse): void {
    this.form.patchValue({
      name: inst.name,
      address: inst.address ?? '',
      phone: inst.phone ?? '',
      email: inst.email ?? '',
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid || !this.institution) return;

    const raw = this.form.value;
    const request: UpdateInstitutionRequest = {
      name: raw.name,
      ...(raw.address && { address: raw.address }),
      ...(raw.phone && { phone: raw.phone }),
      ...(raw.email && { email: raw.email }),
    };

    this.institutionsService.update(this.institution.id.toString(), request).subscribe({
      next: () => {
        this.toastService.success('Institucion actualizada exitosamente');
        this.router.navigate([AppRoutes.Admin.Institutions]);
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al actualizar la institucion';
      },
    });
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Institutions]);
  }
}
