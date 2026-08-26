import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import {
  CardBodyComponent, CardComponent, CardHeaderComponent, ColComponent, RowComponent,
  SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
  ModalComponent, ModalHeaderComponent, ModalBodyComponent,
  ModalFooterComponent, ModalTitleDirective, FormSelectDirective,
  FormControlDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { ProfessionalsService, AssignmentsService, InvitationsService, ReportsService, ToastService } from '@services';
import { MessagesService } from '@services/messages.service';
import { SignalrService } from '@services/signalr.service';
import { getInvitationStatusColor, getCount, exportHtmlElementToPdf } from '@shared/utils';
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

import { HighContrastPieChartComponent, LevelHistogramChartComponent, ClassroomRankingChartComponent } from '@shared/components';
import { AnalyticsService } from '@services/analytics.service';
import { ClassroomResponse, AnalyticsDashboardResponse, FrustrationDetailResponse } from '@models';

@Component({
  selector: 'app-professional-dashboard',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent, CardBodyComponent, CardHeaderComponent, RowComponent, ColComponent,
    SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent,
    ModalFooterComponent, ModalTitleDirective, FormSelectDirective,
    FormControlDirective,
    IconDirective, DatePipe, DecimalPipe,
    HighContrastPieChartComponent,
    LevelHistogramChartComponent,
    ClassroomRankingChartComponent,
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
  private readonly analyticsService     = inject(AnalyticsService);
  private readonly router               = inject(Router);
  private readonly http                 = inject(HttpClient);

  #notifSub = Subscription.EMPTY;

  isLoading = true;
  isLoadingAnalytics = false;
  professional: ProfessionalResponse | null = null;
  persons: ProfessionalPersonResponse[] = [];
  invitations: InvitationResponse[] = [];
  recentReports: ReportListItemResponse[] = [];
  unreadMessages = 0;
  weeklyProgress: WeeklyProgressResponse | null = null;

  classrooms: ClassroomResponse[] = [];
  selectedClassroomId = '';
  analytics: AnalyticsDashboardResponse | null = null;

  draftReportsCount     = 0;
  submittedReportsCount = 0;
  rejectedReportsCount  = 0;

  // Frustration Details Modal State
  showFrustrationModal = false;
  frustrationDetails: FrustrationDetailResponse[] = [];
  isLoadingFrustrationDetails = false;

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

    this.#notifSub = this.signalrService.notification$.subscribe((notif) => {
      const title = notif.title?.toLowerCase() ?? '';
      const url = notif.actionUrl?.toLowerCase() ?? '';
      if (title.includes('mensaje') || url.includes('messages')) {
        this.unreadMessages++;
      }
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
      classrooms:      this.assignmentsService.getClassroomsByProfessional(professionalId),
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
      next: ({ persons, invitations, classrooms, reports, unread, weeklyProgress, countDraft, countSubmitted, countRejected }) => {
        this.persons               = persons;
        this.invitations           = invitations.data;
        this.classrooms            = classrooms;
        this.recentReports         = reports.data;
        this.unreadMessages        = unread;
        this.weeklyProgress        = weeklyProgress;
        this.draftReportsCount     = countDraft.totalCount;
        this.submittedReportsCount = countSubmitted.totalCount;
        this.rejectedReportsCount  = countRejected.totalCount;
        this.isLoading             = false;

        // Cargar métricas analíticas iniciales (todas las aulas)
        this.loadAnalytics();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Error al cargar el panel');
      },
    });
  }

  loadAnalytics(classroomId?: string): void {
    this.isLoadingAnalytics = true;
    this.analyticsService.getProfessionalAnalytics(classroomId, null, null).subscribe({
      next: (data) => {
        this.analytics = data;
        this.isLoadingAnalytics = false;

        // Actualizar dinámicamente los 4 números del resumen semanal según el aula seleccionada
        if (this.weeklyProgress && data) {
          this.weeklyProgress = {
            ...this.weeklyProgress,
            personCount: data.personasActivas,
            totalCompleted: data.totalActividadesCompletadas,
            avgSuccess: Math.round(data.promedioExito),
            frustrationAlerts: data.alertasFrustracion,
          };
        }
      },
      error: () => {
        this.isLoadingAnalytics = false;
        this.toastService.error('Error al actualizar las métricas analíticas.');
      }
    });
  }

  onChangeClassroom(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedClassroomId = select?.value || '';
    this.loadAnalytics(this.selectedClassroomId);
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

  isExportingPdf = false;

  // ── Download Metrics PDF ───────────────────────────────────────────────
  async downloadWeeklyProgressPdf(): Promise<void> {
    const container = document.getElementById('proDashboardMetricsArea');
    if (!container) {
      this.toastService.error('No se encontró el contenedor de métricas para exportar.');
      return;
    }

    try {
      this.isExportingPdf = true;
      this.toastService.info('Generando PDF con las métricas y gráficos del aula...');

      const classroomName = this.selectedClassroomId
        ? (this.classrooms.find(c => c.id === this.selectedClassroomId)?.name || 'Aula')
        : 'Todas_las_Aulas';

      const fileName = `Dashboard_Metricas_${classroomName.replace(/\s+/g, '_')}_${new Date().toISOString().slice(0, 10)}.pdf`;

      // Esperar brevemente para asegurar que los gráficos estén completamente renderizados
      await new Promise(resolve => setTimeout(resolve, 200));

      await exportHtmlElementToPdf(container, {
        filename: fileName,
        orientation: 'landscape',
        format: 'a4',
        margin: 10,
        scale: 2,
        fitToSinglePage: true,
      });

      this.toastService.success('PDF exportado exitosamente.');
    } catch (error) {
      console.error('Error al exportar PDF:', error);
      this.toastService.error('Error al generar el PDF del dashboard.');
    } finally {
      this.isExportingPdf = false;
    }
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

  openFrustrationDetails(): void {
    this.showFrustrationModal = true;
    this.isLoadingFrustrationDetails = true;
    this.frustrationDetails = [];

    this.analyticsService.getFrustrationDetails(this.selectedClassroomId, null, null).subscribe({
      next: (details) => {
        this.frustrationDetails = details || [];
        this.isLoadingFrustrationDetails = false;
      },
      error: () => {
        this.isLoadingFrustrationDetails = false;
        this.toastService.error('Error al cargar el detalle de alertas de frustración.');
      }
    });
  }

  closeFrustrationModal(): void {
    this.showFrustrationModal = false;
  }
}
