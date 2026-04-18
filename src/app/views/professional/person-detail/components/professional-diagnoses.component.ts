import { Component, Input, Output, EventEmitter, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DiagnosesService } from '@services/diagnoses.service';
import { AuthService, ToastService } from '@services';
import { CreateDiagnosisRequest } from '@models/requests/diagnoses/create-diagnosis.request';
import { DiagnosisListItemResponse, DiagnosisResponse } from '@models/responses/diagnosis.response';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  ColComponent,
  FormControlDirective,
  FormFeedbackComponent,
  FormLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-professional-diagnoses',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    BadgeComponent,
    ButtonDirective,
    ColComponent,
    FormControlDirective,
    FormFeedbackComponent,
    FormLabelDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    RowComponent,
    SpinnerComponent,
    IconDirective,
  ],
  templateUrl: './professional-diagnoses.component.html',
  styleUrl: './professional-diagnoses.component.scss',
})
export class ProfessionalDiagnosesComponent implements OnInit {
  @Input({ required: true }) personId!: string;
  @Input() diagnoses: DiagnosisListItemResponse[] = [];
  @Output() diagnosesChange = new EventEmitter<DiagnosisListItemResponse[]>();

  private readonly diagnosesService = inject(DiagnosesService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  canCreate = this.authService.hasPermission('diagnoses:create');
  canUpdate = this.authService.hasPermission('diagnoses:update');
  private readonly currentUserId = this.authService.getCurrentUser()?.id ?? '';

  loading = signal(false);
  saving = signal(false);
  showModal = signal(false);
  editing = signal<DiagnosisResponse | null>(null);
  editingIsCreator = signal(false);
  currentDiagnoses = signal<DiagnosisListItemResponse[]>([]);
  submitted = false;
  form: CreateDiagnosisRequest = this.emptyForm();

  ngOnInit(): void {
    this.currentDiagnoses.set(this.diagnoses);
    if (this.diagnoses.length === 0) {
      this.loadDiagnoses();
    }
  }

  isCreator(diag: DiagnosisListItemResponse): boolean {
    return diag.createdByUserId === this.currentUserId;
  }

  private loadDiagnoses(): void {
    this.loading.set(true);
    this.diagnosesService.getByPerson(this.personId).subscribe({
      next: (data) => {
        this.currentDiagnoses.set(data);
        this.diagnosesChange.emit(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error al cargar los diagnósticos');
      },
    });
  }

  openNew(): void {
    this.editing.set(null);
    this.editingIsCreator.set(true);
    this.submitted = false;
    this.form = this.emptyForm();
    this.showModal.set(true);
  }

  openEdit(item: DiagnosisListItemResponse): void {
    this.editingIsCreator.set(this.isCreator(item));
    this.diagnosesService.getById(item.id).subscribe({
      next: (d) => {
        this.editing.set(d);
        this.submitted = false;
        this.form = {
          diagnosisDate: d.diagnosisDate.substring(0, 10),
          primaryDiagnosis: d.primaryDiagnosis,
          initialObservations: d.initialObservations ?? '',
          identifiedCapabilities: d.identifiedCapabilities ?? '',
          identifiedChallenges: d.identifiedChallenges ?? '',
          requiredSupports: d.requiredSupports ?? '',
          pedagogicalObjectives: d.pedagogicalObjectives ?? '',
          recommendedStrategies: d.recommendedStrategies ?? '',
        };
        this.showModal.set(true);
      },
      error: () => this.toastService.error('Error al cargar el diagnóstico'),
    });
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editing.set(null);
    this.submitted = false;
  }

  save(): void {
    this.submitted = true;
    if (!this.form.primaryDiagnosis?.trim()) return;
    this.saving.set(true);

    const op = this.editing()
      ? this.diagnosesService.update(this.editing()!.id, this.form)
      : this.diagnosesService.create(this.personId, this.form);

    op.subscribe({
      next: () => {
        this.toastService.success(
          this.editing() ? 'Diagnóstico actualizado' : 'Diagnóstico creado'
        );
        this.saving.set(false);
        this.showModal.set(false);
        this.loadDiagnoses();
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.message ?? 'Error al guardar el diagnóstico';
        this.toastService.error(msg);
      },
    });
  }

  private emptyForm(): CreateDiagnosisRequest {
    return {
      diagnosisDate: new Date().toISOString().substring(0, 10),
      primaryDiagnosis: '',
      initialObservations: '',
      identifiedCapabilities: '',
      identifiedChallenges: '',
      requiredSupports: '',
      pedagogicalObjectives: '',
      recommendedStrategies: '',
    };
  }
}
