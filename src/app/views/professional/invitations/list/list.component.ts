import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { switchMap } from 'rxjs';
import { InvitationsService, ToastService, ProfessionalsService, AssignmentsService } from '@services';
import {
  InvitationResponse,
  CreateInvitationRequest,
  ProfessionalPersonResponse,
} from '@models';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn, HeaderButton } from '@shared/components/data-table/data-table.models';
import {
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
    ReactiveFormsModule,
    DataTableComponent,
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
  private readonly toastService       = inject(ToastService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly fb                 = inject(FormBuilder);

  invitations: InvitationResponse[] = [];
  persons: ProfessionalPersonResponse[] = [];
  isLoading         = true;
  isSubmitting      = false;
  showModal         = false;
  errorMessage      = '';
  invitationForm!: FormGroup;
  createdInvitationCode = '';
  sortBy            = 'createdAt';
  sortDirection: 'ASC' | 'DESC' = 'DESC';
  currentPage       = 1;
  readonly pageSize = 10;
  totalRecords      = 0;

  readonly relationships = ['Madre', 'Padre', 'Tutor/a', 'Abuelo/a', 'Hermano/a', 'Tio/a', 'Otro'];

  readonly columns: TableColumn[] = [
    { key: 'email',        label: 'Email',      sortable: true },
    { key: 'fullName',     label: 'Nombre',     sortable: true },
    { key: 'relationship', label: 'Parentesco', sortable: true },
    { key: 'personName',   label: 'Persona',    sortable: true },
    {
      key: 'status', label: 'Estado', type: 'badge', sortable: true,
      badgeMap: {
        'Enviada':  { color: 'info',    label: 'Enviada'  },
        'Aceptada': { color: 'success', label: 'Aceptada' },
        'Expirada': { color: 'danger',  label: 'Expirada' },
      },
    },
    { key: 'createdAtFmt', label: 'Fecha', sortable: true },
    {
      key: '', label: 'Acciones', type: 'actions',
      actions: [
        {
          action: 'copy',
          label: 'Copiar link',
          color: 'light',
          visible: (item) => item.status === 'Enviada',
        },
      ],
    },
  ];

  readonly headerButtons: HeaderButton[] = [
    { action: 'create', label: 'Agregar', color: 'primary' },
  ];

  ngOnInit(): void {
    this.initForm();
    this.loadInvitations();
    this.loadPersons();
  }

  private initForm(): void {
    this.invitationForm = this.fb.group({
      personId:     [''],
      email:        ['', [Validators.required, Validators.email]],
      firstName:    ['', [Validators.required, Validators.maxLength(100)]],
      lastName:     ['', [Validators.required, Validators.maxLength(100)]],
      relationship: [''],
    });
  }

  private loadInvitations(): void {
    this.isLoading = true;
    this.invitationsService.getAll(this.currentPage, this.pageSize).subscribe({
      next: (data) => {
        const sortField = this.sortBy === 'createdAtFmt' ? 'createdAt' : this.sortBy;
        const sorted = [...data.data].sort((a, b) => {
          let aVal: any = a[sortField as keyof InvitationResponse];
          let bVal: any = b[sortField as keyof InvitationResponse];
          if (typeof aVal === 'string') aVal = aVal?.toLowerCase() ?? '';
          if (typeof bVal === 'string') bVal = bVal?.toLowerCase() ?? '';
          const dir = this.sortDirection === 'ASC' ? 1 : -1;
          return aVal < bVal ? -dir : aVal > bVal ? dir : 0;
        });

        this.invitations = sorted.map(inv => ({
          ...inv,
          fullName:     [inv.firstName, inv.lastName].filter(Boolean).join(' ') || '-',
          relationship: inv.relationship || '-',
          personName:   inv.personName   || '-',
          createdAtFmt: inv.createdAt
            ? new Date(inv.createdAt).toLocaleDateString('es-AR', {
                day: '2-digit', month: '2-digit', year: 'numeric',
              })
            : '-',
        }));

        this.totalRecords = data.totalRecords;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar invitaciones');
      },
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadInvitations();
  }

  onSortAction(event: { sortBy: string; sortDirection: 'ASC' | 'DESC' }): void {
    this.sortBy       = event.sortBy;
    this.sortDirection = event.sortDirection;
    this.currentPage  = 1;
    this.loadInvitations();
  }

  onHeaderAction(action: string): void {
    if (action === 'create') this.openModal();
  }

  onRowAction(event: { action: string; item: any }): void {
    if (event.action === 'copy') this.copyToClipboard(event.item.code);
  }

  private loadPersons(): void {
    this.professionalsService.getMyProfile().pipe(
      switchMap(profile => this.assignmentsService.getPersonsByProfessional(profile.id))
    ).subscribe({
      next: (persons) => { this.persons = persons.filter(p => p.isActive); },
      error: () => { this.toastService.error('Error al cargar personas'); },
    });
  }

  openModal(): void {
    this.invitationForm.reset();
    this.errorMessage         = '';
    this.createdInvitationCode = '';
    this.showModal            = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  onSubmit(): void {
    this.errorMessage = '';
    if (this.invitationForm.invalid) {
      Object.keys(this.invitationForm.controls).forEach(key =>
        this.invitationForm.get(key)?.markAsTouched()
      );
      return;
    }

    this.isSubmitting = true;
    const values = this.invitationForm.value;
    const request: CreateInvitationRequest = {
      email:        values.email.trim(),
      firstName:    values.firstName.trim(),
      lastName:     values.lastName.trim(),
      relationship: values.relationship       || undefined,
      personId:     values.personId           || undefined,
    };

    this.invitationsService.create(request).subscribe({
      next: (invitation) => {
        this.isSubmitting         = false;
        this.createdInvitationCode = invitation.code;
        this.toastService.success('Invitacion creada exitosamente');
        this.currentPage = 1;
        this.loadInvitations();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = error.userMessage || 'Error al crear la invitacion';
      },
    });
  }

  getInviteUrl(code: string): string {
    return `${window.location.origin}/#/invite/${code}`;
  }

  copyToClipboard(code: string): void {
    navigator.clipboard.writeText(this.getInviteUrl(code)).then(() => {
      this.toastService.success('Link copiado al portapapeles');
    });
  }

  get f() { return this.invitationForm.controls; }

  hasError(fieldName: string, errorType: string): boolean {
    const field = this.invitationForm.get(fieldName);
    return !!(field?.hasError(errorType) && field?.touched);
  }
}
