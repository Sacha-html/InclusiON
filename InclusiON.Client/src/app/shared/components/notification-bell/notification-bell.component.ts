import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription, filter, interval, switchMap } from 'rxjs';
import { IconDirective } from '@coreui/icons-angular';
import {
  DropdownComponent,
  DropdownToggleDirective,
  DropdownMenuDirective,
  DropdownItemDirective,
  TooltipDirective
} from '@coreui/angular';
import { MessagesService } from '@services/messages.service';
import { SignalrService, SignalRNotification } from '@services/signalr.service';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { ProfessionalsService } from '@services/professionals.service';
import { AssignmentsService } from '@services/assignments.service';
import { UserRoles } from '@shared/constants/roles';

export interface AppNotification {
  id: string;
  title: string;
  message: string;
  actionUrl?: string;
  type: 'message' | 'activity' | 'calendar' | 'system';
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
  private readonly messagesService      = inject(MessagesService);
  private readonly signalrService       = inject(SignalrService);
  private readonly authService          = inject(AuthService);
  private readonly router               = inject(Router);
  private readonly toastService         = inject(ToastService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService   = inject(AssignmentsService);

  readonly notifications = signal<AppNotification[]>([]);
  readonly unreadCount = signal(0);

  private sub?: Subscription;
  private routerSub?: Subscription;
  private pollSub?: Subscription;

  readonly unreadLabel = () => {
    const n = this.unreadCount();
    return n === 0
      ? 'Notificaciones — sin notificaciones no leídas'
      : `Notificaciones — ${n} sin leer`;
  };

  ngOnInit(): void {
    this.loadFromStorage();
    this.fetchCount();

    // Sincronización periódica en segundo plano cada 20 segundos
    this.pollSub = interval(20000).subscribe(() => {
      this.fetchCount();
    });

    // Auto-marcar como leídas según la ruta actual (ej: al estar o entrar al perfil de un alumno)
    this.routerSub = this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.checkAutoReadByUrl(event.urlAfterRedirects || event.url);
    });
    this.checkAutoReadByUrl(this.router.url);

