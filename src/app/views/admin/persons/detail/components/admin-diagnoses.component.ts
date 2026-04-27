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
  templateUrl: './admin-diagnoses.component.html',
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
