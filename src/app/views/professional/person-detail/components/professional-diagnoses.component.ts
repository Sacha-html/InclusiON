import { Component, Input, Output, EventEmitter, inject, OnInit, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { DiagnosesService } from '@services/diagnoses.service';
import { CreateDiagnosisRequest } from '@models/requests/diagnoses/create-diagnosis.request';
import { DiagnosisListItemResponse, DiagnosisResponse } from '@models/responses/diagnosis.response';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  ColComponent,
  FormControlDirective,
  FormLabelDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-diagnoses',
  standalone: true,
  imports: [
    CommonModule,
    ButtonDirective,
    CardBodyComponent,
    CardComponent,
    ColComponent,
    FormControlDirective,
    FormLabelDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    RowComponent,
    FormsModule,
  ],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h6 class="mb-0">Diagnósticos registrados</h6>
      <button cButton color="primary" size="sm" (click)="openNew()" aria-label="Nuevo diagnóstico">Nuevo diagnóstico</button>
    </div>

    @if (currentDiagnoses().length === 0) {
      <p class="text-body-secondary">No hay diagnósticos registrados para esta persona.</p>
    }

    @for (diag of currentDiagnoses(); track diag.id) {
      <c-card class="mb-2">
        <c-card-body class="py-2 px-3">
          <div class="d-flex justify-content-between align-items-start">
            <div>
              <strong>{{ diag.diagnosisDate | date:'dd/MM/yyyy' }}</strong>
              <span class="text-body-secondary ms-2">— {{ diag.professionalName }}</span>
              <p class="mb-0 mt-1">{{ diag.primaryDiagnosis }}</p>
            </div>
            <button cButton color="link" size="sm" (click)="openEdit(diag)"
                    aria-label="Ver/editar diagnóstico">Ver</button>
          </div>
        </c-card-body>
      </c-card>
    }

    <c-modal [visible]="showModal()" (visibleChange)="showModal.set($event)" (visibleChange)="!$event && closeModal()" size="lg">
      <c-modal-header>
        <strong>{{ editing() ? 'Editar Diagnóstico' : 'Nuevo Diagnóstico' }}</strong>
      </c-modal-header>
      <c-modal-body>
        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel>Fecha del diagnóstico *</label>
            <input cFormControl type="date" [(ngModel)]="form.diagnosisDate" />
          </c-col>
        </c-row>
        <c-row class="mb-3">
          <c-col sm="12">
            <label cLabel>Diagnóstico principal *</label>
            <textarea cFormControl rows="2" [(ngModel)]="form.primaryDiagnosis"></textarea>
          </c-col>
        </c-row>
        <c-row class="mb-3">
          <c-col sm="12">
            <label cLabel>Observaciones iniciales</label>
            <textarea cFormControl rows="2" [(ngModel)]="form.initialObservations"></textarea>
          </c-col>
        </c-row>
        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel>Capacidades identificadas</label>
            <textarea cFormControl rows="2" [(ngModel)]="form.identifiedCapabilities"></textarea>
          </c-col>
          <c-col sm="6">
            <label cLabel>Desafíos identificados</label>
            <textarea cFormControl rows="2" [(ngModel)]="form.identifiedChallenges"></textarea>
          </c-col>
        </c-row>
        <c-row class="mb-3">
          <c-col sm="6">
            <label cLabel>Apoyos requeridos</label>
            <textarea cFormControl rows="2" [(ngModel)]="form.requiredSupports"></textarea>
          </c-col>
          <c-col sm="6">
            <label cLabel>Objetivos pedagógicos</label>
            <textarea cFormControl rows="2" [(ngModel)]="form.pedagogicalObjectives"></textarea>
          </c-col>
        </c-row>
        <c-row class="mb-3">
          <c-col sm="12">
            <label cLabel>Estrategias recomendadas</label>
            <textarea cFormControl rows="2" [(ngModel)]="form.recommendedStrategies"></textarea>
          </c-col>
        </c-row>
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="closeModal()">Cancelar</button>
        <button cButton color="primary" (click)="save()" [disabled]="!form.primaryDiagnosis" aria-label="Guardar diagnóstico">
          {{ editing() ? 'Guardar cambios' : 'Crear diagnóstico' }}
        </button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class ProfessionalDiagnosesComponent implements OnInit {
  @Input({ required: true }) personId!: string;
  @Input() diagnoses: DiagnosisListItemResponse[] = [];
  @Output() diagnosesChange = new EventEmitter<DiagnosisListItemResponse[]>();

  private readonly diagnosesService = inject(DiagnosesService);

  showModal = signal(false);
  editing = signal<DiagnosisResponse | null>(null);
  currentDiagnoses = signal<DiagnosisListItemResponse[]>([]);
  form: CreateDiagnosisRequest = this.emptyForm();

  ngOnInit(): void {
    this.currentDiagnoses.set(this.diagnoses);
  }

  private loadDiagnoses(): void {
    this.diagnosesService.getByPerson(this.personId).subscribe({
      next: (data) => {
        this.currentDiagnoses.set(data);
        this.diagnosesChange.emit(data);
      },
    });
  }

  openNew(): void {
    this.editing.set(null);
    this.form = this.emptyForm();
    this.showModal.set(true);
  }

  openEdit(item: DiagnosisListItemResponse): void {
    this.diagnosesService.getById(item.id).subscribe({
      next: (d) => {
        this.editing.set(d);
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
    });
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editing.set(null);
  }

  save(): void {
    if (!this.form.primaryDiagnosis) return;

    if (this.editing()) {
      this.diagnosesService.update(this.editing()!.id, this.form).subscribe({
        next: () => {
          this.showModal.set(false);
          this.loadDiagnoses();
        },
      });
    } else {
      this.diagnosesService.create(this.personId, this.form).subscribe({
        next: () => {
          this.showModal.set(false);
          this.loadDiagnoses();
        },
      });
    }
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
