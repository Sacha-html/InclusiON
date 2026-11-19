import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription, filter, interval } from 'rxjs';
import { IconDirective } from '@coreui/icons-angular';
import {
  DropdownComponent,
  DropdownToggleDirective,
  DropdownMenuDirective,
  DropdownItemDirective,
  TooltipDirective
} from '@coreui/angular';
import { NotificationsService, NotificationDto, NotificationType } from '@services/notifications.service';
import { SignalrService, SignalRNotification } from '@services/signalr.service';
import { AuthService } from '@services/auth.service';
import { UserRoles } from '@shared/constants/roles';

export interface AppNotification {
  id: string;
  title: string;
  message: string;
  actionUrl?: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: Date;
  timeLabel: string;
}

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [
    IconDirective,
    TooltipDirective,
    DropdownComponent,
    DropdownToggleDirective,
    DropdownMenuDirective,
    DropdownItemDirective
  ],
  template: `
    <c-dropdown alignment="end" variant="nav-item">
      <button [caret]="false"
              cDropdownToggle
              [attr.aria-label]="unreadLabel()"
              cTooltip="Notificaciones"
              class="d-flex align-items-center justify-content-center"
              style="position: relative; cursor: pointer; background: none; border: none; color: inherit;">
        <svg cIcon name="cilBell" size="lg" aria-hidden="true"></svg>
        @if (unreadCount() > 0) {
          <span class="bell-badge" aria-hidden="true">
            {{ unreadCount() > 99 ? '99+' : unreadCount() }}
          </span>
        }
      </button>
      <div cDropdownMenu style="min-width: 320px; max-width: 360px; padding: 0;" class="shadow border rounded-2">
        <div class="d-flex justify-content-between align-items-center bg-body-secondary px-3 py-2 border-bottom fw-bold rounded-top">
          <div class="d-flex align-items-center gap-2">
            <span style="font-size: 14px;">Notificaciones</span>
            @if (unreadCount() > 0) {
              <span class="badge bg-primary rounded-pill" style="font-size: 11px;">{{ unreadCount() }} nuevas</span>
            }
          </div>
          <div class="d-flex align-items-center gap-2">
            @if (unreadCount() > 0) {
              <button class="btn btn-sm btn-link p-0 text-decoration-none text-primary" (click)="markAllAsRead($event)" style="font-size: 12px; font-weight: 600;">
                Marcar leídas
              </button>
            }
            @if (notifications().length > 0) {
              <button class="btn btn-sm btn-link p-0 text-decoration-none text-danger" (click)="clearAll($event)" style="font-size: 12px; font-weight: 600;">
                Limpiar
              </button>
            }
          </div>
        </div>

        <div style="max-height: 340px; overflow-y: auto;">
          @if (notifications().length === 0) {
            <div class="text-center py-4 text-body-secondary">
              <svg cIcon name="cilBell" size="xl" class="mb-2 text-opacity-50" style="color: #6c757d; opacity: 0.5;"></svg>
              <p class="mb-0 small">No tenés notificaciones</p>
            </div>
          } @else {
            @for (notif of notifications(); track notif.id) {
              <button cDropdownItem
                      class="d-flex align-items-start border-bottom py-2 px-3 text-wrap notification-item"
                      [class.notification-unread]="!notif.isRead"
                      [class.notification-read]="notif.isRead"
                      (click)="onNotificationClick(notif)"
                      style="border: none; width: 100%;">
                <div class="me-3 mt-1 flex-shrink-0">
                  <div class="icon-container" [class.unread-icon-container]="!notif.isRead" [class.read-icon-container]="notif.isRead">
                    <svg cIcon [name]="getNotificationIcon(notif)" size="md" [class]="getNotificationColorClass(notif)"></svg>
                  </div>
                </div>
                <div class="d-flex flex-column text-start flex-grow-1" style="font-size: 13px;">
                  <div class="d-flex align-items-center justify-content-between gap-1">
                    <span class="notif-title" [class.fw-bold]="!notif.isRead" [class.text-dark]="!notif.isRead" [class.text-secondary]="notif.isRead">{{ notif.title }}</span>
                    <div class="d-flex align-items-center gap-1">
                      @if (!notif.isRead) {
                        <span class="unread-dot" title="No leído"></span>
                      }
                      <span role="button" tabindex="0"
                            class="notif-dismiss-btn"
                            (click)="dismissNotification(notif.id, $event)"
                            (keydown.enter)="dismissNotification(notif.id, $event)"
                            title="Descartar"
                            aria-label="Descartar notificación">&times;</span>
                    </div>
                  </div>
                  <span class="notif-message mt-1" [class.text-body]="!notif.isRead" [class.text-muted]="notif.isRead" style="font-size: 12px; line-height: 1.35;">{{ notif.message }}</span>
                  <span class="text-muted mt-1" style="font-size: 10px;">{{ notif.timeLabel }}</span>
                </div>
              </button>
            }
          }
        </div>
      </div>
    </c-dropdown>
  `,
  styles: [`
    :host { display: contents; }

    button { position: relative; }

    .notification-item {
      cursor: pointer;
      transition: all 0.15s ease-in-out;
      position: relative;

      &.notification-unread {
        background-color: #f0f7ff !important;
        border-left: 4px solid var(--a11y-primary, #1565C0) !important;

        &:hover,
        &:focus {
          background-color: #e0f2fe !important;
        }
      }

      &.notification-read {
        background-color: #ffffff !important;
        border-left: 4px solid transparent !important;
        opacity: 0.85;

        &:hover,
        &:focus {
          background-color: #f8fafc !important;
          opacity: 1;
        }
      }
    }

    .icon-container {
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
    }

    .unread-icon-container {
      background-color: rgba(21, 101, 192, 0.1);
    }

    .read-icon-container {
      background-color: #f1f5f9;
      opacity: 0.7;
    }

    .unread-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background-color: var(--a11y-primary, #1565C0);
      flex-shrink: 0;
      display: inline-block;
    }

    .notif-dismiss-btn {
      color: #94a3b8;
      font-size: 16px;
      line-height: 1;
      padding: 0 4px;
      border-radius: 3px;
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      transition: color 0.15s, background-color 0.15s;

      &:hover,
      &:focus {
        color: #ef4444;
        background-color: rgba(239, 68, 68, 0.1);
      }
    }

    .bell-badge {
      position: absolute;
      top: -2px;
      right: -6px;
      min-width: 17px;
      height: 17px;
      padding: 0 4px;
      border-radius: 9px;
      background: #D32F2F;
      color: #fff;
      font-size: 10.5px;
      font-weight: 700;
      line-height: 17px;
      text-align: center;
      pointer-events: none;
      border: 1.5px solid var(--a11y-bg, #fff);
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
    }

    @media (prefers-reduced-motion: reduce) {
      * { animation: none !important; transition: none !important; }
    }
  `],
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private readonly notificationsService = inject(NotificationsService);
  private readonly signalrService       = inject(SignalrService);
  private readonly authService          = inject(AuthService);
  private readonly router               = inject(Router);

  readonly notifications = signal<AppNotification[]>([]);
  readonly unreadCount   = signal(0);

  private notifSub?: Subscription;
  private routerSub?: Subscription;
  private userSub?: Subscription;
  private tickSub?: Subscription;
  private activeUserId: string | null = null;

  readonly unreadLabel = () => {
    const n = this.unreadCount();
    return n === 0
      ? 'Notificaciones — sin notificaciones no leídas'
      : `Notificaciones — ${n} sin leer`;
  };

  ngOnInit(): void {
    // Sanear cualquier clave legacy de localStorage que pudiera causar fugas entre cuentas
    this.notificationsService.purgeLegacyStorage();

    // Reaccionar a cambios de usuario autenticado (garantiza aislamiento hermético en memoria)
    this.userSub = this.authService.currentUser$.subscribe((user) => {
      const nextUserId = user?.id ?? null;
      if (nextUserId !== this.activeUserId) {
        this.activeUserId = nextUserId;
        this.notifications.set([]);
        this.unreadCount.set(0);
        if (nextUserId && this.authService.isAuthenticated()) {
          this.loadFromServer();
        }
      }
    });

    // Auto-marcar como leídas según la ruta visitada (sincronizado contra backend)
    this.routerSub = this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.checkAutoReadByUrl(event.urlAfterRedirects || event.url);
    });

    // Actualizar etiquetas relativas de tiempo cada 60 segundos sin llamadas HTTP
    this.tickSub = interval(60000).subscribe(() => {
      this.notifications.update(arr =>
        arr.map(n => ({ ...n, timeLabel: this.formatRelativeTime(n.createdAt) }))
      );
    });

    // Escuchar notificaciones push en tiempo real (deduplicando por ID persistido en DB)
    this.notifSub = this.signalrService.notification$.subscribe((data: SignalRNotification) => {
      const role = this.authService.getUserRole();
      const type = this.normalizeType(data.type, data.title, data.actionUrl);

      // Bloqueo IN-217 para rol Administrador
      if (role === UserRoles.Admin && (type === 'calendar' || data.actionUrl?.toLowerCase().includes('calendar'))) {
        return;
      }

      const id = data.id || `rt-${Date.now()}-${Math.floor(Math.random() * 1000)}`;
      const createdAt = data.createdAt ? new Date(data.createdAt) : new Date();

      const incoming: AppNotification = {
        id,
        title: data.title,
        message: data.message,
        actionUrl: data.actionUrl,
        type,
        isRead: Boolean(data.isRead),
        createdAt,
        timeLabel: this.formatRelativeTime(createdAt),
      };

      this.notifications.update(arr => {
        if (data.id && arr.some(existing => existing.id === data.id)) {
          return arr;
        }
        return [incoming, ...arr].slice(0, 50);
      });
      this.updateUnreadCount();
      this.checkAutoReadByUrl(this.router.url);
    });
  }

  private loadFromServer(): void {
    this.notificationsService.getNotifications(1, 30, false).subscribe({
      next: (items: NotificationDto[]) => {
        const role = this.authService.getUserRole();
        const mapped: AppNotification[] = items
          .map(dto => {
            const createdAt = new Date(dto.createdAt);
            const type = this.normalizeType(dto.type, dto.title, dto.actionUrl ?? undefined);
            return {
              id: dto.id,
              title: dto.title,
              message: dto.message,
              actionUrl: dto.actionUrl ?? undefined,
              type,
              isRead: dto.isRead,
              createdAt,
              timeLabel: this.formatRelativeTime(createdAt),
            };
          })
          .filter(n => !(role === UserRoles.Admin && (n.type === 'calendar' || n.actionUrl?.toLowerCase().includes('calendar'))));

        this.notifications.set(mapped);
        this.updateUnreadCount();
        this.checkAutoReadByUrl(this.router.url);
      },
      error: () => {
        // No bloquear UI si el servidor está temporalmente inaccesible
      },
    });
  }

  onNotificationClick(notif: AppNotification): void {
    if (!notif.isRead) {
      this.markSingleAsRead(notif.id);
    }

    if (notif.actionUrl) {
      let path = notif.actionUrl.trim();
      if (path.startsWith('/#')) {
        path = path.substring(2);
      } else if (path.startsWith('#')) {
        path = path.substring(1);
      }

      const role = this.authService.getUserRole();
      if (role === UserRoles.Admin && (notif.type === 'calendar' || path.toLowerCase().includes('calendar'))) {
        path = '/admin/dashboard';
      } else if (!path.startsWith('/') && !path.startsWith('http')) {
        const prefix =
          role === UserRoles.Professional
            ? '/pro/'
            : role === UserRoles.FamilyRepresentative
              ? '/family/'
              : '/admin/';
        path = prefix + path;
      }

      this.router.navigateByUrl(path);
    }
  }

  markAllAsRead(event?: Event): void {
    event?.stopPropagation();
    if (this.unreadCount() === 0) return;

    this.notifications.update(arr => arr.map(n => ({ ...n, isRead: true })));
    this.updateUnreadCount();

    this.notificationsService.markAllAsRead().subscribe({
      error: () => this.loadFromServer(),
    });
  }

  clearAll(event: Event): void {
    event.stopPropagation();
    if (this.notifications().length === 0) return;

    this.notifications.set([]);
    this.updateUnreadCount();

    this.notificationsService.dismissAll().subscribe({
      error: () => this.loadFromServer(),
    });
  }

  dismissNotification(notifId: string, event: Event): void {
    event.stopPropagation();

    this.notifications.update(arr => arr.filter(n => n.id !== notifId));
    this.updateUnreadCount();

    if (!notifId.startsWith('rt-')) {
      this.notificationsService.dismiss(notifId).subscribe({
        error: () => this.loadFromServer(),
      });
    }
  }

  getNotificationIcon(notif: AppNotification): string {
    switch (notif.type) {
      case 'message':    return 'cilEnvelopeClosed';
      case 'activity':   return 'cilCheckCircle';
      case 'calendar':   return 'cilCalendar';
      case 'report':     return 'cilNotes';
      case 'assignment': return 'cilUserFollow';
      case 'alert':      return 'cilWarning';
      default:           return 'cilInfo';
    }
  }

  getNotificationColorClass(notif: AppNotification): string {
    switch (notif.type) {
      case 'message':    return 'text-primary';
      case 'activity':   return 'text-success';
      case 'calendar':   return 'text-warning';
      case 'report':     return 'text-info';
      case 'assignment': return 'text-primary';
      case 'alert':      return 'text-danger';
      default:           return 'text-secondary';
    }
  }

  private markSingleAsRead(notifId: string): void {
    this.notifications.update(arr =>
      arr.map(n => (n.id === notifId ? { ...n, isRead: true } : n))
    );
    this.updateUnreadCount();

    if (!notifId.startsWith('rt-')) {
      this.notificationsService.markAsRead(notifId).subscribe({
        error: () => { /* non-critical */ },
      });
    }
  }

  private normalizeType(rawType?: string, title?: string, actionUrl?: string): NotificationType {
    const valid: NotificationType[] = ['calendar', 'activity', 'report', 'message', 'assignment', 'alert', 'system'];
    const normalized = rawType?.trim().toLowerCase() as NotificationType | undefined;
    if (normalized && valid.includes(normalized)) {
      return normalized;
    }

    const t = (title ?? '').toLowerCase();
    const u = (actionUrl ?? '').toLowerCase();
    if (t.includes('calendario') || t.includes('tutoría') || t.includes('reprogramado') || u.includes('calendar')) return 'calendar';
    if (t.includes('mensaje') || t.includes('respuesta') || u.includes('messages')) return 'message';
    if (t.includes('reporte') || u.includes('reports')) return 'report';
    if (t.includes('alerta') || t.includes('frustraci') || t.includes('bloqueada') || t.includes('suspendid')) return 'alert';
    if (t.includes('alumno asignado') || t.includes('transferid') || t.includes('desvinculad') || t.includes('vinculad')) return 'assignment';
    if (t.includes('actividad') || t.includes('nivel') || u.includes('activities') || u.includes('evaluations')) return 'activity';
    return 'system';
  }

  private normalizeUrl(url?: string): string {
    if (!url) return '';
    let normalized = url.trim();
    if (normalized.startsWith('/#')) {
      normalized = normalized.substring(2);
    } else if (normalized.startsWith('#')) {
      normalized = normalized.substring(1);
    }
    normalized = normalized.split('?')[0].split('#')[0];
    if (!normalized.startsWith('/')) {
      normalized = '/' + normalized;
    }
    if (normalized.length > 1 && normalized.endsWith('/')) {
      normalized = normalized.slice(0, -1);
    }
    return normalized.toLowerCase();
  }

  private checkAutoReadByUrl(url: string): void {
    const currentPath = this.normalizeUrl(url);
    if (!currentPath) return;

    const toMark: string[] = [];
    for (const n of this.notifications()) {
      if (n.isRead) continue;
      const notifPath = this.normalizeUrl(n.actionUrl);
      if (notifPath && notifPath === currentPath) {
        toMark.push(n.id);
      }
    }

    if (toMark.length > 0) {
      toMark.forEach(id => this.markSingleAsRead(id));
    }
  }

  private updateUnreadCount(): void {
    const unread = this.notifications().filter(n => !n.isRead).length;
    this.unreadCount.set(unread);
  }

  private formatRelativeTime(date: Date): string {
    const diffMs = Date.now() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    if (diffMin < 1) return 'Ahora';
    if (diffMin < 60) return `Hace ${diffMin} min`;
    const diffHours = Math.floor(diffMin / 60);
    if (diffHours < 24) return `Hace ${diffHours} h`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays === 1) return 'Ayer';
    if (diffDays < 7) return `Hace ${diffDays} días`;
    return date.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  ngOnDestroy(): void {
    this.notifSub?.unsubscribe();
    this.routerSub?.unsubscribe();
    this.userSub?.unsubscribe();
    this.tickSub?.unsubscribe();
  }
}
