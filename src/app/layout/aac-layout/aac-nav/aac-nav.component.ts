import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { IconDirective } from '@coreui/icons-angular';

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
      path: '/app',
      label: 'Inicio',
      icon: 'cilHome',
      color: '#4CAF50'
    },
    {
      path: '/app/activities',
      label: 'Actividades',
      icon: 'cilTask',
      color: '#2196F3'
    },
    {
      path: '/app/calendar',
      label: 'Calendario',
      icon: 'cilCalendar',
      color: '#FF9800'
    },
    {
      path: '/app/talk',
      label: 'Hablar',
      icon: 'cilChatBubble',
      color: '#9C27B0'
    }
  ];
}
