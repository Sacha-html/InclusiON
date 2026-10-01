import { Component, Input, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DiagnosesService, ToastService } from '@services';
import { DiagnosisListItemResponse, DiagnosisResponse, PersonResponse } from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  ColComponent,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { DataTableComponent } from '@shared/components/data-table/data-table.component';
import { TableColumn } from '@shared/components/data-table/data-table.models';

@Component({
  selector: 'app-admin-diagnoses',
  standalone: true,
  imports: [
    FormsModule,
    BadgeComponent,
    ButtonDirective,
    ColComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    RowComponent,
    SpinnerComponent,
    IconDirective,
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
  selectedItem: DiagnosisListItemResponse | null = null;
  loading = false;
  loadingDetail = false;
  showModal = false;
  downloadingPdfId: string | null = null;

  showDeactivateModal  = false;
  deactivatingDiag: DiagnosisListItemResponse | null = null;
  isDeactivating = false;

  filterFrom = '';
  filterTo = '';
  statusFilter = '';

  get studentName(): string {
    return this.person ? `${this.person.firstName} ${this.person.lastName}` : '';
  }

  get isCurrentActive(): boolean {
    if (this.selectedItem) return this.selectedItem.isActive;
    return this.selected ? this.selected.isActive : true;
  }

  get filteredDiagnoses(): DiagnosisListItemResponse[] {
    const from = this.filterFrom;
    const to = this.filterTo;
    const status = this.statusFilter;

    return this.diagnoses.filter(d => {
      if (status && d.isActive !== (status === 'true')) return false;
      const date = d.diagnosisDate.substring(0, 10);
      if (from && date < from) return false;
      if (to && date > to) return false;
      return true;
    });
  }

  clearFilters(): void {
    this.filterFrom = '';
    this.filterTo = '';
    this.statusFilter = '';
  }

  columns: TableColumn[] = [
    { key: 'diagnosisDate', label: 'Fecha', type: 'date' },
    { key: 'primaryDiagnosis', label: 'Diagnóstico principal' },
    { key: 'professionalName', label: 'Profesional' },
    { key: 'isActive', label: 'Estado', type: 'badge', badgeMap: {
      true: { color: 'success', label: 'Activo' }, false: { color: 'secondary', label: 'Inactivo' },
    } },
    {
      key: 'actions',
      label: 'Acciones',
      type: 'actions',
      actions: [
        { action: 'view', label: 'Ver', icon: 'cilSearch' },
      ],
    },
  ];

  onRowAction(event: { action: string; item: DiagnosisListItemResponse }): void {
    if (event.action === 'view') this.openDetail(event.item);
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

  openDetail(item: DiagnosisListItemResponse): void {
    this.selected = null;
    this.selectedItem = item;
    this.loadingDetail = true;
    this.showModal = true;
    this.diagnosesService.getById(item.encryptedId).subscribe({
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
    this.selectedItem = null;
  }

  openDeactivateFromModal(): void {
    if (this.selectedItem) {
      this.openDeactivate(this.selectedItem);
    } else if (this.selected) {
      this.openDeactivate({
        encryptedId: this.selected.encryptedId,
        diagnosisDate: this.selected.diagnosisDate,
        primaryDiagnosis: this.selected.primaryDiagnosis,
        professionalName: this.selected.professionalName,
        professionalId: this.selected.professionalId,
        createdByUserId: '',
        createdAt: this.selected.createdAt,
        isActive: this.selected.isActive
      });
    }
  }

  reactivateFromModal(): void {
    const diagId = this.selectedItem?.encryptedId || this.selected?.encryptedId;
    if (!diagId) return;
    this.toggleStatusById(diagId, true);
  }

  openDeactivate(diag: DiagnosisListItemResponse): void {
    this.deactivatingDiag = diag;
    this.showDeactivateModal = true;
  }

  confirmDeactivate(): void {
    if (!this.deactivatingDiag) return;
    this.isDeactivating = true;
    const diagId = this.deactivatingDiag.encryptedId;
    this.diagnosesService.patchStatus(diagId, false).subscribe({
      next: () => {
        this.toastService.success('Diagnóstico dado de baja exitosamente.');
        this.diagnoses = this.diagnoses.map(d =>
          d.encryptedId === diagId ? { ...d, isActive: false } : d
        );
        if (this.selectedItem && this.selectedItem.encryptedId === diagId) {
          this.selectedItem.isActive = false;
        }
        if (this.selected && this.selected.encryptedId === diagId) {
          this.selected.isActive = false;
        }
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

  toggleStatus(diag: DiagnosisListItemResponse, isActive: boolean): void {
    this.toggleStatusById(diag.encryptedId, isActive);
  }

  private toggleStatusById(diagId: string, isActive: boolean): void {
    this.diagnosesService.patchStatus(diagId, isActive).subscribe({
      next: () => {
        this.toastService.success(isActive ? 'Diagnóstico reactivado exitosamente.' : 'Diagnóstico dado de baja exitosamente.');
        this.diagnoses = this.diagnoses.map(d => d.encryptedId === diagId ? { ...d, isActive } : d);
        if (this.selectedItem && this.selectedItem.encryptedId === diagId) {
          this.selectedItem.isActive = isActive;
        }
        if (this.selected && this.selected.encryptedId === diagId) {
          this.selected.isActive = isActive;
        }
      },
      error: (err) => this.toastService.error(err?.userMessage ?? 'Error al cambiar el estado del diagnóstico.'),
    });
  }

  formatDate(date?: string): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }

  downloadPdf(encryptedId: string, diagDate?: string): void {
    this.downloadingPdfId = encryptedId;
    this.diagnosesService.exportPdf(encryptedId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const dateStr = diagDate ? diagDate.substring(0, 10) : 'diagnostico';
        a.download = `diagnostico-${dateStr}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
        this.downloadingPdfId = null;
      },
      error: () => {
        this.downloadingPdfId = null;
        this.toastService.error('Error al exportar el diagnóstico a PDF.');
      },
    });
  }
}
