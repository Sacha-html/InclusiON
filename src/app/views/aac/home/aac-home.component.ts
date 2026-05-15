import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BigButtonComponent } from '@shared/components/big-button/big-button.component';
import { AuthService } from '@services';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-aac-home',
  standalone: true,
  imports: [BigButtonComponent, IconDirective],
  templateUrl: './aac-home.component.html',
  styleUrl: './aac-home.component.scss'
})
export class AacHomeComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  get userName(): string {
    return this.authService.getCurrentUser()?.name || 'Usuario';
  }

  get greeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return `Buenos dias, ${this.userName}`;
    if (hour < 19) return `Buenas tardes, ${this.userName}`;
    return `Buenas noches, ${this.userName}`;
  }

  goTo(path: string): void {
    this.router.navigate([path]);
  }

  readonly timeIcon: string = (() => {
    const h = new Date().getHours();
    return h >= 19 || h < 6 ? 'cilMoon' : 'cilSun';
  })();
}
