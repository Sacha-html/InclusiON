import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ActivitiesService, ToastService } from '@services';
import { PersonListItemResponse } from '@models';
import { AppRoutes } from '@shared/constants/app-routes';
import { AuthService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  ButtonDirective,
  SpinnerComponent,
  BadgeComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-activity-detail',
  standalone: true,
  imports: [
    RouterModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ButtonDirective,
    SpinnerComponent,
    BadgeComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly activitiesService = inject(ActivitiesService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  activityId = '';
  canUpdate = this.authService.hasPermission(Permissions.Activities.Update);

  activity = signal<any>(null);
  isLoading = signal(true);

  compatiblePersons = signal<PersonListItemResponse[]>([]);
  personsLoading = signal(false);

  ngOnInit(): void {
    this.activityId = this.route.snapshot.paramMap.get('id')!;
    this.loadActivity();
    this.loadCompatiblePersons();
  }

  private loadActivity(): void {
    this.activitiesService.getById(this.activityId).subscribe({
      next: (data) => {
        this.activity.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar la actividad');
        this.router.navigate([AppRoutes.Pro.Activities]);
      },
    });
  }

  private loadCompatiblePersons(): void {
    this.personsLoading.set(true);
    this.activitiesService.getCompatiblePersons(this.activityId, 10).subscribe({
      next: (data) => {
        this.compatiblePersons.set(data);
        this.personsLoading.set(false);
      },
      error: () => {
        this.personsLoading.set(false);
      },
    });
  }

  edit(): void {
    this.router.navigate([AppRoutes.Pro.Activities, this.activityId, 'edit']);
  }

  back(): void {
    this.router.navigate([AppRoutes.Pro.Activities]);
  }
}