import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportsService, ToastService, CatalogsService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { UpdateReportRequest } from '@models/requests/reports/update-report.request';
import { ReportStatus } from '@models/responses/reports/report.response';
import { CatalogItem } from '@models';
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
  AlertComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-report-edit',
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
    AlertComponent,
  ],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.scss',
})
export class EditComponent implements OnInit {
  private readonly reportsService  = inject(ReportsService);
  private readonly toastService    = inject(ToastService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly route           = inject(ActivatedRoute);
  private readonly router          = inject(Router);

  reportId     = 0;
  isLoading    = signal(true);
  isSaving     = signal(false);
  serverError  = '';
  wasRejected  = false;
  adminComment = '';

  reportTypes = signal<CatalogItem[]>([]);

  form: UpdateReportRequest = {
    title: '',
    content: '',
    reportTypeId: 0,
    reportDate: '',
    periodStartDate: '',
    periodEndDate: '',
    achievedGoals: '',
    areasToReinforce: '',
    futureRecommendations: '',
    nextObjectives: '',
  };

  get isValid(): boolean {
    return (
      this.form.title.trim() !== '' &&
      this.form.content.trim() !== '' &&
      this.form.reportTypeId > 0 &&
      this.form.reportDate !== ''
    );
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.router.navigate([AppRoutes.Pro.Reports]); return; }
    this.reportId = +id;
    this.catalogsService.getReportTypes().subscribe({
      next: (types) => this.reportTypes.set(types),
    });
    this.reportsService.getById(this.reportId).subscribe({
      next: (report) => {
        if (report.status !== ReportStatus.Draft && report.status !== ReportStatus.Rejected) {
          this.toastService.error('Solo se pueden editar reportes en estado Borrador o Rechazado.');
          this.router.navigate([AppRoutes.Pro.Reports, this.reportId]);
          return;
        }
        this.wasRejected  = report.status === ReportStatus.Rejected;
        this.adminComment = report.adminComment ?? '';
        this.form = {
          title:                report.title,
          content:              report.content,
          reportTypeId:         report.reportTypeId,
          reportDate:           report.reportDate.split('T')[0],
          periodStartDate:      report.periodStartDate?.split('T')[0] ?? '',
          periodEndDate:        report.periodEndDate?.split('T')[0] ?? '',
          achievedGoals:        report.achievedGoals ?? '',
          areasToReinforce:     report.areasToReinforce ?? '',
          futureRecommendations: report.futureRecommendations ?? '',
          nextObjectives:       report.nextObjectives ?? '',
        };
        this.isLoading.set(false);
      },
      error: () => this.router.navigate([AppRoutes.Pro.Reports]),
    });
  }

  onSubmit(): void {
    if (!this.isValid) return;
    this.isSaving.set(true);
    this.serverError = '';
    const payload = {
      ...this.form,
      periodStartDate:       this.form.periodStartDate       || undefined,
      periodEndDate:         this.form.periodEndDate         || undefined,
      achievedGoals:         this.form.achievedGoals         || undefined,
      areasToReinforce:      this.form.areasToReinforce      || undefined,
      futureRecommendations: this.form.futureRecommendations || undefined,
      nextObjectives:        this.form.nextObjectives        || undefined,
    };
    this.reportsService.update(this.reportId, payload).subscribe({
      next: () => {
        this.toastService.success('Reporte actualizado exitosamente.');
        this.router.navigate([AppRoutes.Pro.Reports, this.reportId]);
      },
      error: (err) => {
        this.serverError = err?.userMessage ?? 'Error al guardar el reporte.';
        this.isSaving.set(false);
      },
    });
  }

  onCancel(): void {
    this.router.navigate([AppRoutes.Pro.Reports, this.reportId]);
  }
}
