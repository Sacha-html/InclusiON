import { Component, inject, OnInit, signal, HostListener, ElementRef } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FamilyService, PersonsService } from '@services';
import {
  FamilyResponse, LinkedPersonInfo, UpdateFamilyRequest, PersonListItemResponse,
} from '../../../../models';
import {
  BadgeComponent, ButtonDirective, CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, FormControlDirective, FormFeedbackComponent, FormLabelDirective,
  FormSelectDirective, RowComponent, SpinnerComponent,
} from '@coreui/angular';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-family-edit',
  imports: [
    ReactiveFormsModule, FormsModule, CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective, SpinnerComponent,
    BadgeComponent, ConfirmModalComponent,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly familyService = inject(FamilyService);
  private readonly personsService = inject(PersonsService);
  private readonly elRef = inject(ElementRef);

  family: FamilyResponse | null = null;
  submitted = false;
  serverError = '';

  // Form principal (datos del familiar)
  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.maxLength(20)]],
    phone: ['', [Validators.maxLength(20)]],
  });

  get f() { return this.form.controls; }

  // Combobox de persona
  persons = signal<PersonListItemResponse[]>([]);
  filteredPersons = signal<PersonListItemResponse[]>([]);
  isLoadingPersons = signal(true);
  selectedPerson = signal<PersonListItemResponse | null>(null);
  personSearch = '';
  personDropdownOpen = false;

  // Vincular persona
  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];
  linkRelationship = '';
  linkIsPrimary = false;
  isLinking = false;
  linkError = '';

  // Desvincular persona
  showUnlinkModal = false;
  unlinkingPerson: LinkedPersonInfo | null = null;
  isUnlinking = false;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

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
          });
          this.loadPersons();
        },
        error: () => this.router.navigate(['/admin/family']),
      });
    }
  }

  // --- Combobox de persona ---

  loadPersons(): void {
    this.personsService.getPersons({ page: 1, pageSize: 200, isActive: true }).subscribe({
      next: (response) => {
        this.persons.set(response.data);
        this.filterAvailablePersons();
        this.isLoadingPersons.set(false);
      },
      error: () => this.isLoadingPersons.set(false),
    });
  }

  private filterAvailablePersons(searchTerm = ''): void {
    const linkedIds = new Set(this.family?.linkedPersons?.map(lp => lp.personId) ?? []);
    let available = this.persons().filter(p => !linkedIds.has(p.id));

    if (searchTerm.trim()) {
      const lower = searchTerm.toLowerCase();
      available = available.filter(p =>
        p.fullName?.toLowerCase().includes(lower) ||
        p.documentNumber?.toLowerCase().includes(lower)
      );
    }

    this.filteredPersons.set(available);
  }

  onPersonInputFocus(): void {
    this.personSearch = '';
    this.filterAvailablePersons();
    this.personDropdownOpen = true;
  }

  onPersonSearch(term: string): void {
    this.personSearch = term;
    this.personDropdownOpen = true;
    this.filterAvailablePersons(term);
  }

  selectPerson(person: PersonListItemResponse): void {
    this.selectedPerson.set(person);
    this.personSearch = person.fullName ?? '';
    this.personDropdownOpen = false;
  }

  clearPerson(): void {
    this.selectedPerson.set(null);
    this.personSearch = '';
    this.filterAvailablePersons();
  }

  closeDropdown(): void {
    this.personDropdownOpen = false;
    const sel = this.selectedPerson();
    this.personSearch = sel ? (sel.fullName ?? '') : '';
  }

  // --- Vincular persona ---

  linkPerson(): void {
    const person = this.selectedPerson();
    if (!person || !this.linkRelationship || !this.family || this.isLinking) return;

    this.isLinking = true;
    this.linkError = '';

    this.familyService.linkFamilyToPerson(this.family.id, person.id, {
      relationship: this.linkRelationship,
      isPrimary: this.linkIsPrimary,
    }).subscribe({
      next: () => {
        this.clearPerson();
        this.linkRelationship = '';
        this.linkIsPrimary = false;
        this.isLinking = false;
        this.refreshFamily();
      },
      error: (err) => {
        this.linkError = err?.error?.message || 'Error al vincular la persona';
        this.isLinking = false;
      },
    });
  }

  // --- Desvincular persona ---

  openUnlinkModal(person: LinkedPersonInfo): void {
    this.unlinkingPerson = person;
    this.showUnlinkModal = true;
  }

  confirmUnlink(observation: string): void {
    if (!this.unlinkingPerson || !this.family || this.isUnlinking) return;

    this.isUnlinking = true;

    this.familyService.unlinkFamilyFromPerson(
      this.family.id,
      this.unlinkingPerson.personId,
      observation,
    ).subscribe({
      next: () => {
        this.isUnlinking = false;
        this.showUnlinkModal = false;
        this.unlinkingPerson = null;
        this.refreshFamily();
      },
      error: () => {
        this.isUnlinking = false;
      },
    });
  }

  // --- Guardar datos del familiar ---

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

  private refreshFamily(): void {
    if (!this.family) return;
    this.familyService.getFamilyById(this.family.id).subscribe({
      next: (data) => {
        this.family = data;
        this.filterAvailablePersons();
      },
    });
  }
}
