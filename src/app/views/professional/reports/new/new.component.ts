import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
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
    NgSelectModule,
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
  private readonly reportsService       = inject(ReportsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService   = inject(AssignmentsService);
  private readonly toastService         = inject(ToastService);
  private readonly router               = inject(Router);

  persons          = signal<ProfessionalPersonResponse[]>([]);
  isLoading        = signal(false);
  isLoadingPersons = signal(true);
  selectedPerson   = signal<ProfessionalPersonResponse | null>(null);

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

  searchPersonFn = (term: string, item: ProfessionalPersonResponse): boolean => {
    const fullName = `${item.personFirstName} ${item.personLastName}`.toLowerCase();
    return fullName.includes(term.toLowerCase());
  };

  onPersonChange(person: ProfessionalPersonResponse | null): void {
    this.form.personId = person?.personId ?? '';
    this.selectedPerson.set(person);
  }

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.professionalsService.getMyProfile().subscribe({
      next: (prof) => {
        this.assignmentsService.getPersonsByProfessional(prof.id).subscribe({
          next: (persons) => {
            this.persons.set(persons.filter(p => p.isActive));
            this.isLoadingPersons.set(false);
          },
          error: () => this.isLoadingPersons.set(false),
        });
      },
      error: () => this.isLoadingPersons.set(false),
    });
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

  reviewLater(): void {
    this.toastService.success('Reporte guardado como borrador.');
    this.router.navigate(['/pro/reports']);
  }

  onCancel(): void {
    this.router.navigate(['/pro/reports']);
  }
}
