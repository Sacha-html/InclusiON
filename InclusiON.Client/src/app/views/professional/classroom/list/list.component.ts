import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin, switchMap } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ProfessionalsService, AssignmentsService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ProfessionalPersonResponse, ClassroomResponse } from '@models';
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
    EmptyStateComponent, ActorAvatarComponent, FormsModule,
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
  classrooms: ClassroomResponse[] = [];
  selectedClassroomId = '';
  isLoading = true;

  get hasClassrooms(): boolean {
    return this.classrooms.length > 0;
  }

  get selectedClassroom(): ClassroomResponse | undefined {
    return this.classrooms.find(c => c.id.toLowerCase() === this.selectedClassroomId.toLowerCase());
  }

  get filteredPersons(): ProfessionalPersonResponse[] {
    const active = this.persons.filter(p => p.isActive);

    if (!this.selectedClassroomId) {
      return active;
    }

    if (this.selectedClassroomId === 'unassigned') {
      return active.filter(p => !p.classroomId && !p.classroomName);
    }

    // 1. Buscamos el aula seleccionada
    const selectedRoom = this.classrooms.find(
      c => c.id === this.selectedClassroomId || (c.id && c.id.toLowerCase() === this.selectedClassroomId.toLowerCase())
    );

    const targetName = selectedRoom?.name?.toLowerCase()?.trim();
    const targetIdRaw = this.selectedClassroomId.replace(/^ENC:/i, '').toLowerCase().trim();

    // 2. Coincidencia dual: por nombre de aula (primaria) o por ID sanitizado (fallback)
    return active.filter(p => {
      if (targetName && p.classroomName?.toLowerCase()?.trim() === targetName) {
        return true;
      }

      if (p.classroomId) {
        const personRoomIdRaw = p.classroomId.replace(/^ENC:/i, '').toLowerCase().trim();
        if (personRoomIdRaw === targetIdRaw) {
          return true;
        }
      }

      return false;
    });
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => {
        return forkJoin({
          persons: this.assignmentsService.getPersonsByProfessional(prof.id),
          classrooms: this.assignmentsService.getClassroomsByProfessional(prof.id)
        });
      })
    ).subscribe({
      next: (res) => {
        this.persons = res.persons;
        this.classrooms = res.classrooms;
        const firstWithStudents = this.classrooms.find(c => (c.studentCount ?? 0) > 0);
        if (firstWithStudents) {
          this.selectedClassroomId = firstWithStudents.id;
        } else if (this.classrooms.length > 0) {
          this.selectedClassroomId = this.classrooms[0].id;
        } else {
          this.selectedClassroomId = 'unassigned';
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; this.toastService.error('Error al cargar el aula'); },
    });
  }

  onFilterChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedClassroomId = select.value;
  }

  goToDetail(person: ProfessionalPersonResponse): void {
    this.router.navigate([AppRoutes.Pro.Persons, person.personId]);
  }
}
