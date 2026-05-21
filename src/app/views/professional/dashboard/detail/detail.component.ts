import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  CardBodyComponent, CardComponent, ColComponent, RowComponent,
  SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { ProfessionalsService, AssignmentsService, InvitationsService, ReportsService, ToastService } from '@services';
import { MessagesService } from '@services/messages.service';
import { getInvitationStatusColor } from '@shared/utils';
import {
  ProfessionalResponse,
  ProfessionalPersonResponse,
  InvitationResponse,
} from '@models';
import { ReportListItemResponse, ReportStatus } from '@models';
import { ReportStatus as ReportStatusLabels } from '@shared/constants/status-labels';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-professional-dashboard',
  standalone: true,
  imports: [
    CardComponent, CardBodyComponent, RowComponent, ColComponent,
    SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
    IconDirective,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService   = inject(AssignmentsService);
  private readonly invitationsService   = inject(InvitationsService);
  private readonly reportsService       = inject(ReportsService);
  private readonly messagesService      = inject(MessagesService);
  private readonly toastService         = inject(ToastService);
  private readonly router               = inject(Router);

  isLoading = true;
  professional: ProfessionalResponse | null = null;
  persons: ProfessionalPersonResponse[] = [];
  invitations: InvitationResponse[] = [];
  recentReports: ReportListItemResponse[] = [];
  unreadMessages = 0;

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

  get draftReportsCount(): number {
    return this.recentReports.filter(r => r.status === ReportStatus.Draft).length;
  }

  get submittedReportsCount(): number {
    return this.recentReports.filter(r => r.status === ReportStatus.Submitted).length;
  }

  get rejectedReportsCount(): number {
    return this.recentReports.filter(r => r.status === ReportStatus.Rejected).length;
  }

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
  }

  private loadDashboardData(professionalId: string): void {
    forkJoin({
      persons:     this.assignmentsService.getPersonsByProfessional(professionalId),
      invitations: this.invitationsService.getAll(),
      reports:     this.reportsService.getReports({
                     page: 1,
                     professionalId,
                     pageSize: 10,
                     sortBy: 'createdAt',
                     sortDirection: 'DESC',
                   }),
      unread:      this.messagesService.getUnreadCount(),
    }).subscribe({
      next: ({ persons, invitations, reports, unread }) => {
        this.persons        = persons;
        this.invitations    = invitations.data;
        this.recentReports  = reports.data;
        this.unreadMessages = unread;
        this.isLoading      = false;
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
