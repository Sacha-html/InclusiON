import { Component, inject } from '@angular/core';
import { Location } from '@angular/common';
import { AuthService, AccessibilityService } from '@services';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-aac-header',
  standalone: true,
  imports: [IconDirective],
  templateUrl: './aac-header.component.html',
  styleUrl: './aac-header.component.scss'
})
export class AacHeaderComponent {
  readonly authService = inject(AuthService);
  readonly a11y = inject(AccessibilityService);
  private readonly location = inject(Location);

  get userName(): string {
    return this.authService.getCurrentUser()?.name || 'Usuario';
  }

  get userInitial(): string {
    return this.userName.charAt(0).toUpperCase();
  }

  get isReadingMode(): boolean {
    return this.a11y.readingMode();
  }

  goBack(): void {
    this.location.back();
  }

  openSettings(): void {
    this.a11y.openPanel();
  }

  logout(): void {
    this.authService.logout();
  }
}
