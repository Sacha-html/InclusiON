import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { IconDirective } from '@coreui/icons-angular';
import { AppRoutes } from '@shared/constants/app-routes';

interface NavItem {
  path: string;
  label: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-aac-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, IconDirective],
  templateUrl: './aac-nav.component.html',
  styleUrl: './aac-nav.component.scss'
})
export class AacNavComponent {
  readonly navItems: NavItem[] = [
    {
      path: AppRoutes.Aac.Root,
      label: 'Inicio',
      icon: 'cilHome',
      color: 'var(--a11y-nav-home, #4CAF50)'
    },
    {
      path: AppRoutes.Aac.MiCamino,
      label: 'Mi Camino',
      icon: 'cilMap',
      color: 'var(--a11y-nav-roadmap, #673AB7)'
    },
    {
      path: AppRoutes.Aac.Activities,
      label: 'Actividades',
      icon: 'cilTask',
      color: 'var(--a11y-nav-activities, #2196F3)'
    },
    {
      path: AppRoutes.Aac.Talk,
      label: 'Hablar',
      icon: 'cilChatBubble',
      color: 'var(--a11y-nav-talk, #9C27B0)'
    }
  ];
}
