import {
  Component, inject, OnDestroy, OnInit,
  signal, ViewChild, ViewContainerRef,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ButtonDirective } from '@coreui/angular';
import { ActivitiesService } from '@services/activities.service';
import { AppRoutes } from '@shared/constants/app-routes';
import { ActivityAssignmentResponse } from '@models';
import { PLAYER_REGISTRY } from './player-registry';
import { PlayerBaseComponent } from './player-base.component';

@Component({
  selector: 'app-activity-player-shell',
  standalone: true,
  imports: [ButtonDirective],
  templateUrl: './activity-player-shell.component.html',
  styleUrl: './activity-player-shell.component.scss',
})
export class ActivityPlayerShellComponent implements OnInit, OnDestroy {
  // El ViewChild se resuelve después de que Angular renderiza el @else block
  @ViewChild('playerHost', { read: ViewContainerRef })
  private playerHost!: ViewContainerRef;

  private readonly activitiesService = inject(ActivitiesService);
  private readonly route             = inject(ActivatedRoute);
  private readonly router            = inject(Router);

  assignment   = signal<ActivityAssignmentResponse | null>(null);
  isLoading    = signal(true);
  hasError     = signal(false);
  errorMessage = signal('');
  unsupported  = signal(false);

  private completedSub?: Subscription;

  ngOnInit(): void {
    const assignmentId = this.route.snapshot.paramMap.get('assignmentId')!;

    this.activitiesService.getAssignmentById(assignmentId).subscribe({
      next: (found) => {
        if (!PLAYER_REGISTRY[found.templateTypeCode]) {
          this.unsupported.set(true);
          this.isLoading.set(false);
          return;
        }

        this.assignment.set(found);
        this.isLoading.set(false);

        // Defer un tick para que Angular procese el @else block y exponga #playerHost
        setTimeout(() => this.renderPlayer());
      },
      error: (err) => {
        this.hasError.set(true);
        this.isLoading.set(false);
        this.errorMessage.set(err?.message || 'No se pudo cargar la actividad.');
      },
    });
  }

  /** Crea dinámicamente el componente player correspondiente al templateTypeCode. */
  private renderPlayer(): void {
    const assignment = this.assignment();
    if (!assignment || !this.playerHost) return;

    const PlayerComponent = PLAYER_REGISTRY[assignment.templateTypeCode];
    if (!PlayerComponent) { this.unsupported.set(true); return; }

    this.playerHost.clear();
    this.completedSub?.unsubscribe();

    const ref = this.playerHost.createComponent<PlayerBaseComponent>(PlayerComponent);
    ref.setInput('assignment', assignment);
    this.completedSub = ref.instance.completed.subscribe(() => this.onCompleted());
    ref.changeDetectorRef.detectChanges();

    setTimeout(() => {
      const container = this.playerHost.element.nativeElement;
      const heading = container.querySelector('[role="heading"], h1, h2, h3') as HTMLElement;
      heading?.focus();
    }, 100);
  }

  onCompleted(): void {
    this.router.navigate([AppRoutes.Aac.Activities]);
  }

  ngOnDestroy(): void {
    this.completedSub?.unsubscribe();
  }
}
