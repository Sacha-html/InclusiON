import { Component, Input, OnInit, inject } from '@angular/core';
import { DiagnosesService } from '@services';
import { DiagnosisListItemResponse, DiagnosisResponse, PersonResponse } from '@models';
import {
  ButtonDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
  TableDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-diagnoses',
  standalone: true,
  imports: [
    ButtonDirective,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    SpinnerComponent,
    TableDirective,
  ],
  template: `
    <h5 class="mb-3">Diagnósticos</h5>

    @if (loading) {
      <div class="text-center py-4">
        <c-spinner></c-spinner>
      </div>
    } @else if (diagnoses.length === 0) {
      <p class="text-body-secondary">No hay diagnósticos registrados para esta persona.</p>
    } @else {
      <table cTable hover responsive>
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Diagnóstico principal</th>
            <th>Profesional</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (d of diagnoses; track d.id) {
            <tr>
              <td>{{ formatDate(d.diagnosisDate) }}</td>
              <td>{{ truncate(d.primaryDiagnosis, 80) }}</td>
              <td>{{ d.professionalName }}</td>
              <td>
                <button cButton color="primary" size="sm" variant="outline"
                        (click)="openDetail(d.id)">
                  Ver
                </button>
              </td>
            </tr>
          }
        </tbody>
      </table>
    }

    <!-- Modal detalle -->
    <c-modal [visible]="showModal" (visibleChange)="!$event && closeModal()"
             aria-labelledby="diag-modal-title" size="lg">
      <c-modal-header>
        <strong id="diag-modal-title">Diagnóstico — {{ formatDate(selected?.diagnosisDate) }}</strong>
      </c-modal-header>
      <c-modal-body>
        @if (loadingDetail) {
          <div class="text-center py-3">
            <c-spinner></c-spinner>
          </div>
        } @else if (selected) {
          <dl class="row mb-0">
            @if (person?.autonomyLevelName) {
              <dt class="col-sm-4">Nivel de autonomía</dt>
              <dd class="col-sm-8">{{ person!.autonomyLevelName }}</dd>
            }

            <dt class="col-sm-4">Profesional</dt>
            <dd class="col-sm-8">{{ selected.professionalName }}</dd>

            <dt class="col-sm-4">Diagnóstico principal</dt>
            <dd class="col-sm-8">{{ selected.primaryDiagnosis }}</dd>

            @if (selected.initialObservations) {
              <dt class="col-sm-4">Observaciones iniciales</dt>
              <dd class="col-sm-8">{{ selected.initialObservations }}</dd>
            }
            @if (selected.identifiedCapabilities) {
              <dt class="col-sm-4">Capacidades identificadas</dt>
              <dd class="col-sm-8">{{ selected.identifiedCapabilities }}</dd>
            }
            @if (selected.identifiedChallenges) {
              <dt class="col-sm-4">Desafíos identificados</dt>
              <dd class="col-sm-8">{{ selected.identifiedChallenges }}</dd>
            }
            @if (selected.requiredSupports) {
              <dt class="col-sm-4">Apoyos requeridos</dt>
              <dd class="col-sm-8">{{ selected.requiredSupports }}</dd>
            }
            @if (selected.pedagogicalObjectives) {
              <dt class="col-sm-4">Objetivos pedagógicos</dt>
              <dd class="col-sm-8">{{ selected.pedagogicalObjectives }}</dd>
            }
            @if (selected.recommendedStrategies) {
              <dt class="col-sm-4">Estrategias recomendadas</dt>
              <dd class="col-sm-8">{{ selected.recommendedStrategies }}</dd>
            }
          </dl>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="closeModal()">Cerrar</button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class AdminDiagnosesComponent implements OnInit {
  @Input({ required: true }) personId!: string;
  @Input() person: PersonResponse | null = null;

  private readonly diagnosesService = inject(DiagnosesService);

  diagnoses: DiagnosisListItemResponse[] = [];
  selected: DiagnosisResponse | null = null;
  loading = false;
  loadingDetail = false;
  showModal = false;

  ngOnInit(): void {
    this.loading = true;
    this.diagnosesService.getByPerson(this.personId).subscribe({
      next: (data) => {
        this.diagnoses = data ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  openDetail(id: number): void {
    this.selected = null;
    this.loadingDetail = true;
    this.showModal = true;
    this.diagnosesService.getById(id).subscribe({
      next: (data) => {
        this.selected = data;
        this.loadingDetail = false;
      },
      error: () => {
        this.loadingDetail = false;
        this.showModal = false;
      },
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.selected = null;
  }

  formatDate(date?: string): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }

  truncate(text: string, max: number): string {
    return text.length > max ? text.slice(0, max) + '…' : text;
  }
}
