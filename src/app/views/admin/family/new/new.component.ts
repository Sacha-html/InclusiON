import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FamilyService, PersonsService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { CreateFamilyRequest, FamilyResponse, PersonListItemResponse } from '../../../../models';
import {
  ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent, SpinnerComponent,
} from '@coreui/angular';
import { PasswordModalComponent } from '@shared/components/password-modal/password-modal.component';

@Component({
  selector: 'app-family-new',
  imports: [
    ReactiveFormsModule, FormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective,
    SpinnerComponent, PasswordModalComponent,
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

  searchPersonText = '';
  filteredPersons: PersonListItemResponse[] = [];
  selectedPersonDisplay: PersonListItemResponse | null = null;

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.minLength(6), Validators.maxLength(20), Validators.pattern(/^[a-zA-Z0-9]+$/)]],
    phone: ['', [Validators.maxLength(20)]],
    relationship: [''],
    personId: [null, [Validators.required]],
  });

  filterPersons(text: string): void {
    if (!text) { this.filteredPersons = []; return; }
    const lower = text.toLowerCase();
    this.filteredPersons = this.persons().filter(p =>
      (p.fullName?.toLowerCase().includes(lower) ||
       p.documentNumber?.toLowerCase().includes(lower)) ?? false
    );
  }

  selectPerson(p: PersonListItemResponse): void {
    this.selectedPersonDisplay = p;
    this.searchPersonText = '';
    this.filteredPersons = [];
    this.form.patchValue({ personId: p.id });
  }

  clearSelectedPerson(): void {
    this.selectedPersonDisplay = null;
    this.searchPersonText = '';
    this.filteredPersons = [];
    this.form.patchValue({ personId: null });
  }

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
