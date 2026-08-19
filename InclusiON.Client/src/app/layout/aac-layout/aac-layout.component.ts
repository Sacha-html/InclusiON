import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AacNavComponent } from './aac-nav/aac-nav.component';
import { AacHeaderComponent } from './aac-header/aac-header.component';
import { ToasterComponent } from '@components/toaster/toaster.component';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';
import { AccessibilityService } from '@services/accessibility.service';

@Component({
  selector: 'app-aac-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    AacNavComponent,
    AacHeaderComponent,
    ToasterComponent,
    AccessibilityPanelComponent,
  ],
  templateUrl: './aac-layout.component.html',
  styleUrl: './aac-layout.component.scss'
})
export class AacLayoutComponent {
  readonly a11y = inject(AccessibilityService);
}
