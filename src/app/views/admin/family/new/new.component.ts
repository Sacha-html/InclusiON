import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { FamilyService, PersonsService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { CreateFamilyRequest, FamilyResponse, PersonListItemResponse } from '../../../../models';
import {
  ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent,
} from '@coreui/angular';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';

@Component({
  selector: 'app-family-new',
  imports: [
    ReactiveFormsModule, NgSelectModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective,
    PasswordModalComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly fb            = inject(FormBuilder);
  private readonly router        = inject(Router);
  private readonly familyService = inject(FamilyService);
  private readonly personsService = inject(PersonsService);

  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdFamily: FamilyResponse | null = null;

  persons          = signal<PersonListItemResponse[]>([]);
  isLoadingPersons = signal(true);

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.maxLength(20)]],
    phone: ['', [Validators.maxLength(20)]],
    relationship: [''],
    personId: [null, [Validators.required]],
  });

  searchPersonFn = (term: string, item: PersonListItemResponse): boolean => {
    const lower = term.toLowerCase();
    return (
      (item.fullName?.toLowerCase().includes(lower) ||
        item.documentNumber?.toLowerCase().includes(lower)) ??
      false
    );
  };

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.personsService.getPersons({ page: 1, pageSize: 200, isActive: true }).subscribe({
      next: (response) => {
        this.persons.set(response.data);
        this.isLoadingPersons.set(false);
      },
      error: () => this.isLoadingPersons.set(false),
    });
  }

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
      ...(raw.relationship && { relationship: raw.relationship }),
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
