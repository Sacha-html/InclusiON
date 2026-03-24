import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvitationsService } from '@services';
import { InvitationResponse } from '@models';
import { getInvitationStatusColor } from '@shared/utils';

import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  TableDirective, BadgeComponent, SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-admin-invitations',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    TableDirective, BadgeComponent, SpinnerComponent,
  ],
  templateUrl: './invitations.component.html',
  styleUrl: './invitations.component.scss',
})
export class InvitationsComponent implements OnInit {
  private readonly invitationsService = inject(InvitationsService);

  invitations: InvitationResponse[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.invitationsService.getAll().subscribe({
      next: (data) => {
        this.invitations = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  getStatusColor = getInvitationStatusColor;
}
