import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FamilyService, PersonsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
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
    ReactiveFormsModule, FormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent, FormControlDirective, FormLabelDirective,
    FormFeedbackComponent, FormSelectDirective, ButtonDirective, SpinnerComponent,
    BadgeComponent, ConfirmModalComponent,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly fb             = inject(FormBuilder);
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private readonly familyService  = inject(FamilyService);
  private readonly personsService = inject(PersonsService);
  private readonly toastService   = inject(ToastService);

  family: FamilyResponse | null = null;
  submitted = false;
  serverError = '';

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', [Validators.minLength(6), Validators.maxLength(20), Validators.pattern(/^[a-zA-Z0-9]+$/)]],
    phone: ['', [Validators.maxLength(20)]],
  });

  get f() { return this.form.controls; }

  persons          = signal<PersonListItemResponse[]>([]);
  isLoadingPersons = signal(true);

  // Selección para vincular
  selectedPersonForLink: PersonListItemResponse | null = null;
  searchPersonText = '';
  filteredPersons: PersonListItemResponse[] = [];
  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];
  readonly PARENT_RELATIONSHIPS = ['Madre', 'Padre'];
  linkRelationship = '';
  linkIsPrimary    = false;
  isLinking        = false;
  linkError        = '';

  // Desvincular
  showUnlinkModal  = false;
  unlinkingPerson: LinkedPersonInfo | null = null;
  isUnlinking      = false;

  get availablePersons(): PersonListItemResponse[] {
    const linkedIds = new Set(this.family?.linkedPersons?.map(lp => lp.personId) ?? []);
    return this.persons().filter(p => !linkedIds.has(p.id));
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
        error: () => this.router.navigate([AppRoutes.Admin.Family]),
      });
    }
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

  // --- Búsqueda de persona ---

  filterPersons(text: string): void {
    if (!text) { this.filteredPersons = []; return; }
    const lower = text.toLowerCase();
    this.filteredPersons = this.availablePersons.filter(p =>
      (p.fullName?.toLowerCase().includes(lower) ||
       p.documentNumber?.toLowerCase().includes(lower)) ?? false
    );
  }

  selectPerson(p: PersonListItemResponse): void {
    this.selectedPersonForLink = p;
    this.searchPersonText = '';
    this.filteredPersons = [];
  }

  clearSelectedPerson(): void {
    this.selectedPersonForLink = null;
    this.searchPersonText = '';
    this.filteredPersons = [];
  }

  validateParentLimit(): string {
    if (!this.linkRelationship) return '';
    if (!this.PARENT_RELATIONSHIPS.includes(this.linkRelationship)) return '';
    const exists = this.family?.linkedPersons?.some(
      lp => lp.relationship === this.linkRelationship
    );
    return exists
      ? `Ya existe una persona vinculada con la relación "${this.linkRelationship}". Solo puede haber una.`
      : '';
  }

  // --- Vincular persona ---

  linkPerson(): void {
    const person = this.selectedPersonForLink;
    if (!person || !this.linkRelationship || !this.family || this.isLinking) return;

    this.isLinking = true;
    this.linkError = '';

    this.familyService.linkFamilyToPerson(this.family.id, person.id, {
      relationship: this.linkRelationship,
      isPrimary: this.linkIsPrimary,
    }).subscribe({
      next: () => {
        this.clearSelectedPerson();
        this.linkRelationship = '';
        this.linkIsPrimary = false;
        this.isLinking = false;
        this.refreshFamily();
      },
      error: (err) => {
        this.linkError = err?.userMessage || 'Error al vincular la persona';
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
        this.showUnlinkModal = false;
        this.toastService.error('Error al desvincular la persona');
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
        this.router.navigate([AppRoutes.Admin.Family, this.family!.id]);
      },
      error: (err) => {
        this.serverError = err?.userMessage || 'Error al actualizar el familiar';
      },
    });
  }

  goBack(): void {
    if (this.family) {
      this.router.navigate([AppRoutes.Admin.Family, this.family.id]);
    } else {
      this.router.navigate([AppRoutes.Admin.Family]);
    }
  }

  private refreshFamily(): void {
    if (!this.family) return;
    this.familyService.getFamilyById(this.family.id).subscribe({
      next: (data) => { this.family = data; },
    });
  }
}
