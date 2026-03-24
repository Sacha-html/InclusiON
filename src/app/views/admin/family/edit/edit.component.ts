import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FamilyService } from '@services';
import { FamilyResponse, UpdateFamilyRequest } from '../../../../models';
import {
  ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-family-edit',
  imports: [
    ReactiveFormsModule, CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly familyService = inject(FamilyService);

  family: FamilyResponse | null = null;
  submitted = false;
  serverError = '';

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

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.familyService.getFamilyById(id).subscribe({
        next: (data) => {
          this.family = data;
          this.form.patchValue({
            firstName: data.firstName,
            lastName: data.lastName,
            email: data.email ?? '',
            documentNumber: data.documentNumber ?? '',
            phone: data.phone ?? '',
            relationship: data.relationship ?? '',
          });
        },
        error: () => this.router.navigate(['/admin/family']),
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    if (this.form.invalid || !this.family) return;

    const raw = this.form.value;
    const request: UpdateFamilyRequest = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      ...(raw.phone && { phone: raw.phone }),
      ...(raw.relationship && { relationship: raw.relationship }),
    };

    this.familyService.updateFamily(this.family.id, request).subscribe({
      next: () => {
        this.router.navigate(['/admin/family', this.family!.id]);
      },
      error: (err) => {
        this.serverError = err?.error?.message || 'Error al actualizar el familiar';
      },
    });
  }

  goBack(): void {
    if (this.family) {
      this.router.navigate(['/admin/family', this.family.id]);
    } else {
      this.router.navigate(['/admin/family']);
    }
  }
}
