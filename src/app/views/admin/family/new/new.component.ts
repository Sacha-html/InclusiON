import { Component, inject, OnInit, signal, HostListener, ElementRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FamilyService, PersonsService } from '@services';
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
    ReactiveFormsModule, CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective, SpinnerComponent,
    PasswordModalComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly fb           = inject(FormBuilder);
  private readonly router       = inject(Router);
  private readonly familyService = inject(FamilyService);
  private readonly personsService = inject(PersonsService);
  private readonly elRef        = inject(ElementRef);

  submitted = false;
  serverError = '';
  showPasswordModal = false;
  createdFamily: FamilyResponse | null = null;

  // Combobox de persona
  persons           = signal<PersonListItemResponse[]>([]);
  filteredPersons   = signal<PersonListItemResponse[]>([]);
  isLoadingPersons  = signal(true);
  selectedPerson    = signal<PersonListItemResponse | null>(null);
  personSearch      = '';
  personDropdownOpen = false;

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.maxLength(20)]],
    phone: ['', [Validators.maxLength(20)]],
    relationship: [''],
    personId: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.personsService.getPersons({ page: 1, pageSize: 200, isActive: true }).subscribe({
      next: (response) => {
        this.persons.set(response.data);
        this.filteredPersons.set(response.data);
        this.isLoadingPersons.set(false);
      },
      error: () => this.isLoadingPersons.set(false),
    });
  }

  onPersonInputFocus(): void {
    this.personSearch = '';
    this.filteredPersons.set(this.persons());
    this.personDropdownOpen = true;
  }

  onPersonSearch(term: string): void {
    this.personSearch = term;
    this.personDropdownOpen = true;
    if (!term.trim()) {
      this.filteredPersons.set(this.persons());
      return;
    }
    const lower = term.toLowerCase();
    this.filteredPersons.set(
      this.persons().filter(p =>
        p.fullName?.toLowerCase().includes(lower) ||
        p.documentNumber?.toLowerCase().includes(lower)
      )
    );
  }

  selectPerson(person: PersonListItemResponse): void {
    this.selectedPerson.set(person);
    this.form.patchValue({ personId: person.id });
    this.personSearch = person.fullName ?? '';
    this.personDropdownOpen = false;
  }

  clearPerson(): void {
    this.selectedPerson.set(null);
    this.form.patchValue({ personId: '' });
    this.personSearch = '';
    this.filteredPersons.set(this.persons());
  }

  closeDropdown(): void {
    this.personDropdownOpen = false;
    const sel = this.selectedPerson();
    this.personSearch = sel ? (sel.fullName ?? '') : '';
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
        this.serverError = err?.error?.message || 'Error al crear el familiar';
      },
    });
  }

  closeModalAndNavigate(): void {
    this.showPasswordModal = false;
    if (this.createdFamily) {
      this.router.navigate(['/admin/family', this.createdFamily.id]);
    }
  }

  goBack(): void {
    this.router.navigate(['/admin/family']);
  }
}
