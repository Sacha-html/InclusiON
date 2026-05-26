import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  CardBodyComponent, CardComponent, ColComponent, RowComponent,
  SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
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
    CardComponent, CardBodyComponent, RowComponent, ColComponent,
    SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
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

    // Increment unread badge in real-time when a push arrives
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
}
