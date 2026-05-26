import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, input, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import {
  AvatarComponent,
  BreadcrumbRouterComponent,
  ContainerComponent,
  DropdownComponent,
  DropdownHeaderDirective,
  DropdownItemDirective,
  DropdownMenuDirective,
  DropdownToggleDirective,
  HeaderComponent,
  HeaderNavComponent,
  HeaderTogglerDirective,
  NavItemComponent,
  NavLinkDirective,
  SidebarToggleDirective,
  TooltipDirective,
} from '@coreui/angular';

import { IconDirective } from '@coreui/icons-angular';
import { AuthService, AccessibilityService } from '@services';
import { User } from '@models';
import { NotificationBellComponent } from '@shared/components/notification-bell/notification-bell.component';

@Component({
  selector: 'app-default-header',
  templateUrl: './default-header.component.html',
  imports: [
    ContainerComponent,
    HeaderTogglerDirective,
    SidebarToggleDirective,
    IconDirective,
    HeaderNavComponent,
    NavItemComponent,
    NavLinkDirective,
    RouterLink,
    RouterLinkActive,
    NgTemplateOutlet,
    BreadcrumbRouterComponent,
    DropdownComponent,
    DropdownToggleDirective,
    AvatarComponent,
    DropdownMenuDirective,
    DropdownHeaderDirective,
    DropdownItemDirective,
    TooltipDirective,
    NotificationBellComponent,
  ],
})
export class DefaultHeaderComponent extends HeaderComponent implements OnInit {
  readonly authService = inject(AuthService);
  readonly accessibilityService = inject(AccessibilityService);

  readonly colorModeIcon = computed(() => {
    const mode = this.accessibilityService.colorMode();
    return mode === 'dark' ? 'cilMoon' : 'cilSun';
  });

  readonly accessibilityIcon = computed(() => {
    return this.accessibilityService.getCurrentThemeInfo().icon;
  });

  currentUser?: User | null = null;

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
  }

  sidebarId = input<string>('sidebar1');

  logout(): void {
    this.authService.logout();
  }
}
