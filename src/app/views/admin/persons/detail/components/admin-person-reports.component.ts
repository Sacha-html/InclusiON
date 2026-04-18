import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportsService, ToastService } from '@services';
import { ReportListItemResponse, ReportStatus } from '@models/responses/reports/report.response';
import {
  BadgeComponent,
  ButtonDirective,
  FormSelectDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  SpinnerComponent,
  TableDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-person-reports',
  standalone: true,
  imports: [
    FormsModule,
    BadgeComponent,
    ButtonDirective,
    FormSelectDirective,
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    SpinnerComponent,
    TableDirective,
  ],
  template: `
    <h5 class="mb-3">Reportes</h5>

    <!-- Filtro estado -->
    <div class="d-flex gap-2 mb-3">
      <select cSelect style="max-width: 180px" [(ngModel)]="statusFilter" (ngModelChange)="loadReports()">
        <option value="">Todos los estados</option>
        <option value="Draft">Borrador</option>
        <option value="Submitted">Enviado</option>
        <option value="Approved">Aprobado</option>
        <option value="Rejected">Rechazado</option>
      </select>
    </div>

    @if (isLoading()) {
      <div class="text-center py-4">
        <c-spinner></c-spinner>
      </div>
    } @else if (reports().length === 0) {
      <p class="text-body-secondary">No hay reportes registrados para esta persona.</p>
    } @else {
      <table cTable hover responsive>
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Título</th>
            <th>Tipo</th>
            <th>Profesional</th>
            <th>Estado</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (r of reports(); track r.id) {
            <tr>
              <td class="text-nowrap">{{ formatDate(r.reportDate) }}</td>
              <td>{{ r.title }}</td>
              <td>{{ r.reportTypeName }}</td>
              <td>{{ r.professionalName }}</td>
              <td>
                <c-badge [color]="badgeMap[r.status].color">
                  {{ badgeMap[r.status].label }}
                </c-badge>
              </td>
              <td class="text-nowrap">
                <button cButton color="primary" size="sm" variant="ghost"
                        (click)="viewReport(r.id)">
                  Ver
                </button>
                @if (r.status === ReportStatus.Submitted) {
                  <button cButton color="success" size="sm" variant="ghost" class="ms-1"
                          (click)="openApproveModal(r)">
                    Aprobar
                  </button>
                  <button cButton color="danger" size="sm" variant="ghost" class="ms-1"
                          (click)="openRejectModal(r)">
                    Rechazar
                  </button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>

      @if (totalPages() > 1) {
        <div class="d-flex justify-content-end align-items-center gap-2 mt-2">
          <button cButton color="secondary" size="sm" variant="ghost"
                  [disabled]="currentPage() === 1"
                  (click)="changePage(currentPage() - 1)">‹</button>
          <span class="text-body-secondary small">Página {{ currentPage() }} de {{ totalPages() }}</span>
          <button cButton color="secondary" size="sm" variant="ghost"
                  [disabled]="currentPage() === totalPages()"
                  (click)="changePage(currentPage() + 1)">›</button>
        </div>
      }
    }

    <!-- Modal aprobar -->
    <c-modal [visible]="showApproveModal()" (visibleChange)="showApproveModal.set($event)" alignment="center">
      <c-modal-header>
        <h5 cModalTitle>Aprobar reporte</h5>
      </c-modal-header>
      <c-modal-body>
        @if (selectedReport()) {
          <p>
            ¿Confirma que desea aprobar <strong>{{ selectedReport()!.title }}</strong>?
          </p>
          <p class="text-body-secondary mb-0">Esta acción notificará al familiar del progreso.</p>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="showApproveModal.set(false)">Cancelar</button>
        <button cButton color="success" [disabled]="isProcessing()" (click)="confirmApprove()">
          @if (isProcessing()) { <c-spinner size="sm"></c-spinner> }
          Aprobar
        </button>
      </c-modal-footer>
    </c-modal>

    <!-- Modal rechazar -->
    <c-modal [visible]="showRejectModal()" (visibleChange)="showRejectModal.set($event)" alignment="center">
      <c-modal-header>
        <h5 cModalTitle>Rechazar reporte</h5>
      </c-modal-header>
      <c-modal-body>
        @if (selectedReport()) {
          <p>Rechazando: <strong>{{ selectedReport()!.title }}</strong></p>
          <div class="mb-3">
            <label class="form-label">Motivo del rechazo *</label>
            <textarea class="form-control" rows="3"
                      placeholder="Indicá el motivo para que el profesional pueda corregirlo..."
                      [(ngModel)]="rejectComment"></textarea>
          </div>
        }
      </c-modal-body>
      <c-modal-footer>
        <button cButton color="secondary" (click)="closeRejectModal()">Cancelar</button>
        <button cButton color="danger"
                [disabled]="isProcessing() || !rejectComment.trim()"
                (click)="confirmReject()">
          @if (isProcessing()) { <c-spinner size="sm"></c-spinner> }
          Rechazar
        </button>
      </c-modal-footer>
    </c-modal>
  `,
})
export class AdminPersonReportsComponent implements OnInit {
  @Input({ required: true }) personId!: string;

