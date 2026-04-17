import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BigButtonComponent } from '../../../shared/components/big-button/big-button.component';
import { VisualCardComponent } from '../../../shared/components/visual-card/visual-card.component';
import { AuthService, ToastService } from '../../../services';

@Component({
  selector: 'app-aac-home',
  standalone: true,
  imports: [BigButtonComponent, VisualCardComponent],
  templateUrl: './aac-home.component.html',
  styleUrl: './aac-home.component.scss'
})
export class AacHomeComponent {
  private router = inject(Router);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

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

  requestHelp(): void {
    this.toastService.info('Solicitando ayuda...');
  }
}
