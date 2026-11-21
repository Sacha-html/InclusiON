import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { filter, take } from 'rxjs';
import { PersonsService, ValidationService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { PersonResponse, UpdatePersonRequest } from '@models';
import { validDate, notFutureDate, ageRangeValidator, toIsoDate, toInputDate, uniqueDocumentValidator } from '@shared/utils';
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
    OnlyNumbersDirective,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly validationService = inject(ValidationService);

  person: PersonResponse | null = null;
  submitted = false;
  serverError = '';

  readonly minBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 40, today.getMonth(), today.getDate()));
  })();

  readonly maxBirthDate: string = (() => {
    const today = new Date();
    return toInputDate(new Date(today.getFullYear() - 12, today.getMonth(), today.getDate()));
  })();

  form: FormGroup = this.fb.group({
    // Datos personales
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    documentNumber: [
      '',
      [Validators.required, Validators.minLength(7), Validators.maxLength(8), Validators.pattern(/^[0-9]+$/)],
      [uniqueDocumentValidator(
        doc => this.validationService.checkDocument(doc, this.person?.id),
        () => this.person?.documentNumber,
      )],
    ],
    birthDate: ['', [Validators.required, validDate, notFutureDate, ageRangeValidator(12, 40)]],
  });

  get f() {
    return this.form.controls;
  }

  showFieldError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control || !control.invalid) return false;
    if (control.errors?.['documentExists']) return true;
    return this.submitted || control.touched;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (person) => {
          this.person = person;
          this.patchForm(person);
        },
        error: () => this.router.navigate([AppRoutes.Admin.Persons]),
      });
    }
  }

  private patchForm(p: PersonResponse): void {
    this.form.patchValue({
      firstName: p.firstName,
      lastName: p.lastName,
      documentNumber: p.documentNumber ?? '',
      birthDate: toInputDate(p.birthDate),
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.serverError = '';
    this.form.markAllAsTouched();

    if (this.form.pending) {
      this.form.statusChanges.pipe(filter(s => s !== 'PENDING'), take(1)).subscribe(() => {
        this.attemptSubmit();
      });
      return;
    }

    this.attemptSubmit();
  }

  private attemptSubmit(): void {
    if (this.form.invalid || !this.person) return;

    const raw = this.form.value;
    const request: UpdatePersonRequest = {
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim(),
      ...(raw.documentNumber && { documentNumber: raw.documentNumber.trim() }),
      birthDate: toIsoDate(raw.birthDate),
    };

    this.personsService.updatePerson(this.person.id, request).subscribe({
      next: () => {
        this.router.navigate([AppRoutes.Admin.Persons, this.person!.id]);
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al actualizar el alumno';
      },
    });
  }

  goBack(): void {
    if (this.person) {
      this.router.navigate([AppRoutes.Admin.Persons, this.person.id]);
    } else {
      this.router.navigate([AppRoutes.Admin.Persons]);
    }
  }
}