    // Increment badge and append notification on each real-time push notification
    this.sub = this.signalrService.notification$.subscribe((data: SignalRNotification) => {
      const role = this.authService.getUserRole();
      // Bloquear notificaciones de calendario para el Administrador
      if (role === UserRoles.Admin) {
        const titleLower = data.title?.toLowerCase() ?? '';
        const urlLower = data.actionUrl?.toLowerCase() ?? '';
        if (titleLower.includes('calendario') || urlLower.includes('calendar')) {
          return; // Ignorar notificación de calendario para Admin
        }
      }

      const type = this.detectNotificationType(data.title, data.actionUrl);
      const newNotif: AppNotification = {
        id: Date.now().toString() + '-' + Math.floor(Math.random() * 1000),
        title: data.title,
        message: data.message,
        actionUrl: data.actionUrl,
        type: type,
        isRead: false,
        createdAt: new Date(),
        timeLabel: 'Ahora'
      };

      this.notifications.update(arr => [newNotif, ...arr]);
      this.saveToStorage();
      this.updateUnreadCount();
      this.checkAutoReadByUrl(this.router.url);
    });
  }

  private extractStudentKeyFromMessage(message?: string): string | null {
    if (!message) return null;
    const match = message.match(/alumno,\s*([^.]+)\./i);
    if (match && match[1]) {
      return match[1]
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .trim()
        .replace(/[^a-z0-9]/g, '-');
    }
    return null;
  }

  private recordReadNotification(notif: AppNotification): void {
    this.recordReadId(notif.id);
    const studentKey = this.extractStudentKeyFromMessage(notif.message);
    if (studentKey) {
      this.recordReadId(`assign-${studentKey}`);
    }
  }

  private recordDismissedNotification(notif: AppNotification): void {
    this.recordDismissedId(notif.id);
    this.recordReadNotification(notif);
    const studentKey = this.extractStudentKeyFromMessage(notif.message);
    if (studentKey) {
      this.recordDismissedId(`assign-${studentKey}`);
    }
  }

  onNotificationClick(notif: AppNotification): void {
    if (!notif.isRead) {
      this.recordReadNotification(notif);
      this.notifications.update(arr => arr.map(n => n.id === notif.id ? { ...n, isRead: true } : n));
      this.saveToStorage();
      this.updateUnreadCount();
    }

    if (notif.actionUrl) {
      let path = notif.actionUrl;
      if (path.startsWith('/#')) {
        path = path.substring(2);
      } else if (path.startsWith('#')) {
        path = path.substring(1);
      }
      const role = this.authService.getUserRole();
      
      // Si el Administrador recibe por error una notificación de calendario, llevarlo a dashboard
      if (role === UserRoles.Admin && (notif.type === 'calendar' || path.includes('calendar'))) {
        path = '/admin/dashboard';
      } else if (!path.startsWith('/') && !path.startsWith('http')) {
        const prefix = role === UserRoles.Professional ? '/pro/' : (role === UserRoles.FamilyRepresentative ? '/family/' : '/admin/');
        path = prefix + path;
      }
      this.router.navigateByUrl(path);
    }
  }

  markAllAsRead(event?: Event): void {
    event?.stopPropagation();
    this.notifications.update(arr => arr.map(n => {
      this.recordReadNotification(n);
      return { ...n, isRead: true };
    }));
    this.saveToStorage();
    this.updateUnreadCount();
  }

  clearAll(event: Event): void {
    event.stopPropagation();
    const dismissed = this.getDismissedIds();
    this.notifications().forEach(n => {
      this.recordDismissedNotification(n);
      dismissed.add(n.id);
    });
    try {
      localStorage.setItem(this.getDismissedIdsKey(), JSON.stringify(Array.from(dismissed)));
    } catch {
      // ignore
    }
    this.notifications.set([]);
    this.saveToStorage();
    this.updateUnreadCount();
  }

  dismissNotification(notifId: string, event: Event): void {
    event.stopPropagation();
    const target = this.notifications().find(n => n.id === notifId);
    if (target) {
      this.recordDismissedNotification(target);
    } else {
      this.recordReadId(notifId);
      this.recordDismissedId(notifId);
    }
    this.notifications.update(arr => arr.filter(n => n.id !== notifId));
    this.saveToStorage();
    this.updateUnreadCount();
  }

  getNotificationIcon(notif: AppNotification): string {
    switch (notif.type) {
      case 'message':  return 'cilEnvelopeClosed';
      case 'activity': return 'cilCheckCircle';
      case 'calendar': return 'cilCalendar';
      default:         return 'cilInfo';
    }
  }

  getNotificationColorClass(notif: AppNotification): string {
    switch (notif.type) {
      case 'message':  return 'text-primary';
      case 'activity': return 'text-success';
      case 'calendar': return 'text-warning';
      default:         return 'text-info';
    }
  }

  private detectNotificationType(title: string, actionUrl?: string): 'message' | 'activity' | 'calendar' | 'system' {
    const t = title.toLowerCase();
    const url = actionUrl?.toLowerCase() ?? '';
    if (t.includes('mensaje') || url.includes('messages')) return 'message';
    if (t.includes('actividad') || url.includes('persons') || url.includes('classroom') || url.includes('evaluations') || url.includes('activities')) return 'activity';
    if (t.includes('calendario') || url.includes('calendar')) return 'calendar';
    return 'system';
  }

  private getEffectiveUserId(): string {
    const user = this.authService.getCurrentUser();
    if (user?.email) return user.email.toLowerCase().trim();
    const tokenUser = this.authService.getUserFromToken();
    if (tokenUser?.email) return tokenUser.email.toLowerCase().trim();
    return this.authService.getUserRole() ?? 'default_user';
  }

  private getStorageKey(): string {
    return `app_notifications_${this.getEffectiveUserId()}`;
  }

  private getReadIdsKey(): string {
    return `app_notifications_read_${this.getEffectiveUserId()}`;
  }

  private getDismissedIdsKey(): string {
    return `app_notifications_dismissed_${this.getEffectiveUserId()}`;
  }

  private getLastAssignmentSyncKey(): string {
    return `app_last_assignment_sync_${this.getEffectiveUserId()}`;
  }

  private getReadIds(): Set<string> {
    try {
      const stored = localStorage.getItem(this.getReadIdsKey());
      if (stored) {
        return new Set<string>(JSON.parse(stored));
      }
      // Buscar en keys previas para migración automática transparente
      for (let i = 0; i < localStorage.length; i++) {
        const k = localStorage.key(i);
        if (k && k.startsWith('app_notifications_read_')) {
          const oldData = localStorage.getItem(k);
          if (oldData) {
            const arr = JSON.parse(oldData);
            if (Array.isArray(arr) && arr.length > 0) {
              const s = new Set<string>(arr);
              localStorage.setItem(this.getReadIdsKey(), JSON.stringify(arr));
              return s;
            }
          }
        }
      }
    } catch {
      // ignore
    }
    return new Set<string>();
  }

  private recordReadId(id: string): void {
    const set = this.getReadIds();
    set.add(id);
    try {
      localStorage.setItem(this.getReadIdsKey(), JSON.stringify(Array.from(set)));
    } catch {
      // ignore
    }
  }

  private getDismissedIds(): Set<string> {
    try {
      const stored = localStorage.getItem(this.getDismissedIdsKey());
      if (stored) {
        return new Set<string>(JSON.parse(stored));
      }
      // Buscar en keys previas para migración automática transparente
      for (let i = 0; i < localStorage.length; i++) {
        const k = localStorage.key(i);
        if (k && k.startsWith('app_notifications_dismissed_')) {
          const oldData = localStorage.getItem(k);
          if (oldData) {
            const arr = JSON.parse(oldData);
            if (Array.isArray(arr) && arr.length > 0) {
              const s = new Set<string>(arr);
              localStorage.setItem(this.getDismissedIdsKey(), JSON.stringify(arr));
              return s;
            }
          }
        }
      }
    } catch {
      // ignore
    }
    return new Set<string>();
  }

  private recordDismissedId(id: string): void {
    const set = this.getDismissedIds();
    set.add(id);
    try {
      localStorage.setItem(this.getDismissedIdsKey(), JSON.stringify(Array.from(set)));
    } catch {
      // ignore
    }
  }

  private normalizeUrl(url: string | undefined): string {
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
    if (!url) return;
    const currentPath = this.normalizeUrl(url);
    if (!currentPath) return;

    // Detectar si estamos en el detalle de un alumno (/pro/persons/:id)
    const personMatch = currentPath.match(/\/persons\/([a-z0-9-]+)/);
    const personId = personMatch ? personMatch[1] : null;

    // Detectar si estamos en la lista de alumnos / perfiles (/pro/persons)
    const isPersonsList = currentPath === '/pro/persons' || currentPath.endsWith('/persons');

    // Detectar si estamos en mensajes
    const isMessages = currentPath.includes('/messages');

    let changed = false;
    this.notifications.update(arr =>
      arr.map(n => {
        if (n.isRead) return n;

        const notifPath = this.normalizeUrl(n.actionUrl);
        let shouldMarkRead = false;

        // 1. Coincidencia de ruta exacta
        if (notifPath && currentPath === notifPath) {
          shouldMarkRead = true;
        }

        // 2. Detalle de alumno específico
        if (personId && (n.id.includes(personId) || notifPath.includes(personId))) {
          shouldMarkRead = true;
        }

        // 3. Vista de lista de perfiles / aula
        if (isPersonsList && (notifPath === '/pro/persons' || notifPath.endsWith('/persons') || n.id.startsWith('assign-'))) {
          shouldMarkRead = true;
        }

        // 4. Vista de mensajes
        if (isMessages && (n.type === 'message' || notifPath.includes('messages') || n.id === 'unread-messages-summary')) {
          shouldMarkRead = true;
        }

        if (shouldMarkRead) {
          changed = true;
          this.recordReadNotification(n);
          return { ...n, isRead: true };
        }

        return n;
      })
    );

    if (changed) {
      this.saveToStorage();
      this.updateUnreadCount();
    }
  }

  private loadFromStorage(): void {
    const storageKey = this.getStorageKey();
    let stored = localStorage.getItem(storageKey);
    if (!stored) {
      const roleKey = `app_notifications_${this.authService.getUserRole() ?? 'guest'}`;
      stored = localStorage.getItem(roleKey) || localStorage.getItem('app_notifications');
      if (!stored) {
        for (let i = 0; i < localStorage.length; i++) {
          const k = localStorage.key(i);
          if (k && k.startsWith('app_notifications_') && !k.includes('_read_') && !k.includes('_dismissed_') && !k.includes('_sync_')) {
            stored = localStorage.getItem(k);
            if (stored) break;
          }
        }
      }
    }
    const readIds = this.getReadIds();
    const dismissedIds = this.getDismissedIds();
    if (stored) {
      try {
        let parsed = JSON.parse(stored) as AppNotification[];
        // Filtrar notificaciones descartadas y seed demos
        parsed = parsed.filter(n => {
          if (dismissedIds.has(n.id) || n.id.startsWith('seed-')) return false;
          const studentKey = this.extractStudentKeyFromMessage(n.message);
          if (studentKey && dismissedIds.has(`assign-${studentKey}`)) return false;
          return true;
        });

        // Deduplicar agresivamente cualquier acumulación previa por título/mensaje
        const seen = new Set<string>();
        const deduplicated: AppNotification[] = [];
        for (const n of parsed) {
          const studentKey = this.extractStudentKeyFromMessage(n.message);
          const dedupKey = studentKey 
            ? `assign:${studentKey}` 
            : `${n.type || 'sys'}:${(n.title || '').trim().toLowerCase()}:${(n.message || '').trim().toLowerCase()}`;

          if (!seen.has(dedupKey)) {
            seen.add(dedupKey);
            n.createdAt = new Date(n.createdAt);
            if (readIds.has(n.id) || (studentKey && readIds.has(`assign-${studentKey}`))) {
              n.isRead = true;
            }
            deduplicated.push(n);
          } else if (n.isRead) {
            const first = deduplicated.find(x => {
              const k = this.extractStudentKeyFromMessage(x.message);
              return k ? `assign:${k}` === dedupKey : `${x.type || 'sys'}:${(x.title || '').trim().toLowerCase()}:${(x.message || '').trim().toLowerCase()}` === dedupKey;
            });
            if (first) first.isRead = true;
          }
        }

        const role = this.authService.getUserRole();
        let filtered = deduplicated;
        if (role === UserRoles.Admin) {
          filtered = filtered.filter(n => n.type !== 'calendar' && !n.actionUrl?.includes('calendar'));
        }
        this.notifications.set(filtered);
        this.saveToStorage();
        this.updateUnreadCount();
        return;
      } catch {
        // Fallback
      }
    }

    this.notifications.set([]);
    this.updateUnreadCount();
  }

  private saveToStorage(): void {
    const list = this.notifications().slice(0, 30);
    localStorage.setItem(this.getStorageKey(), JSON.stringify(list));
  }

  private updateUnreadCount(): void {
    const unread = this.notifications().filter(n => !n.isRead).length;
    this.unreadCount.set(unread);
  }

  private fetchCount(): void {
    this.messagesService.getUnreadCount().subscribe({
      next: (n) => {
        const readIds = this.getReadIds();
        const dismissedIds = this.getDismissedIds();
        if (n > 0) {
          const list = this.notifications();
          const hasMsgNotif = list.some(notif => notif.id === 'unread-messages-summary' && !notif.isRead);
          const isAlreadyRead = readIds.has('unread-messages-summary');
          const isDismissed = dismissedIds.has('unread-messages-summary');

          if (!hasMsgNotif && !isAlreadyRead && !isDismissed) {
            const role = this.authService.getUserRole();
            const actionUrl = role === UserRoles.Professional ? '/pro/messages' : role === UserRoles.FamilyRepresentative ? '/family/messages' : '/admin/messages';
            const msgNotif: AppNotification = {
              id: 'unread-messages-summary',
              title: 'Mensajes sin leer',
              message: `Tenés ${n} mensaje(s) sin leer en tu bandeja de entrada.`,
              actionUrl: actionUrl,
              type: 'message',
              isRead: false,
              createdAt: new Date(),
              timeLabel: 'Ahora'
            };
            this.notifications.update(arr => [msgNotif, ...arr.filter(x => x.id !== 'unread-messages-summary')]);
            this.saveToStorage();
          }
        } else {
          this.notifications.update(arr => arr.map(x => x.id === 'unread-messages-summary' ? { ...x, isRead: true } : x));
          this.saveToStorage();
        }
        this.updateUnreadCount();
        this.checkAutoReadByUrl(this.router.url);
      },
      error: ()  => { /* non-critical, skip */ },
    });

    // Para el rol Profesional: sincronizar asignaciones recientes si estaba desconectado
    if (this.authService.getUserRole() === UserRoles.Professional) {
      this.professionalsService.getMyProfile().pipe(
        switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
      ).subscribe({
        next: (persons) => {
          if (!persons || persons.length === 0) return;
          const current = this.notifications();
          const readIds = this.getReadIds();
          const dismissedIds = this.getDismissedIds();
          const syncKey = this.getLastAssignmentSyncKey();
          const lastSyncStr = localStorage.getItem(syncKey);

          // Si es la primera vez que carga (ej: inicio con base de datos ya poblada o seeded),
          // fijamos la marca de tiempo a 'ahora' para no inundar con 40 notificaciones viejas.
          if (!lastSyncStr) {
            localStorage.setItem(syncKey, new Date().toISOString());
            return;
          }

          const lastSyncDate = new Date(lastSyncStr);
          let changed = false;
          const newNotifications: AppNotification[] = [];

          for (const p of persons) {
            const assignedDate = new Date(p.assignedAt);
            // Solo considerar asignaciones posteriores a la última sincronización
            if (assignedDate > lastSyncDate) {
              const studentKey = (p.personFullName || p.personFirstName || 'alumno')
                .toLowerCase()
                .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
                .trim()
                .replace(/[^a-z0-9]/g, '-');
              const notifId = `assign-${studentKey}`;

              if (dismissedIds.has(notifId) || dismissedIds.has(p.personId) || dismissedIds.has(`assign-${p.personId}`)) {
                continue;
              }

              const isAlreadyRead = readIds.has(notifId) || readIds.has(p.personId) || readIds.has(`assign-${p.personId}`);
              const existingNotif = current.find(n =>
                n.id === notifId ||
                n.id === `assign-${p.personId}` ||
                (n.message && n.message.toLowerCase().includes(p.personFullName.toLowerCase()))
              );

              if (existingNotif) {
                existingNotif.actionUrl = `/pro/persons/${p.personId}`;
                if (isAlreadyRead && !existingNotif.isRead) {
                  existingNotif.isRead = true;
                  changed = true;
                }
              } else if (!isAlreadyRead) {
                newNotifications.push({
                  id: notifId,
                  title: '🎓 Nuevo alumno asignado',
                  message: `Tienes un nuevo alumno, ${p.personFullName}. Llená el perfil funcional.`,
                  actionUrl: `/pro/persons/${p.personId}`,
                  type: 'activity',
                  isRead: false,
                  createdAt: assignedDate,
                  timeLabel: 'Reciente'
                });
                changed = true;
              }
            }
          }

          localStorage.setItem(syncKey, new Date().toISOString());

          if (changed) {
            this.notifications.update(list => [...newNotifications, ...list]);
            this.saveToStorage();
            this.updateUnreadCount();
          }
          this.checkAutoReadByUrl(this.router.url);
        },
        error: () => { /* non-critical, skip */ }
      });
    }
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.routerSub?.unsubscribe();
    this.pollSub?.unsubscribe();
  }
}
