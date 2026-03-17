import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProfessionalsService } from '@services';
import { ProfessionalResponse, UpdateProfessionalRequest } from '../../../../models';
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
    birthDate: ['', [Validators.required, EditComponent.validDate, EditComponent.notFutureDate]],
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
      birthDate: this.toDisplayDate(p.birthDate),
      address: p.address ?? '',
    });
  }

  private toDisplayDate(iso: string | undefined | null): string {
    if (!iso) return '';
    const d = new Date(iso);
    if (isNaN(d.getTime())) return '';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
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
      ...(raw.birthDate && { birthDate: this.toIsoDate(raw.birthDate) }),
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

  private toIsoDate(ddmmyyyy: string): string {
    const [day, month, year] = ddmmyyyy.split('/');
    return `${year}-${month}-${day}T00:00:00`;
  }

  goBack(): void {
    if (this.professional) {
      this.router.navigate(['/admin/professionals', this.professional.id]);
    } else {
      this.router.navigate(['/admin/professionals']);
    }
  }
}