  private readonly reportsService = inject(ReportsService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  readonly ReportStatus = ReportStatus;

  reports = signal<ReportListItemResponse[]>([]);
  isLoading = signal(true);
  isProcessing = signal(false);
  currentPage = signal(1);
  totalPages = signal(1);

  statusFilter = '';
  rejectComment = '';

  showApproveModal = signal(false);
  showRejectModal = signal(false);
  selectedReport = signal<ReportListItemResponse | null>(null);

  readonly badgeMap: Record<ReportStatus, { color: string; label: string }> = {
    [ReportStatus.Draft]:     { color: 'secondary', label: 'Borrador' },
    [ReportStatus.Submitted]: { color: 'warning',   label: 'Enviado' },
    [ReportStatus.Approved]:  { color: 'success',   label: 'Aprobado' },
    [ReportStatus.Rejected]:  { color: 'danger',    label: 'Rechazado' },
  };

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.isLoading.set(true);
    this.reportsService.getReports({
      personId: this.personId,
      status: this.statusFilter || undefined,
      page: this.currentPage(),
      pageSize: 10,
      sortBy: 'reportDate',
      sortDirection: 'desc',
    }).subscribe({
      next: (response) => {
        this.reports.set(response.data);
        this.totalPages.set(response.totalPages);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar los reportes.');
        this.isLoading.set(false);
      },
    });
  }

  changePage(page: number): void {
    this.currentPage.set(page);
    this.loadReports();
  }

  viewReport(id: number): void {
    this.router.navigate(['/admin/reports', id]);
  }

  openApproveModal(report: ReportListItemResponse): void {
    this.selectedReport.set(report);
    this.showApproveModal.set(true);
  }

  confirmApprove(): void {
    if (!this.selectedReport()) return;
    this.isProcessing.set(true);
    this.reportsService.approveReport(this.selectedReport()!.id).subscribe({
      next: () => {
        this.toastService.success('Reporte aprobado exitosamente.');
        this.showApproveModal.set(false);
        this.selectedReport.set(null);
        this.isProcessing.set(false);
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al aprobar el reporte.');
        this.isProcessing.set(false);
      },
    });
  }

  openRejectModal(report: ReportListItemResponse): void {
    this.selectedReport.set(report);
    this.rejectComment = '';
    this.showRejectModal.set(true);
  }

  confirmReject(): void {
    if (!this.selectedReport() || !this.rejectComment.trim()) return;
    this.isProcessing.set(true);
    this.reportsService.rejectReport(this.selectedReport()!.id, this.rejectComment.trim()).subscribe({
      next: () => {
        this.toastService.success('Reporte rechazado.');
        this.closeRejectModal();
        this.isProcessing.set(false);
        this.loadReports();
      },
      error: () => {
        this.toastService.error('Error al rechazar el reporte.');
        this.isProcessing.set(false);
      },
    });
  }

  closeRejectModal(): void {
    this.showRejectModal.set(false);
    this.selectedReport.set(null);
    this.rejectComment = '';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('es-AR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });
  }
}
