import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { ProfessionalResponse, UpdateProfessionalRequest } from '../../../../models';
import { validDate, notFutureDate, toIsoDate, toDisplayDate } from '@shared/utils';
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
  private readonly professionalsService = inject(ProfessionalsService);

  professional: ProfessionalResponse | null = null;
  submitted = false;
  serverError = '';

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    documentNumber: ['', [Validators.maxLength(20)]],
    phone: ['', [Validators.maxLength(20)]],
    specialty: ['', [Validators.maxLength(100)]],
    licenseNumber: ['', [Validators.maxLength(50)]],
    birthDate: ['', [Validators.required, validDate, notFutureDate]],
    address: ['', [Validators.maxLength(200)]],
  });

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.professionalsService.getProfessionalById(id).subscribe({
        next: (data) => {
          this.professional = data;
          this.patchForm(data);
        },
        error: () => this.router.navigate(['/admin/professionals']),
      });
    }
  }

  private patchForm(p: ProfessionalResponse): void {
    this.form.patchValue({
      firstName: p.firstName,
      lastName: p.lastName,
      documentNumber: p.documentNumber ?? '',
      phone: p.phone ?? '',
      specialty: p.specialty ?? '',
      licenseNumber: p.licenseNumber ?? '',
      birthDate: toDisplayDate(p.birthDate),
      address: p.address ?? '',
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';

    if (this.form.invalid || !this.professional) return;

    const raw = this.form.value;
    const request: UpdateProfessionalRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      ...(raw.phone && { phone: raw.phone }),
      ...(raw.specialty && { specialty: raw.specialty }),
      ...(raw.licenseNumber && { licenseNumber: raw.licenseNumber }),
      ...(raw.birthDate && { birthDate: toIsoDate(raw.birthDate) }),
      ...(raw.address && { address: raw.address }),
    };

    this.professionalsService.updateProfessional(this.professional.id, request).subscribe({
      next: () => {
        this.router.navigate(['/admin/professionals', this.professional!.id]);
      },
      error: (err) => {
        this.serverError = err?.error?.message || 'Error al actualizar el profesional';
      },
    });
  }

  goBack(): void {
    if (this.professional) {
      this.router.navigate(['/admin/professionals', this.professional.id]);
    } else {
      this.router.navigate(['/admin/professionals']);
    }
  }
}
