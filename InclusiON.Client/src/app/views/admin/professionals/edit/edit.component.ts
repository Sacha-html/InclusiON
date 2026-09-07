import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { AppRoutes, SPECIALTIES } from '@shared/constants';
import { ProfessionalResponse, UpdateProfessionalRequest } from '@models';
import { validDate, notFutureDate, minAge, toIsoDate, toInputDate } from '@shared/utils';
import { OnlyNumbersDirective } from '@shared/directives';
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
    FormSelectDirective,
    ButtonDirective,
    OnlyNumbersDirective,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly professionalsService = inject(ProfessionalsService);

  readonly defaultSpecialties = SPECIALTIES;
  specialties: string[] = [...SPECIALTIES];

  readonly minBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 100, today.getMonth(), today.getDate()));
  })();

  readonly maxBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 18, today.getMonth(), today.getDate()));
  })();

  professional: ProfessionalResponse | null = null;
  submitted = false;
  serverError = '';

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    documentNumber: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(8), Validators.pattern(/^[0-9]+$/)]],
    phone: ['', [Validators.maxLength(20)]],
    specialty: ['', [Validators.maxLength(100)]],
    licenseNumber: ['', [Validators.maxLength(50)]],
    birthDate: ['', [Validators.required, validDate, notFutureDate, minAge(18)]],
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
        error: () => this.router.navigate([AppRoutes.Admin.Professionals]),
      });
    }
  }

  private patchForm(p: ProfessionalResponse): void {
    if (p.specialty && !this.specialties.includes(p.specialty)) {
      this.specialties = [p.specialty, ...this.defaultSpecialties];
    }
    this.form.patchValue({
      firstName: p.firstName,
      lastName: p.lastName,
      documentNumber: p.documentNumber ?? '',
      phone: p.phone ?? '',
      specialty: p.specialty ?? '',
      licenseNumber: p.licenseNumber ?? '',
      birthDate: toInputDate(p.birthDate),
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    this.form.markAllAsTouched();

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
    };

    this.professionalsService.updateProfessional(this.professional.id, request).subscribe({
      next: () => {
        this.router.navigate([AppRoutes.Admin.Professionals, this.professional!.id]);
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al actualizar el profesional';
      },
    });
  }

  goBack(): void {
    if (this.professional) {
      this.router.navigate([AppRoutes.Admin.Professionals, this.professional.id]);
    } else {
      this.router.navigate([AppRoutes.Admin.Professionals]);
    }
  }
}
