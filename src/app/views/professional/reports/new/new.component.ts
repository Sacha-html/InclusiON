import { Component, inject, OnInit, signal, HostListener, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportsService, ProfessionalsService, AssignmentsService, ToastService } from '@services';
import { CreateReportRequest } from '@models/requests/reports/create-report.request';
import { ProfessionalPersonResponse } from '@models';
import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  ButtonDirective,
  ColComponent,
  RowComponent,
  FormControlDirective,
  FormSelectDirective,
  SpinnerComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-report-new',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ButtonDirective,
    ColComponent,
    RowComponent,
    FormControlDirective,
    FormSelectDirective,
    SpinnerComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly reportsService      = inject(ReportsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService  = inject(AssignmentsService);
  private readonly toastService        = inject(ToastService);
  private readonly router = inject(Router);
  private readonly elRef  = inject(ElementRef);

  persons           = signal<ProfessionalPersonResponse[]>([]);
  filteredPersons   = signal<ProfessionalPersonResponse[]>([]);
  isLoading         = signal(false);
  isLoadingPersons  = signal(true);

  // Combobox state
  personSearch       = '';
  personDropdownOpen = false;
  selectedPerson     = signal<ProfessionalPersonResponse | null>(null);

  // Modal post-creación
  showSubmitModal  = signal(false);
  isSubmitting     = signal(false);
  createdReportId: number | null = null;

  readonly reportTypes = [
    { id: 1, name: 'Evaluación Mensual' },
    { id: 2, name: 'Informe de Progreso' },
    { id: 3, name: 'Evaluación Trimestral' },
    { id: 4, name: 'Informe Anual' },
  ];

  form: CreateReportRequest = {
    personId: '',
    title: '',
    content: '',
    reportTypeId: 0,
    reportDate: new Date().toISOString().split('T')[0],
    periodStartDate: '',
    periodEndDate: '',
    achievedGoals: '',
    areasToReinforce: '',
    futureRecommendations: '',
    nextObjectives: '',
  };

  get isValid(): boolean {
    return (
      this.form.personId !== '' &&
      this.form.title.trim() !== '' &&
      this.form.content.trim() !== '' &&
      this.form.reportTypeId > 0
    );
  }

  // Cierra el dropdown al hacer click fuera del componente
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.professionalsService.getMyProfile().subscribe({
      next: (prof) => {
        this.assignmentsService.getPersonsByProfessional(prof.id).subscribe({
          next: (persons) => {
            const active = persons.filter(p => p.isActive);
            this.persons.set(active);
            this.filteredPersons.set(active);
            this.isLoadingPersons.set(false);
          },
          error: () => this.isLoadingPersons.set(false),
        });
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
    } else {
      const lower = term.toLowerCase();
      this.filteredPersons.set(
        this.persons().filter(p =>
          `${p.personFirstName} ${p.personLastName}`.toLowerCase().includes(lower)
        )
      );
    }
  }

  selectPerson(person: ProfessionalPersonResponse): void {
    this.selectedPerson.set(person);
    this.form.personId = person.personId;
    this.personSearch = `${person.personFirstName} ${person.personLastName}`;
    this.personDropdownOpen = false;
  }

  clearPerson(): void {
    this.selectedPerson.set(null);
    this.form.personId = '';
    this.personSearch = '';
    this.filteredPersons.set(this.persons());
  }

  closeDropdown(): void {
    this.personDropdownOpen = false;
    // Si no hay selección, restaurar el texto al nombre del seleccionado (o vaciar)
    const sel = this.selectedPerson();
    this.personSearch = sel
      ? `${sel.personFirstName} ${sel.personLastName}`
      : '';
  }

  onSubmit(): void {
    this.isLoading.set(true);
    this.reportsService.create(this.form).subscribe({
      next: (report) => {
        this.createdReportId = report.id;
        this.isLoading.set(false);
        this.showSubmitModal.set(true);
      },
      error: () => this.isLoading.set(false),
    });
  }

  /** El profesional elige enviar el reporte al admin de inmediato */
  submitNow(): void {
    if (!this.createdReportId) return;
    this.isSubmitting.set(true);
    this.reportsService.submitReport(this.createdReportId).subscribe({
      next: () => {
        this.toastService.success('Reporte enviado al administrador para revisión.');
        this.router.navigate(['/pro/reports']);
      },
      error: () => {
        this.toastService.error('No se pudo enviar el reporte. Podés hacerlo desde el listado.');
        this.isSubmitting.set(false);
        this.router.navigate(['/pro/reports']);
      },
    });
  }

  /** El profesional prefiere revisar antes de enviar */
  reviewLater(): void {
    this.toastService.success('Reporte guardado como borrador.');
    this.router.navigate(['/pro/reports']);
  }

  onCancel(): void {
    this.router.navigate(['/pro/reports']);
  }
}
