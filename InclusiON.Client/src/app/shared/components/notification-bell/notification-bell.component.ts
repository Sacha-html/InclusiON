import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { IconDirective } from '@coreui/icons-angular';
import { NavItemComponent, NavLinkDirective, TooltipDirective } from '@coreui/angular';
import { MessagesService } from '@services/messages.service';
import { SignalrService } from '@services/signalr.service';
import { AuthService } from '@services/auth.service';
import { UserRoles } from '@shared/constants/roles';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [IconDirective, NavItemComponent, NavLinkDirective, TooltipDirective],
  template: `
    <c-nav-item>
      <a cNavLink
         role="button"
         (click)="onBellClick()"
         [attr.aria-label]="unreadLabel()"
         cTooltip="Mensajes sin leer"
         style="position: relative; cursor: pointer; display: inline-flex; align-items: center;">
        <svg cIcon class="my-1" name="cilBell" size="lg" aria-hidden="true"></svg>
        @if (unreadCount() > 0) {
          <span class="bell-badge" aria-hidden="true">
            {{ unreadCount() > 99 ? '99+' : unreadCount() }}
          </span>
        }
      </a>
    </c-nav-item>
  `,
  styles: [`
    :host { display: contents; }

    a { position: relative; }

    .bell-badge {
      position: absolute;
      top: -2px;
      right: -6px;
      min-width: 16px;
      height: 16px;
      padding: 0 3px;
      border-radius: 8px;
      background: #D32F2F;
      color: #fff;
      font-size: 10px;
      font-weight: 700;
      line-height: 16px;
      text-align: center;
      pointer-events: none;
      border: 1.5px solid var(--a11y-bg, #fff);
    }

    @media (prefers-reduced-motion: reduce) {
      * { animation: none !important; transition: none !important; }
    }
  `],
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private readonly messagesService = inject(MessagesService);
  private readonly signalrService  = inject(SignalrService);
  private readonly authService     = inject(AuthService);
  private readonly router          = inject(Router);

  readonly unreadCount = signal(0);

  private sub?: Subscription;

  readonly unreadLabel = () => {
    const n = this.unreadCount();
    return n === 0
      ? 'Mensajes — sin mensajes no leídos'
      : `Mensajes — ${n} sin leer`;
  };

  ngOnInit(): void {
    this.fetchCount();

    // Increment badge on each real-time push notification
    this.sub = this.signalrService.notification$.subscribe(() => {
      this.unreadCount.update(n => n + 1);
    });
  }

  onBellClick(): void {
    this.unreadCount.set(0);
    const role = this.authService.getUserRole();
    if (role === UserRoles.Professional) {
      this.router.navigate(['/pro/messages']);
    } else if (role === UserRoles.FamilyRepresentative) {
      this.router.navigate(['/family/messages']);
    } else {
      this.router.navigate(['/messages']);
    }
  }

  private fetchCount(): void {
    this.messagesService.getUnreadCount().subscribe({
      next:  (n) => this.unreadCount.set(n),
      error: ()  => { /* non-critical, skip */ },
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
