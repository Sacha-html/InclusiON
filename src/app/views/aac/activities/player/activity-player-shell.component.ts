import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ActivitiesService } from '@services/activities.service';
import { ActivityAssignmentResponse } from '@models/responses/activity.response';
import { SelectFigurePlayerComponent } from './select-figure/select-figure-player.component';

@Component({
  selector: 'app-activity-player-shell',
  standalone: true,
  imports: [SelectFigurePlayerComponent],
  templateUrl: './activity-player-shell.component.html',
  styleUrl: './activity-player-shell.component.scss',
})
export class ActivityPlayerShellComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly route             = inject(ActivatedRoute);
  private readonly router            = inject(Router);

  assignment  = signal<ActivityAssignmentResponse | null>(null);
  isLoading   = signal(true);
  hasError    = signal(false);

  get templateCode(): string {
    return this.assignment()?.templateTypeCode ?? '';
  }

  ngOnInit(): void {
    const assignmentId = +this.route.snapshot.paramMap.get('assignmentId')!;

    // Cargar todas las asignaciones del estudiante y filtrar por ID.
    // (Si hubiera un endpoint GET /activity-assignments/:id lo usaríamos directamente.)
    this.activitiesService.getMyAssignments().subscribe({
      next: (list) => {
        const found = list.find(a => a.id === assignmentId) ?? null;
        if (!found) {
          this.hasError.set(true);
        } else {
          this.assignment.set(found);
        }
        this.isLoading.set(false);
      },
      error: () => { this.hasError.set(true); this.isLoading.set(false); },
    });
  }

  onCompleted(): void {
    this.router.navigate(['/app/activities']);
  }
}
