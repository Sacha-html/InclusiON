import { Component, inject, OnInit, signal } from '@angular/core';
import { switchMap } from 'rxjs';
import { ProfessionalsService, AssignmentsService, ToastService } from '@services';
import { ProfessionalPersonResponse } from '@models';
import { ProfessionalRoadmapTabComponent } from '../person-detail/components/professional-roadmap-tab.component';
import {
  AlertComponent,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-professional-goals',
  standalone: true,
  imports: [
    AlertComponent,
    SpinnerComponent,
    ProfessionalRoadmapTabComponent,
  ],
  templateUrl: './professional-goals.component.html',
  styleUrl: './professional-goals.component.scss',
})
export class ProfessionalGoalsComponent implements OnInit {
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService   = inject(AssignmentsService);
  private readonly toastService         = inject(ToastService);

  persons          = signal<ProfessionalPersonResponse[]>([]);
  selectedPersonId = signal<string | null>(null);
  isLoading        = signal(true);
  hasError         = signal(false);

  ngOnInit(): void {
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
    ).subscribe({
      next: (persons) => {
        const active = persons.filter(p => p.isActive);
        this.persons.set(active);
        this.isLoading.set(false);
        if (active.length === 1) {
          this.selectedPersonId.set(active[0].personId);
        }
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
        this.toastService.error('Error al cargar las personas asignadas');
      },
    });
  }

  selectPerson(personId: string): void {
    this.selectedPersonId.set(personId);
  }

  getInitial(p: ProfessionalPersonResponse): string {
    return (p.personFirstName?.charAt(0) || '?').toUpperCase();
  }
}
