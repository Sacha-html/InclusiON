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

@Component({
  selector: 'app-classroom-list',
  standalone: true,
  imports: [
    CardComponent, CardBodyComponent, CardHeaderComponent,
    ColComponent, RowComponent, SpinnerComponent, BadgeComponent,
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

  getInitial(person: ProfessionalPersonResponse): string {
    return (person.personFirstName?.charAt(0) || '?').toUpperCase();
  }

  getAvatarColor(person: ProfessionalPersonResponse): string {
    return person.avatarColor || '#2196F3';
  }

  getTextColor(bgColor: string): string {
    const hex = bgColor.replace('#', '');
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
    return luminance > 0.5 ? '#000000' : '#FFFFFF';
  }

  goToDetail(person: ProfessionalPersonResponse): void {
    this.router.navigate([AppRoutes.Pro.Persons, person.personId]);
  }
}
