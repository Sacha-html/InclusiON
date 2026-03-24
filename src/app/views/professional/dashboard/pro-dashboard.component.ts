import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  CardBodyComponent, CardComponent, ColComponent, RowComponent,
  SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { ProfessionalsService, AssignmentsService, InvitationsService } from '@services';
import { getInvitationStatusColor } from '@shared/utils';
import {
  ProfessionalResponse,
  ProfessionalPersonResponse,
  InvitationResponse,
} from '@models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-pro-dashboard',
  standalone: true,
  imports: [
    CardComponent, CardBodyComponent, RowComponent, ColComponent,
    SpinnerComponent, BadgeComponent, TableDirective, ButtonDirective,
    IconDirective,
  ],
  templateUrl: './pro-dashboard.component.html',
  styleUrl: './pro-dashboard.component.scss',
})
export class ProDashboardComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly invitationsService = inject(InvitationsService);
  private readonly router = inject(Router);

  isLoading = true;
  professional: ProfessionalResponse | null = null;
  persons: ProfessionalPersonResponse[] = [];
  invitations: InvitationResponse[] = [];

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

  get recentInvitations(): InvitationResponse[] {
    return this.invitations.slice(0, 5);
  }

  ngOnInit(): void {
    this.professionalsService.getMyProfile().subscribe({
      next: (prof) => {
        this.professional = prof;
        this.loadDashboardData(prof.id);
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  private loadDashboardData(professionalId: string): void {
    forkJoin({
      persons: this.assignmentsService.getPersonsByProfessional(professionalId),
      invitations: this.invitationsService.getAll(),
    }).subscribe({
      next: ({ persons, invitations }) => {
        this.persons = persons;
        this.invitations = invitations;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  getStatusColor = getInvitationStatusColor;

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Buenos dias';
    if (hour < 19) return 'Buenas tardes';
    return 'Buenas noches';
  }
}
