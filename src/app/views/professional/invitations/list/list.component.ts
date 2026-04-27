import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InvitationsService, ToastService, ProfessionalsService, AssignmentsService } from '@services';
import { getInvitationStatusColor } from '@shared/utils';
import {
  InvitationResponse,
  CreateInvitationRequest,
  ProfessionalPersonResponse,
} from '@models';

import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  TableDirective,
  ButtonDirective,
  BadgeComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  FormControlDirective,
  FormSelectDirective,
  AlertComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-invitations-list',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    TableDirective,
    ButtonDirective,
    BadgeComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    FormControlDirective,
    FormSelectDirective,
    AlertComponent,
    SpinnerComponent,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly invitationsService = inject(InvitationsService);
  private readonly toastService = inject(ToastService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly fb = inject(FormBuilder);

  invitations: InvitationResponse[] = [];
  persons: ProfessionalPersonResponse[] = [];
  isLoading = true;
  isSubmitting = false;
  showModal = false;
  errorMessage = '';
  invitationForm!: FormGroup;
  createdInvitationCode = '';
  sortBy = 'createdAt';
  sortDirection: 'ASC' | 'DESC' = 'DESC';

  readonly relationships = [
    'Madre',
    'Padre',
    'Tutor/a',
    'Abuelo/a',
    'Hermano/a',
    'Tio/a',
    'Otro',
  ];

  ngOnInit(): void {
    this.initForm();
    this.loadInvitations();
    this.loadPersons();
  }

  private initForm(): void {
    this.invitationForm = this.fb.group({
      personId: [''],
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', [Validators.maxLength(100)]],
      lastName: ['', [Validators.maxLength(100)]],
      relationship: [''],
    });
  }

  private loadInvitations(): void {
    this.isLoading = true;
    this.invitationsService.getAll().subscribe({
      next: (data) => {
        const sorted = [...data].sort((a, b) => {
          let aVal: any = a[this.sortBy as keyof InvitationResponse];
          let bVal: any = b[this.sortBy as keyof InvitationResponse];
          if (typeof aVal === 'string') aVal = aVal?.toLowerCase() || '';
          if (typeof bVal === 'string') bVal = bVal?.toLowerCase() || '';
          const direction = this.sortDirection === 'ASC' ? 1 : -1;
          if (aVal < bVal) return -1 * direction;
          if (aVal > bVal) return 1 * direction;
          return 0;
        });
        this.invitations = sorted;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar invitaciones');
      },
    });
  }

  onSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'ASC' ? 'DESC' : 'ASC';
    } else {
      this.sortBy = column;
      this.sortDirection = 'DESC';
    }
    this.loadInvitations();
  }

  private loadPersons(): void {
    this.professionalsService.getMyProfile().subscribe({
      next: (profile) => {
        this.assignmentsService.getPersonsByProfessional(profile.id).subscribe({
          next: (persons) => {
            this.persons = persons.filter(p => p.isActive);
          },
          error: () => {
            // Non-critical, persons dropdown will be empty
          },
        });
      },
      error: () => {
        // Non-critical
      },
    });
  }

  openModal(): void {
    this.invitationForm.reset();
    this.errorMessage = '';
    this.createdInvitationCode = '';
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (this.invitationForm.invalid) {
      Object.keys(this.invitationForm.controls).forEach((key) => {
        this.invitationForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isSubmitting = true;
    const values = this.invitationForm.value;

    const request: CreateInvitationRequest = {
      email: values.email.trim(),
      firstName: values.firstName?.trim() || undefined,
      lastName: values.lastName?.trim() || undefined,
      relationship: values.relationship || undefined,
      personId: values.personId || undefined,
    };

    this.invitationsService.create(request).subscribe({
      next: (invitation) => {
        this.isSubmitting = false;
        this.createdInvitationCode = invitation.code;
        this.toastService.success('Invitacion creada exitosamente');
        this.loadInvitations();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = error.userMessage || 'Error al crear la invitacion';
      },
    });
  }

  getStatusColor = getInvitationStatusColor;

  getInviteUrl(code: string): string {
    return `${window.location.origin}/#/invite/${code}`;
  }

  copyToClipboard(code: string): void {
    navigator.clipboard.writeText(this.getInviteUrl(code)).then(() => {
      this.toastService.success('Link copiado al portapapeles');
    });
  }

  get f() {
    return this.invitationForm.controls;
  }

  hasError(fieldName: string, errorType: string): boolean {
    const field = this.invitationForm.get(fieldName);
    return !!(field?.hasError(errorType) && field?.touched);
  }
}
