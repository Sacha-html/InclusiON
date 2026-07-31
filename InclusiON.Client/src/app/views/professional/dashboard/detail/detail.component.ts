import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import {
  CardBodyComponent, CardComponent, ColComponent, RowComponent,
  SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
  ModalComponent, ModalHeaderComponent, ModalBodyComponent,
  ModalFooterComponent, ModalTitleDirective, FormSelectDirective,
  FormControlDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { ProfessionalsService, AssignmentsService, InvitationsService, ReportsService, ToastService } from '@services';
import { MessagesService } from '@services/messages.service';
import { SignalrService } from '@services/signalr.service';
import { getInvitationStatusColor, getCount } from '@shared/utils';
import {
  ProfessionalResponse,
  ProfessionalPersonResponse,
  InvitationResponse,
  WeeklyProgressResponse,
} from '@models';
import { ReportListItemResponse, ReportStatus } from '@models';
import { ReportStatus as ReportStatusLabels } from '@shared/constants/status-labels';
import { forkJoin } from 'rxjs';
import { environment } from '@env';

@Component({
  selector: 'app-professional-dashboard',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent, CardBodyComponent, RowComponent, ColComponent,
    SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent,
    ModalFooterComponent, ModalTitleDirective, FormSelectDirective,
    FormControlDirective,
    IconDirective, DatePipe,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit, OnDestroy {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService   = inject(AssignmentsService);
  private readonly invitationsService   = inject(InvitationsService);
  private readonly reportsService       = inject(ReportsService);
  private readonly messagesService      = inject(MessagesService);
  private readonly signalrService       = inject(SignalrService);
  private readonly toastService         = inject(ToastService);
  private readonly router               = inject(Router);
  private readonly http                 = inject(HttpClient);

  #notifSub = Subscription.EMPTY;

  isLoading = true;
  professional: ProfessionalResponse | null = null;
  persons: ProfessionalPersonResponse[] = [];
  invitations: InvitationResponse[] = [];
  recentReports: ReportListItemResponse[] = [];
  unreadMessages = 0;
  weeklyProgress: WeeklyProgressResponse | null = null;

  draftReportsCount     = 0;
  submittedReportsCount = 0;
  rejectedReportsCount  = 0;

  // Sharing Modal State
  showShareModal = false;
  contacts: any[] = [];
  selectedTutorId = '';
  shareMessageBody = '';
  sendingShare = false;

  get personCount(): number {
    return this.persons.filter(p => p.isActive).length;
  }

  get pendingInvitations(): number {
    return this.invitations.filter(i => i.status === 'Enviada').length;
  }

  get acceptedInvitations(): number {
    return this.invitations.filter(i => i.status === 'Aceptada').length;
  }

  get recentPersons(): ProfessionalPersonResponse[] {
    return this.persons.filter(p => p.isActive).slice(0, 5);
  }

  get recentInvitationsList(): InvitationResponse[] {
    return this.invitations.slice(0, 5);
  }

  readonly reportBadgeMap: Partial<Record<string, { color: string; label: string }>> = {
    [ReportStatus.Draft]:     { color: 'secondary', label: ReportStatusLabels.Borrador },
    [ReportStatus.Submitted]: { color: 'warning',   label: ReportStatusLabels.Enviado  },
    [ReportStatus.Approved]:  { color: 'success',   label: ReportStatusLabels.Aprobado },
    [ReportStatus.Rejected]:  { color: 'danger',    label: ReportStatusLabels.Rechazado },
  };

  ngOnInit(): void {
    this.professionalsService.getMyProfile().subscribe({
      next: (prof) => {
        this.professional = prof;
        this.loadDashboardData(prof.id);
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar el perfil profesional');
      },
    });

    this.#notifSub = this.signalrService.notification$.subscribe(() => {
      this.unreadMessages++;
    });
  }

  ngOnDestroy(): void {
    this.#notifSub.unsubscribe();
  }

  private loadDashboardData(professionalId: string): void {
    const reportsUrl = `${environment.apiUrl}/reports`;

    const countParams = (status: string | number) =>
      new HttpParams()
        .set('professionalId', professionalId)
        .set('pageSize', '1')
        .set('page', '1')
        .set('status', status);

    forkJoin({
      persons:         this.assignmentsService.getPersonsByProfessional(professionalId),
      invitations:     this.invitationsService.getAll(),
      reports:         this.reportsService.getReports({
                         page: 1,
                         professionalId,
                         pageSize: 5,
                         sortBy: 'createdAt',
                         sortDirection: 'DESC',
                       }),
      unread:          this.messagesService.getUnreadCount(),
      weeklyProgress:  this.professionalsService.getWeeklyProgress(),
      countDraft:      getCount(this.http, reportsUrl, countParams(ReportStatus.Draft)),
      countSubmitted:  getCount(this.http, reportsUrl, countParams(ReportStatus.Submitted)),
      countRejected:   getCount(this.http, reportsUrl, countParams(ReportStatus.Rejected)),
    }).subscribe({
      next: ({ persons, invitations, reports, unread, weeklyProgress, countDraft, countSubmitted, countRejected }) => {
        this.persons               = persons;
        this.invitations           = invitations.data;
        this.recentReports         = reports.data;
        this.unreadMessages        = unread;
        this.weeklyProgress        = weeklyProgress;
        this.draftReportsCount     = countDraft.totalCount;
        this.submittedReportsCount = countSubmitted.totalCount;
        this.rejectedReportsCount  = countRejected.totalCount;
        this.isLoading             = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar el panel');
      },
    });
  }

  navigateTo(path: string, queryParams?: Record<string, string>): void {
    this.router.navigate([path], queryParams ? { queryParams } : {});
  }

  getStatusColor = getInvitationStatusColor;

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Buenos días';
    if (hour < 19) return 'Buenas tardes';
    return 'Buenas noches';
  }

  // ── Download Metrics PDF ───────────────────────────────────────────────
  downloadWeeklyProgressPdf(): void {
    const wp = this.weeklyProgress;
    if (!wp) return;

    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      this.toastService.error('Por favor, permite ventanas emergentes para descargar el PDF.');
      return;
    }

    const start = new Date(wp.periodStart).toLocaleDateString('es-ES');
    const end = new Date(wp.periodEnd).toLocaleDateString('es-ES');

    printWindow.document.write(`
      <html>
        <head>
          <title>Reporte de Métricas Semanales - InclusiON</title>
          <style>
            body { font-family: 'Helvetica Neue', Arial, sans-serif; padding: 40px; color: #333; }
            .header { border-bottom: 2px solid #0096c7; padding-bottom: 20px; margin-bottom: 30px; }
            .logo { font-size: 26px; font-weight: bold; color: #0077b6; }
            .title { font-size: 20px; margin-top: 10px; font-weight: 600; }
            .period { font-size: 14px; color: #666; margin-top: 5px; }
            .grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 20px; margin-bottom: 40px; }
            .card { border: 1px solid #e0e0e0; padding: 25px; border-radius: 12px; background: #fbfbfb; box-shadow: 0 2px 4px rgba(0,0,0,0.02); }
            .label { font-size: 13px; text-transform: uppercase; letter-spacing: 0.5px; color: #777; font-weight: 600; }
            .value { font-size: 32px; font-weight: 800; color: #0077b6; margin-top: 8px; }
            .success-high { color: #2e7d32; }
            .alert-danger { color: #c62828; }
            .footer { border-top: 1px solid #eee; padding-top: 20px; font-size: 11px; color: #999; margin-top: 60px; text-align: center; }
          </style>
        </head>
        <body>
          <div class="header">
            <div class="logo">InclusiON</div>
            <div class="title">Resumen de Métricas Semanales de Alumnos</div>
            <div class="period">Período de evaluación: ${start} — ${end}</div>
          </div>
          <div class="grid">
            <div class="card">
              <div class="label">Personas Activas</div>
              <div class="value">${wp.personCount}</div>
            </div>
            <div class="card">
              <div class="label">Actividades Completadas</div>
              <div class="value">${wp.totalCompleted}</div>
            </div>
            <div class="card">
              <div class="label">Promedio de Éxito</div>
              <div class="value ${wp.avgSuccess >= 70 ? 'success-high' : ''}">${wp.avgSuccess}%</div>
            </div>
            <div class="card">
              <div class="label">Alertas de Frustración</div>
              <div class="value ${wp.frustrationAlerts > 0 ? 'alert-danger' : ''}">${wp.frustrationAlerts}</div>
            </div>
          </div>
          <div class="footer">
            Generado automáticamente por el portal profesional de InclusiON el ${new Date().toLocaleDateString('es-ES')} a las ${new Date().toLocaleTimeString('es-ES')}.
          </div>
          <script>
            window.onload = function() {
              window.print();
              setTimeout(function() { window.close(); }, 500);
            };
          </script>
        </body>
      </html>
    `);
    printWindow.document.close();
    this.toastService.success('Preparando PDF de métricas...');
  }

  // ── Share Metrics Modal ────────────────────────────────────────────────
  openShareModal(): void {
    const wp = this.weeklyProgress;
    if (!wp) return;

    this.selectedTutorId = '';
    const start = new Date(wp.periodStart).toLocaleDateString('es-ES');
    const end = new Date(wp.periodEnd).toLocaleDateString('es-ES');

    this.shareMessageBody = `Estimado Tutor, le comparto el resumen de métricas semanales (${start} — ${end}):\n\n` +
      `- Alumnos activos a cargo: ${wp.personCount}\n` +
      `- Actividades completadas: ${wp.totalCompleted}\n` +
      `- Promedio de éxito académico: ${wp.avgSuccess}%\n` +
      `- Alertas de frustración registradas: ${wp.frustrationAlerts}\n\n` +
      `Quedo a su entera disposición para cualquier aclaración o consulta.`;

    this.messagesService.getContacts(1, 100).subscribe({
      next: (list) => {
        this.contacts = list;
        this.showShareModal = true;
      },
      error: () => {
        this.toastService.error('Error al cargar la lista de tutores');
      }
    });
  }

  closeShareModal(): void {
    this.showShareModal = false;
  }

  sendSharedMetrics(): void {
    if (!this.selectedTutorId) {
      this.toastService.error('Por favor, selecciona un tutor destinatario.');
      return;
    }
    if (!this.shareMessageBody.trim()) {
      this.toastService.error('El contenido del mensaje no puede estar vacío.');
      return;
    }

    this.sendingShare = true;
    this.messagesService.send({
      receiverId: this.selectedTutorId,
      subject: 'Resumen de Métricas Semanales de Alumnos',
      content: this.shareMessageBody.trim()
    }).subscribe({
      next: () => {
        this.sendingShare = false;
        this.showShareModal = false;
        this.toastService.success('Métricas compartidas por mensajería exitosamente.');
      },
      error: () => {
        this.sendingShare = false;
        this.toastService.error('Error al enviar las métricas.');
      }
    });
  }
}
