import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RoadmapService } from '@services/roadmap.service';
import { ActivitiesService } from '@services/activities.service';
import { AppRoutes } from '@shared/constants/app-routes';
import { contrastTextColor } from '@shared/utils';
import {
  RoadmapResponse,
  RoadmapAreaResponse,
  RoadmapActivityResponse,
  ActivityAssignmentResponse,
  ActivityAssignmentStatus,
  ActivityListItemResponse
} from '@models';
import { ButtonDirective, SpinnerComponent } from '@coreui/angular';
import { NgClass } from '@angular/common';

export type NodeStatus = 'locked' | 'available' | 'in-progress' | 'completed';

export interface RoadmapNode {
  activity: RoadmapActivityResponse;
  areaId: number;
  assignment?: ActivityAssignmentResponse;
  status: NodeStatus;
  side: 'left' | 'right';
  score?: number;
}

export interface EnrichedArea extends RoadmapAreaResponse {
  nodes: RoadmapNode[];
  headerColor: string;
}

@Component({
  selector: 'app-aac-roadmap',
  standalone: true,
  imports: [ButtonDirective, SpinnerComponent, NgClass],
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
    if (!r || !r.areas) return [];

    return r.areas.map(area => {
      let prevCompleted = true; // El nivel 1 siempre está desbloqueado

      const nodes: RoadmapNode[] = (area.activities ?? []).map((act, idx) => {
        // Buscar asignación asociada a la actividad (por activityId o título)
        const matching = asns.filter(a => a.activityId === act.activityId || a.activityTitle === act.activityTitle);
        const assignment = matching.sort((a, b) => {
          if (a.status === ActivityAssignmentStatus.Completada && b.status !== ActivityAssignmentStatus.Completada) return -1;
          if (b.status === ActivityAssignmentStatus.Completada && a.status !== ActivityAssignmentStatus.Completada) return 1;
          return new Date(b.assignedAt).getTime() - new Date(a.assignedAt).getTime();
        })[0];

        // Obtener porcentaje de éxito de la última respuesta del backend
        const latestResponse = assignment?.responses?.sort(
          (a, b) => new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
        )[0];

        const backendScore = latestResponse?.successPercentage;

        // Consultar también progreso local persistido en el navegador
        let localScore: number | undefined;
        let localPassed = false;
        try {
          const raw = localStorage.getItem('roadmap_progress_' + act.activityId) || localStorage.getItem('roadmap_progress_' + act.id);
          if (raw) {
            const parsed = JSON.parse(raw);
            localScore = parsed.score;
            localPassed = parsed.passed || (localScore !== undefined && localScore >= 60);
          }
        } catch {}

        const isPassed = localPassed || (backendScore !== undefined && backendScore >= 60) || assignment?.status === ActivityAssignmentStatus.Completada;
        const effectiveScore = localScore !== undefined ? localScore : backendScore;

        let status: NodeStatus = 'locked';
        if (idx === 0 || prevCompleted) {
          if (isPassed) {
            status = 'completed';
          } else if (assignment?.status === ActivityAssignmentStatus.EnProgreso) {
            status = 'in-progress';
          } else {
            status = 'available';
          }
        } else {
          status = 'locked';
        }

        // Para que el siguiente nivel se desbloquee, este debe estar superado al 60%
        prevCompleted = isPassed;

        return {
          activity: act,
          areaId: area.id,
          assignment,
          status,
          side: (idx % 2 === 0 ? 'left' : 'right') as 'left' | 'right',
          score: effectiveScore
        };
      });

      return {
        ...area,
        headerColor: area.color ?? '#673AB7',
        nodes
      };
    });
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.hasError.set(false);

    forkJoin({
      officialTemplates:  this.activitiesService.getRoadmap().pipe(catchError(() => of([] as ActivityListItemResponse[]))),
      myRoadmap:          this.roadmapService.getMyRoadmap().pipe(catchError(() => of(null))),
      assignments:        this.activitiesService.getMyAssignments().pipe(catchError(() => of([] as ActivityAssignmentResponse[]))),
    }).subscribe({
      next: ({ officialTemplates, myRoadmap, assignments }) => {
        this.assignments.set(assignments ?? []);

        // Priorizar las 10 actividades oficiales del Roadmap que creaste
        if (officialTemplates && officialTemplates.length > 0) {
          const sorted = [...officialTemplates].sort((a, b) => (a.roadmapOrder ?? 0) - (b.roadmapOrder ?? 0));
          const defaultRoadmap: RoadmapResponse = {
            id: 1,
            encryptedId: 'official-roadmap',
            personId: '',
            createdByProfessionalId: '',
            createdByProfessionalFullName: 'Sistema InclusiON',
            notes: 'Estructura secuencial anti-frustración del Nivel 1 al 10.',
            createdAt: new Date().toISOString(),
            areas: [
              {
                id: 1,
                encryptedId: 'area-1',
                skillAreaId: 1,
                skillAreaName: 'Camino de Aprendizaje',
                color: '#673AB7',
                icon: 'map',
                displayOrder: 1,
                activities: sorted.map((item, idx) => ({
                  id: item.id,
                  encryptedId: item.encryptedId,
                  activityId: item.id,
                  activityTitle: item.title,
                  sequenceOrder: item.roadmapOrder ?? (idx + 1),
                  isUnlocked: idx === 0,
                  unlockThresholdPercent: 60,
                  difficultyLevel: item.complexityLevel ?? 1,
                  showHints: true
                }))
              }
            ]
          };
          this.roadmap.set(defaultRoadmap);
        } else if (myRoadmap && myRoadmap.areas && myRoadmap.areas.length > 0) {
          this.roadmap.set(myRoadmap);
        } else {
          this.roadmap.set(null);
        }

        this.loading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.loading.set(false);
      },
    });
  }

  onNodeClick(node: RoadmapNode): void {
    if (node.status === 'locked') return;

    const activityEncryptedId = node.activity.encryptedId;
    if (activityEncryptedId) {
      this.activitiesService.autoAssign(activityEncryptedId).subscribe({
        next: (assignment) => {
          if (assignment?.encryptedId) {
            this.router.navigate([AppRoutes.Aac.Activities, assignment.encryptedId]);
          }
        },
        error: () => {
          const fallbackId = node.assignment?.encryptedId || node.activity.encryptedId;
          if (fallbackId) {
            this.router.navigate([AppRoutes.Aac.Activities, fallbackId]);
          }
        }
      });
    } else if (node.assignment?.encryptedId) {
      this.router.navigate([AppRoutes.Aac.Activities, node.assignment.encryptedId]);
    }
  }

  nodeLabel(node: RoadmapNode): string {
    switch (node.status) {
      case 'locked':      return '🔒';
      case 'completed':   return '✓';
      case 'in-progress': return '▶';
      case 'available':   return String(node.activity.sequenceOrder);
      default:            return String(node.activity.sequenceOrder);
    }
  }

  nodeAriaLabel(node: RoadmapNode): string {
    const title = node.activity.activityTitle;
    switch (node.status) {
      case 'locked':      return `Nivel ${node.activity.sequenceOrder}: ${title} - bloqueada`;
      case 'completed':   return `Nivel ${node.activity.sequenceOrder}: ${title} - completada, tap para volver a jugar`;
      case 'in-progress': return `Nivel ${node.activity.sequenceOrder}: ${title} - en progreso, tap para continuar`;
      case 'available':   return `Nivel ${node.activity.sequenceOrder}: ${title} - disponible, tap para iniciar`;
      default:            return `Nivel ${node.activity.sequenceOrder}: ${title} - pendiente, tap para iniciar`;
    }
  }

  difficultyStars(level: number): string {
    return '★'.repeat(Math.min(level, 5)) + '☆'.repeat(Math.max(0, 5 - level));
  }

  /** Returns '#000000' or '#ffffff' for max contrast against a hex background color. */
  headerTextColor(hexColor: string): string {
    return contrastTextColor(hexColor);
  }
}
