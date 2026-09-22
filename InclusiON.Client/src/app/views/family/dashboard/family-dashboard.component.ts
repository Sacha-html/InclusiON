import { Component, ElementRef, HostListener, inject, OnDestroy, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { IconDirective } from '@coreui/icons-angular';
import {
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  BadgeComponent,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';
import { FamilyService, SignalrService } from '@services';
import { FamilyDashboardResponse, FamilyPersonSummaryResponse, RoadmapLevelStepResponse } from '@models';

@Component({
  selector: 'app-family-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    IconDirective,
    DatePipe,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    RowComponent,
    ColComponent,
    BadgeComponent,
    SpinnerComponent,
    AlertComponent,
  ],
  templateUrl: './family-dashboard.component.html',
  styleUrl: './family-dashboard.component.scss',
})
export class FamilyDashboardComponent implements OnInit, OnDestroy {
  private readonly familyService = inject(FamilyService);
  private readonly signalrService = inject(SignalrService);
  readonly #el = inject(ElementRef<HTMLElement>);

  private notifSub?: Subscription;
  private pollSub?: Subscription;

  readonly defaultRoadmapSteps: RoadmapLevelStepResponse[] = Array.from({ length: 10 }, (_, i) => ({
    level: i + 1,
    title: `Nivel ${i + 1}`,
    status: (i === 0 ? 'active' : 'locked') as 'completed' | 'struggling' | 'active' | 'locked',
  }));

  dashboard: FamilyDashboardResponse | null = null;
  loading = true;
  error = false;

  @HostListener('document:visibilitychange')
  onVisibilityChange(): void {
    if (document.visibilityState === 'visible') {
      this.loadDashboard(false);
    }
  }

  ngOnInit(): void {
    this.signalrService.start();

    this.loadDashboard(true);

    // Actualización automática en tiempo real al recibir eventos de actividades/resolución
    this.notifSub = this.signalrService.notification$.subscribe(() => {
      this.loadDashboard(false);
    });

    // Sondeo periódico de respaldo cada 30s mientras la pestaña esté visible
    this.pollSub = interval(30000).subscribe(() => {
      if (document.visibilityState === 'visible') {
        this.loadDashboard(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.notifSub?.unsubscribe();
    this.pollSub?.unsubscribe();
  }

  loadDashboard(showLoading = true): void {
    if (showLoading) this.loading = true;
    this.familyService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
        this.loading = false;
        this.error = false;
        if (showLoading) {
          setTimeout(() => {
            const h = this.#el.nativeElement.querySelector('h1') as HTMLElement;
            h?.focus();
          }, 80);
        }
      },
      error: () => {
        this.error = true;
        this.loading = false;
      },
    });
  }

  getRoadmapSteps(person: FamilyPersonSummaryResponse): RoadmapLevelStepResponse[] {
    if (person.roadmapLevels && person.roadmapLevels.length > 0) {
      return person.roadmapLevels.map(step => {
        if (step.level === person.currentRoadmapLevel && step.status === 'locked') {
          return {
            ...step,
            status: (person.hasFrustrationAlert ? 'struggling' : 'active') as 'struggling' | 'active',
          };
        }
        return step;
      });
    }
    return this.defaultRoadmapSteps.map(step => {
      if (step.level < person.currentRoadmapLevel) {
        return { ...step, status: 'completed' as const };
      }
      if (step.level === person.currentRoadmapLevel) {
        return {
          ...step,
          status: (person.hasFrustrationAlert ? 'struggling' : 'active') as 'struggling' | 'active',
          title: person.roadmapLevelName || step.title,
        };
      }
      return { ...step, status: 'locked' as const };
    });
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'completed': return 'Aprobado';
      case 'struggling': return 'Requiere apoyo';
      case 'active': return 'En proceso';
      case 'locked':
      default: return 'Bloqueado';
    }
  }

  getSuccessColor(pct: number): string {
    if (pct >= 80) return 'success';
    if (pct >= 50) return 'warning';
    return 'danger';
  }

  getGasBadgeColor(label?: string): string {
    if (!label) return 'info';
    const lower = label.toLowerCase();
    if (lower.includes('superando')) return 'success';
    if (lower.includes('ritmo')) return 'info';
    if (lower.includes('desarrollo')) return 'warning';
    return 'secondary';
  }
}

