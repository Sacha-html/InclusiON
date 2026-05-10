import { Component, Input, OnInit, inject } from '@angular/core';
import { DiagnosesService, ToastService } from '@services';
import { DiagnosisListItemResponse, DiagnosisResponse, PersonResponse } from '@models';
import {
  ButtonDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';

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
    DataTableComponent,
    ConfirmModalComponent,
  ],
  templateUrl: './admin-diagnoses.component.html',
})
export class AdminDiagnosesComponent implements OnInit {
  @Input({ required: true }) personId!: string;
  @Input() person: PersonResponse | null = null;

  private readonly diagnosesService = inject(DiagnosesService);
  private readonly toastService     = inject(ToastService);

  diagnoses: DiagnosisListItemResponse[] = [];
  selected: DiagnosisResponse | null = null;
  loading = false;
  loadingDetail = false;
  showModal = false;

  showDeactivateModal  = false;
  deactivatingDiag: DiagnosisListItemResponse | null = null;
  isDeactivating = false;

  columns: TableColumn[] = [
    { key: 'diagnosisDate', label: 'Fecha', type: 'date' },
    { key: 'primaryDiagnosis', label: 'Diagnóstico principal' },
    { key: 'professionalName', label: 'Profesional' },
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'view',       label: 'Ver',         icon: 'cil-search' },
        { action: 'deactivate', label: 'Dar de baja', icon: 'cil-ban'    },
      ],
    },
  ];

  onRowAction(event: { action: string; item: DiagnosisListItemResponse }): void {
    if (event.action === 'view')       this.openDetail(event.item.id);
    if (event.action === 'deactivate') this.openDeactivate(event.item);
  }

  ngOnInit(): void {
    this.loading = true;
    this.diagnosesService.getByPerson(this.personId).subscribe({
      next: (data) => {
        this.diagnoses = data ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.error('Error al cargar los diagnósticos');
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
        this.toastService.error('Error al cargar el detalle del diagnóstico');
      },
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.selected = null;
  }

  openDeactivate(diag: DiagnosisListItemResponse): void {
    this.deactivatingDiag = diag;
    this.showDeactivateModal = true;
  }

  confirmDeactivate(): void {
    if (!this.deactivatingDiag) return;
    this.isDeactivating = true;
    this.diagnosesService.patchStatus(this.deactivatingDiag.id, false).subscribe({
      next: () => {
        this.toastService.success('Diagnóstico dado de baja exitosamente.');
        this.diagnoses = this.diagnoses.filter(d => d.id !== this.deactivatingDiag!.id);
        this.showDeactivateModal = false;
        this.isDeactivating = false;
        this.deactivatingDiag = null;
      },
      error: (err) => {
        const msg = err?.userMessage ?? 'Error al dar de baja el diagnóstico.';
        this.toastService.error(msg);
        this.isDeactivating = false;
        this.showDeactivateModal = false;
      },
    });
  }

  cancelDeactivate(): void {
    this.showDeactivateModal = false;
    this.deactivatingDiag = null;
  }

  formatDate(date?: string): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }

}
