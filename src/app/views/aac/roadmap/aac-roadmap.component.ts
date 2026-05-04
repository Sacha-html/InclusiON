import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RoadmapService } from '@services/roadmap.service';
import { ActivitiesService } from '@services/activities.service';
import { AppRoutes } from '@shared/constants/app-routes';
import { RoadmapResponse, RoadmapAreaResponse, RoadmapActivityResponse } from '@models/responses';
import { ActivityAssignmentResponse, ActivityAssignmentStatus } from '@models/responses/activity.response';
import { SpinnerComponent } from '@coreui/angular';
import { NgClass } from '@angular/common';

export type NodeStatus = 'locked' | 'available' | 'pending' | 'in-progress' | 'completed';

export interface RoadmapNode {
  activity: RoadmapActivityResponse;
  assignment?: ActivityAssignmentResponse;
  status: NodeStatus;
  side: 'left' | 'right';
}

export interface EnrichedArea extends RoadmapAreaResponse {
  nodes: RoadmapNode[];
  headerColor: string;
}

@Component({
  selector: 'app-aac-roadmap',
  standalone: true,
  imports: [SpinnerComponent, NgClass],
  templateUrl: './aac-roadmap.component.html',
  styleUrl: './aac-roadmap.component.scss',
})
export class AacRoadmapComponent implements OnInit {
  private readonly roadmapService    = inject(RoadmapService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly router            = inject(Router);

  loading   = signal(true);
  hasError  = signal(false);
  roadmap   = signal<RoadmapResponse | null>(null);
  assignments = signal<ActivityAssignmentResponse[]>([]);

  enrichedAreas = computed<EnrichedArea[]>(() => {
    const r = this.roadmap();
    const asns = this.assignments();
    if (!r) return [];

    return r.areas.map(area => ({
      ...area,
      headerColor: area.color ?? '#5C6BC0',
      nodes: area.activities.map((act, idx) => {
        const assignment = asns.find(a => a.activityId === act.activityId);
        return {
          activity:   act,
          assignment,
          status:     this.resolveStatus(act, assignment),
          side:       (idx % 2 === 0 ? 'left' : 'right') as 'left' | 'right',
        };
      }),
    }));
  });

  ngOnInit(): void {
    forkJoin({
      roadmap:     this.roadmapService.getMyRoadmap().pipe(catchError(() => of(null))),
      assignments: this.activitiesService.getMyAssignments().pipe(catchError(() => of([]))),
    }).subscribe({
      next: ({ roadmap, assignments }) => {
        this.roadmap.set(roadmap);
        this.assignments.set(assignments ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.loading.set(false);
      },
    });
  }

  onNodeClick(node: RoadmapNode): void {
    if (node.status === 'locked' || !node.assignment || node.status === 'completed') return;
    this.router.navigate([AppRoutes.Aac.Activities, node.assignment.id]);
  }

  nodeLabel(node: RoadmapNode): string {
    switch (node.status) {
      case 'locked':      return '🔒';
      case 'completed':   return '✓';
      case 'in-progress': return '▶';
      default:            return String(node.activity.sequenceOrder);
    }
  }

  nodeAriaLabel(node: RoadmapNode): string {
    const title = node.activity.activityTitle;
    switch (node.status) {
      case 'locked':      return `${title} - bloqueada`;
      case 'completed':   return `${title} - completada`;
      case 'in-progress': return `${title} - en progreso, tap para continuar`;
      case 'available':   return `${title} - disponible pero no asignada aún`;
      default:            return `${title} - pendiente, tap para iniciar`;
    }
  }

  difficultyStars(level: number): string {
    return '★'.repeat(Math.min(level, 5)) + '☆'.repeat(Math.max(0, 5 - level));
  }

  private resolveStatus(act: RoadmapActivityResponse, assignment?: ActivityAssignmentResponse): NodeStatus {
    if (!act.isUnlocked) return 'locked';
    if (!assignment)     return 'available';
    switch (assignment.status) {
      case ActivityAssignmentStatus.Completada:  return 'completed';
      case ActivityAssignmentStatus.EnProgreso:  return 'in-progress';
      default:                                   return 'pending';
    }
  }
}
