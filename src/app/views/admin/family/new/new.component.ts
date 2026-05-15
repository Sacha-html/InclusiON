import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FamilyService, PersonsService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { CreateFamilyRequest, FamilyResponse, PersonListItemResponse } from '../../../../models';
import {
  ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent,
} from '@coreui/angular';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';
import { SearchableSelectComponent } from '@shared/components/searchable-select/searchable-select.component';
import { map } from 'rxjs';

@Component({
  selector: 'app-family-new',
  imports: [
    ReactiveFormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective,
    PasswordModalComponent, SearchableSelectComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent {
  private readonly fb            = inject(FormBuilder);
  private readonly router        = inject(Router);
  private readonly familyService = inject(FamilyService);
  private readonly personsService = inject(PersonsService);

  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdFamily: FamilyResponse | null = null;

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.minLength(6), Validators.maxLength(20), Validators.pattern(/^[a-zA-Z0-9]+$/)]],
    phone: ['', [Validators.maxLength(20)]],
    relationship: ['', [Validators.required]],
    personId: [null, [Validators.required]],
  });

  readonly searchPersonsFn = (query: string) =>
    this.personsService.getPersons({ search: query, pageSize: 20, isActive: true }).pipe(
      map(r => r.data)
    );
  readonly displayPerson = (p: PersonListItemResponse) => p.fullName ?? '';
  readonly subDisplayPerson = (p: PersonListItemResponse) => p.disabilityTypeName ?? '';
  readonly valueFromPerson = (p: PersonListItemResponse) => p.id;

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
      personId: raw.personId,
      ...(raw.documentNumber && { documentNumber: raw.documentNumber }),
      ...(raw.phone && { phone: raw.phone }),
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
