import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BigButtonComponent } from '../../../shared/components/big-button/big-button.component';
import { VisualCardComponent } from '../../../shared/components/visual-card/visual-card.component';
import { AuthService } from '../../../services';

@Component({
  selector: 'app-aac-home',
  standalone: true,
  imports: [BigButtonComponent, VisualCardComponent],
  template: `
    <div class="aac-home">
      <h1 class="home-greeting">{{ greeting }}</h1>

      <section class="quick-actions">
        <h2 class="section-title">Acciones rapidas</h2>
        <div class="actions-grid">
          <app-big-button
            label="Mis Actividades"
            icon="cilTask"
            color="#2196F3"
            (buttonClick)="goTo('/app/activities')"
          />
          <app-big-button
            label="Ver Calendario"
            icon="cilCalendar"
            color="#FF9800"
            (buttonClick)="goTo('/app/calendar')"
          />
          <app-big-button
            label="Hablar"
            icon="cilChatBubble"
            color="#9C27B0"
            (buttonClick)="goTo('/app/talk')"
          />
          <app-big-button
            label="Pedir Ayuda"
            icon="cilBell"
            color="#F44336"
            (buttonClick)="requestHelp()"
          />
        </div>
      </section>

      <section class="today-activities">
        <h2 class="section-title">Hoy</h2>
        <div class="cards-list">
          <app-visual-card
            title="Actividad de ejemplo"
            subtitle="10:00 AM"
            icon="cilPuzzle"
            accentColor="#4CAF50"
            badge="Pendiente"
            badgeColor="#FF9800"
            (cardClick)="goTo('/app/activities')"
          />
        </div>
      </section>
    </div>
  `,
  styles: [`
    .aac-home {
      padding: 8px;
    }

    .home-greeting {
      font-size: 32px;
      font-weight: 700;
      color: var(--aac-text, #1a1a1a);
      margin: 0 0 24px;
      text-align: center;
    }

    .section-title {
      font-size: 22px;
      font-weight: 600;
      color: var(--aac-text, #1a1a1a);
      margin: 0 0 16px;
    }

    .quick-actions {
      margin-bottom: 32px;
    }

    .actions-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }

    .today-activities {
      margin-bottom: 24px;
    }

    .cards-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    :host-context([data-profile="high-contrast"]) {
      .home-greeting,
      .section-title {
        color: #fff;
      }
    }

    :host-context([data-color-mode="dark"]) {
      .home-greeting,
      .section-title {
        color: #f5f5f5;
      }
    }

    @media (min-width: 600px) {
      .actions-grid {
        grid-template-columns: repeat(4, 1fr);
      }
    }
  `]
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

  requestHelp(): void {
    alert('Solicitando ayuda...');
  }
}
