import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { switchMap } from 'rxjs';
import { ProfessionalsService, AssignmentsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ProfessionalPersonResponse } from '@models';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ColComponent, RowComponent, SpinnerComponent, BadgeComponent,
} from '@coreui/angular';
import { EmptyStateComponent } from '@shared/components/empty-state/empty-state.component';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';

@Component({
  selector: 'app-classroom-list',
  standalone: true,
  imports: [
    CardComponent, CardBodyComponent, CardHeaderComponent,
    ColComponent, RowComponent, SpinnerComponent, BadgeComponent,
    EmptyStateComponent, ActorAvatarComponent,
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.scss',
})
export class ListComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  persons: ProfessionalPersonResponse[] = [];
  isLoading = true;

  get activePersons(): ProfessionalPersonResponse[] {
    return this.persons.filter(p => p.isActive);
  }

  ngOnInit(): void {
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
    ).subscribe({
      next: (persons) => {
        this.persons = persons;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; this.toastService.error('Error al cargar el aula'); },
    });
  }

  goToDetail(person: ProfessionalPersonResponse): void {
    this.router.navigate([AppRoutes.Pro.Persons, person.personId]);
  }
}
