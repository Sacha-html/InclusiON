import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ProfessionalsService, AssignmentsService } from '@services';
import { ProfessionalPersonResponse } from '@models';
import {
  CardComponent, CardBodyComponent, CardHeaderComponent,
  ColComponent, RowComponent, SpinnerComponent, BadgeComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-classroom',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    ColComponent, RowComponent, SpinnerComponent, BadgeComponent,
  ],
  templateUrl: './classroom.component.html',
  styleUrl: './classroom.component.scss',
})
export class ClassroomComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly router = inject(Router);

  persons: ProfessionalPersonResponse[] = [];
  isLoading = true;

  get activePersons(): ProfessionalPersonResponse[] {
    return this.persons.filter(p => p.isActive);
  }

  ngOnInit(): void {
    this.professionalsService.getMyProfile().subscribe({
      next: (prof) => {
        this.assignmentsService.getPersonsByProfessional(prof.id).subscribe({
          next: (persons) => {
            this.persons = persons;
            this.isLoading = false;
          },
          error: () => this.isLoading = false,
        });
      },
      error: () => this.isLoading = false,
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
    this.router.navigate(['/pro/persons', person.personId]);
  }
}
