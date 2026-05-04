import { Routes } from '@angular/router';

export const aacRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./home/aac-home.component').then((m) => m.AacHomeComponent),
    data: { title: 'Inicio' }
  },
  {
    path: 'activities',
    loadComponent: () =>
      import('./activities/aac-activities.component').then(
        (m) => m.AacActivitiesComponent
      ),
    data: { title: 'Actividades' }
  },
  {
    path: 'activities/:assignmentId',
    loadComponent: () =>
      import('./activities/player/activity-player-shell.component').then(
        (m) => m.ActivityPlayerShellComponent
      ),
    data: { title: 'Actividad' }
  },
  {
    path: 'roadmap',
    loadComponent: () =>
      import('./roadmap/aac-roadmap.component').then(
        (m) => m.AacRoadmapComponent
      ),
    data: { title: 'Mi Roadmap' }
  },
  {
    path: 'calendar',
    loadComponent: () =>
      import('./calendar/aac-calendar.component').then(
        (m) => m.AacCalendarComponent
      ),
    data: { title: 'Calendario' }
  },
  {
    path: 'talk',
    loadComponent: () =>
      import('./communication/aac-communication.component').then(
        (m) => m.AacCommunicationComponent
      ),
    data: { title: 'Comunicacion' }
  }
];
