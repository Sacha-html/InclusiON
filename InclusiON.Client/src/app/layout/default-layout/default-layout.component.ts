import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NgScrollbar } from 'ngx-scrollbar';
import { INavData } from '@coreui/angular';

import {
  ContainerComponent,
  ShadowOnScrollDirective,
  SidebarBrandComponent,
  SidebarComponent,
  SidebarFooterComponent,
  SidebarHeaderComponent,
  SidebarNavComponent,
  SidebarToggleDirective,
  SidebarTogglerDirective,
} from '@coreui/angular';

import { DefaultFooterComponent, DefaultHeaderComponent } from './';
import { navItems } from './_nav';
import { ToasterComponent } from '@components/toaster/toaster.component';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';
import { AuthService } from '@services/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './default-layout.component.html',
  styleUrls: ['./default-layout.component.scss'],
  imports: [
    SidebarComponent,
    SidebarHeaderComponent,
    SidebarBrandComponent,
    SidebarNavComponent,
    SidebarFooterComponent,
    SidebarToggleDirective,
    SidebarTogglerDirective,
    ContainerComponent,
    DefaultFooterComponent,
    DefaultHeaderComponent,
    NgScrollbar,
    RouterOutlet,
    RouterLink,
    ShadowOnScrollDirective,
    ToasterComponent,
    AccessibilityPanelComponent,
  ],
})
export class DefaultLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  public navItems: INavData[] = [];

  ngOnInit(): void {
    const isGlobal = this.authService.isGlobalAdmin();

    if (isGlobal) {
      // Admin global: ve todo excepto "Mis Instituciones"
      this.navItems = navItems.filter(item => item.url !== '/admin/my-institutions');
    } else {
      // Admin institucional: no ve Instituciones, Administradores, Roles, ni seccion "Sistema"
      const globalOnlyUrls = ['/admin/institutions', '/admin/roles', '/admin/admins'];
      this.navItems = navItems.filter(item => {
        // Filtrar la seccion "Sistema" (solo tiene items de admin global)
        if (item.title && item.name === 'Sistema') return false;
        const url = typeof item.url === 'string' ? item.url : '';
        return !globalOnlyUrls.includes(url);
      });
    }
  }
}
