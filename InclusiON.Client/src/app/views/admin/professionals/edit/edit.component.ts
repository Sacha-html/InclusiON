import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProfessionalsService, CatalogsService } from '@services';
import { AppRoutes } from '@shared/constants';
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

  professional: ProfessionalResponse | null = null;
  submitted = false;
  serverError = '';

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    documentNumber: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(8), Validators.pattern(/^[0-9]+$/)]],
    phone: ['', [Validators.maxLength(20)]],
    specialtyId: ['', [Validators.required]],
    licenseNumber: ['', [Validators.required, Validators.maxLength(50)]],
    birthDate: ['', [Validators.required, validDate, notFutureDate, minAge(18)]],
  });

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.catalogsService.getSpecialties()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(data => {
        this.specialties = data.filter(specialty => specialty.isActive === true);
      });
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
    if (p.specialtyId && !this.specialties.some(x => x.id === p.specialtyId) && p.specialty) {
      this.specialties = [{ id: p.specialtyId, name: `${p.specialty} (inactiva)` }, ...this.specialties];
    }
    this.form.patchValue({
      firstName: p.firstName,
      lastName: p.lastName,
      documentNumber: p.documentNumber ?? '',
      phone: p.phone ?? '',
      specialtyId: p.specialtyId ?? '',
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
      ...(raw.specialtyId && { specialtyId: +raw.specialtyId, specialty: this.specialties.find(x => x.id === +raw.specialtyId)?.name }),
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
