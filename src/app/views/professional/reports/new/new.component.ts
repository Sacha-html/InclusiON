import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { of, switchMap } from 'rxjs';
import { ReactiveFormsModule, FormControl, FormsModule } from '@angular/forms';
import { SearchableSelectComponent } from '@shared/components/searchable-select/searchable-select.component';
import { ReportsService, ProfessionalsService, AssignmentsService, ToastService, CatalogsService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { CreateReportRequest } from '@models/requests/reports/create-report.request';
import { CatalogItem, ProfessionalPersonResponse } from '@models';
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
    ReactiveFormsModule,
    SearchableSelectComponent,
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
  private readonly catalogsService      = inject(CatalogsService);
  private readonly router               = inject(Router);

  persons          = signal<ProfessionalPersonResponse[]>([]);
  reportTypes      = signal<CatalogItem[]>([]);
  isLoading        = signal(false);
  personControl = new FormControl<ProfessionalPersonResponse | null>(null);

  readonly searchPersonsFn = (query: string) => {
    const lower = query.toLowerCase();
    return of(
      this.persons().filter(p =>
        `${p.personFirstName} ${p.personLastName}`.toLowerCase().includes(lower)
      )
    );
  };

  readonly displayPersonFn = (p: ProfessionalPersonResponse) =>
    `${p.personFirstName} ${p.personLastName}`;

  readonly personValueFn = (p: ProfessionalPersonResponse) => p;

  // Modal post-creación
  showSubmitModal  = signal(false);
  isSubmitting     = signal(false);
  createdReportId: number | null = null;

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
      this.personControl.value !== null &&
      this.form.title.trim() !== '' &&
      this.form.content.trim() !== '' &&
      this.form.reportTypeId > 0
    );
  }

  ngOnInit(): void {
    this.loadPersons();
    this.catalogsService.getReportTypes().subscribe(types => this.reportTypes.set(types));
  }

  loadPersons(): void {
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
    ).subscribe({
      next: (persons) => {
        this.persons.set(persons.filter(p => p.isActive));
      },
      error: () => {},
    });
  }

  onSubmit(): void {
    this.isLoading.set(true);
    const payload: CreateReportRequest = {
      ...this.form,
      personId: this.personControl.value?.personId ?? '',
      periodStartDate: this.form.periodStartDate || undefined,
      periodEndDate:   this.form.periodEndDate   || undefined,
      achievedGoals:         this.form.achievedGoals         || undefined,
      areasToReinforce:      this.form.areasToReinforce      || undefined,
      futureRecommendations: this.form.futureRecommendations || undefined,
      nextObjectives:        this.form.nextObjectives        || undefined,
    };
    this.reportsService.create(payload).subscribe({
      next: (report) => {
        this.createdReportId = report.id;
        this.isLoading.set(false);
        this.showSubmitModal.set(true);
      },
      error: () => { this.isLoading.set(false); this.toastService.error('Error al crear el informe'); },
    });
  }

  submitNow(): void {
    if (!this.createdReportId) return;
    this.isSubmitting.set(true);
    this.reportsService.submitReport(this.createdReportId).subscribe({
      next: () => {
        this.toastService.success('Reporte enviado al administrador para revisión.');
        this.router.navigate([AppRoutes.Pro.Reports]);
      },
      error: () => {
        this.toastService.error('No se pudo enviar el reporte. Podés hacerlo desde el listado.');
        this.isSubmitting.set(false);
        this.router.navigate([AppRoutes.Pro.Reports]);
      },
    });
  }

  reviewLater(): void {
    this.toastService.success('Reporte guardado como borrador.');
    this.router.navigate([AppRoutes.Pro.Reports]);
  }

  onCancel(): void {
    this.router.navigate([AppRoutes.Pro.Reports]);
  }
}
