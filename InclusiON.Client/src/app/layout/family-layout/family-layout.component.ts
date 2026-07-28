import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgScrollbar } from 'ngx-scrollbar';
import {
  ContainerComponent,
  SidebarBrandComponent,
  SidebarComponent,
  SidebarFooterComponent,
  SidebarHeaderComponent,
  SidebarNavComponent,
  SidebarToggleDirective,
  INavData,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { DefaultHeaderComponent } from '../default-layout/default-header/default-header.component';
import { DefaultFooterComponent } from '../default-layout/default-footer/default-footer.component';
import { familyNavItems } from './_nav';
import { ToasterComponent } from '@components/toaster/toaster.component';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';
import { MessagesService } from '@services';

@Component({
  selector: 'app-family-layout',
  standalone: true,
  imports: [
    SidebarComponent,
    SidebarHeaderComponent,
    SidebarBrandComponent,
    SidebarNavComponent,
    SidebarFooterComponent,
    SidebarToggleDirective,
    ContainerComponent,
    DefaultHeaderComponent,
    DefaultFooterComponent,
    IconDirective,
    NgScrollbar,
    RouterOutlet,
    ToasterComponent,
    AccessibilityPanelComponent
  ],
  templateUrl: './family-layout.component.html',
  styleUrl: './family-layout.component.scss'
})
export class FamilyLayoutComponent implements OnInit {
  private readonly messagesService = inject(MessagesService);

  navItems: INavData[] = familyNavItems;

  ngOnInit(): void {
    this.messagesService.getUnreadCount().subscribe({
      next: (count) => {
        this.navItems = familyNavItems.map(item =>
          typeof item.url === 'string' && item.url.endsWith('/messages')
            ? { ...item, badge: count > 0 ? { color: 'danger', text: String(count) } : undefined }
            : item
        );
      },
    });
  }
}
