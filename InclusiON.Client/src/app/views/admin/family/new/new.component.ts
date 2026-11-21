import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FamilyService, PersonsService, ValidationService } from '@services';
import { AppRoutes, RELATIONSHIPS } from '@shared/constants';
import { CreateFamilyRequest, FamilyResponse, PersonListItemResponse } from '@models';
import { uniqueDocumentValidator, uniqueEmailValidator } from '@shared/utils';
import {
  ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent,
} from '@coreui/angular';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';
import { SearchableSelectComponent } from '@shared/components/searchable-select/searchable-select.component';
import { OnlyNumbersDirective } from '@shared/directives';
import { filter, map, take } from 'rxjs';

@Component({
  selector: 'app-family-new',
  imports: [
    ReactiveFormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective,
    PasswordModalComponent, SearchableSelectComponent, OnlyNumbersDirective,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly familyService = inject(FamilyService);
  private readonly personsService = inject(PersonsService);
  private readonly validationService = inject(ValidationService);

  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdFamily: FamilyResponse | null = null;

  readonly relationships = RELATIONSHIPS;

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: [
      '',
      [Validators.required, Validators.email],
      [uniqueEmailValidator(email => this.validationService.checkEmail(email))],
    ],
    documentNumber: [
      '',
      [Validators.required, Validators.minLength(7), Validators.maxLength(8), Validators.pattern(/^[0-9]+$/)],
      [uniqueDocumentValidator(doc => this.validationService.checkDocument(doc))],
    ],
    phone: ['', [Validators.maxLength(20), Validators.pattern(/^[0-9]+$/)]],
    relationship: ['', [Validators.required]],
    personId: [null, [Validators.required]],
  });

  showFieldError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    if (!control || !control.invalid) return false;
    if (control.errors?.['emailExists'] || control.errors?.['documentExists']) return true;
    return this.submitted || control.touched;
  }

  readonly searchPersonsFn = (query: string) =>
    this.personsService.getPersons({ search: query, pageSize: 20 }).pipe(
      map(r => r.data)
    );
  readonly displayPerson = (p: PersonListItemResponse) => p.fullName ?? '';
  readonly subDisplayPerson = (p: PersonListItemResponse) => {
    const dni = p.documentNumber ? `DNI: ${p.documentNumber}` : 'Sin DNI';
    const status = !p.isActive ? ' · Inactivo/Suspendido' : '';
    return `${p.disabilityTypeName ?? 'Sin tipo'} (${dni}${status})`;
  };
  readonly valueFromPerson = (p: PersonListItemResponse) => p.id;

  get f() { return this.form.controls; }

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
    if (this.form.invalid) return;

    const raw = this.form.value;
    const request: CreateFamilyRequest = {
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim(),
      email: raw.email.trim(),
      personId: raw.personId,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber.trim() }),
      ...(raw.phone && { phone: raw.phone.trim() }),
      relationship: raw.relationship,
    };

    this.familyService.createFamily(request).subscribe({
      next: (response) => {
        this.createdFamily = response;
        this.showPasswordModal = true;
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al crear el familiar';
      },
    });
  }

  closeModalAndNavigate(): void {
    this.showPasswordModal = false;
    if (this.createdFamily) {
      this.router.navigate([AppRoutes.Admin.Family, this.createdFamily.id]);
    }
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Admin.Family]);
  }
}
