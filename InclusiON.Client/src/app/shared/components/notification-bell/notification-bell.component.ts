import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription, switchMap } from 'rxjs';
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
                    @if (!notif.isRead) {
                      <span class="unread-dot" title="No leído"></span>
                    }
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

  readonly unreadLabel = () => {
    const n = this.unreadCount();
    return n === 0
      ? 'Notificaciones — sin notificaciones no leídas'
      : `Notificaciones — ${n} sin leer`;
  };

  ngOnInit(): void {
    this.loadFromStorage();
    this.fetchCount();

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
      this.toastService.info(data.message, data.title);
    });
  }

  onNotificationClick(notif: AppNotification): void {
    if (!notif.isRead) {
      this.notifications.update(arr => arr.map(n => n.id === notif.id ? { ...n, isRead: true } : n));
      this.saveToStorage();
      this.updateUnreadCount();
    }

    if (notif.actionUrl) {
      let path = notif.actionUrl;
      if (path.startsWith('/#')) {
        path = path.substring(2);
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
    this.notifications.update(arr => arr.map(n => ({ ...n, isRead: true })));
    this.saveToStorage();
    this.updateUnreadCount();
  }

  clearAll(event: Event): void {
    event.stopPropagation();
    this.notifications.set([]);
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

  private getStorageKey(): string {
    const user = this.authService.getCurrentUser();
    const id = user?.id ?? this.authService.getUserRole() ?? 'guest';
    return `app_notifications_${id}`;
  }

  private loadFromStorage(): void {
    const storageKey = this.getStorageKey();
    const stored = localStorage.getItem(storageKey);
    if (stored) {
      try {
        let parsed = JSON.parse(stored) as AppNotification[];
        parsed.forEach(n => n.createdAt = new Date(n.createdAt));
        const role = this.authService.getUserRole();
        if (role === UserRoles.Admin) {
          parsed = parsed.filter(n => n.type !== 'calendar' && !n.actionUrl?.includes('calendar'));
        }
        this.notifications.set(parsed);
        this.updateUnreadCount();
        return;
      } catch {
        // Fallback
      }
    }

    this.seedInitialNotifications();
  }

  private saveToStorage(): void {
    const list = this.notifications().slice(0, 30);
    localStorage.setItem(this.getStorageKey(), JSON.stringify(list));
  }

  private updateUnreadCount(): void {
    const unread = this.notifications().filter(n => !n.isRead).length;
    this.unreadCount.set(unread);
  }

  private seedInitialNotifications(): void {
    const role = this.authService.getUserRole();
    const list: AppNotification[] = [];

    if (role === UserRoles.Professional) {
      list.push(
        {
          id: 'seed-msg',
          title: 'Nuevo mensaje',
          message: 'Tenés un nuevo mensaje de Miguel Fernández (Tutor).',
          actionUrl: 'messages',
          type: 'message',
          isRead: false,
          createdAt: new Date(Date.now() - 5 * 60000),
          timeLabel: 'Hace 5 min'
        },
        {
          id: 'seed-act',
          title: 'Actividad completada',
          message: 'Tomás Pérez completó la actividad \'Mi rutina visual\' con 90% de éxito.',
          actionUrl: 'evaluations',
          type: 'activity',
          isRead: false,
          createdAt: new Date(Date.now() - 15 * 60000),
          timeLabel: 'Hace 15 min'
        },
        {
          id: 'seed-cal',
          title: 'Recordatorio del calendario',
          message: 'Recordatorio: Sesión de terapia con Sofía Rodríguez mañana a las 10:00.',
          actionUrl: 'calendar',
          type: 'calendar',
          isRead: true,
          createdAt: new Date(Date.now() - 60 * 60000),
          timeLabel: 'Hace 1 hora'
        }
      );
    } else if (role === UserRoles.FamilyRepresentative) {
      list.push(
        {
          id: 'seed-msg',
          title: 'Nuevo mensaje',
          message: 'Tenés un nuevo mensaje de Pedro Martínez (Terapeuta).',
          actionUrl: 'messages',
          type: 'message',
          isRead: false,
          createdAt: new Date(Date.now() - 5 * 60000),
          timeLabel: 'Hace 5 min'
        },
        {
          id: 'seed-act',
          title: 'Actividad asignada',
          message: 'Pedro Martínez asignó una nueva actividad a Tomás Pérez: \'Concepto Muchos / Pocos\'.',
          actionUrl: 'activities',
          type: 'activity',
          isRead: false,
          createdAt: new Date(Date.now() - 15 * 60000),
          timeLabel: 'Hace 15 min'
        },
        {
          id: 'seed-cal',
          title: 'Recordatorio del calendario',
          message: 'Recordatorio: Sesión de terapia para Tomás Pérez mañana a las 10:00.',
          actionUrl: 'calendar',
          type: 'calendar',
          isRead: true,
          createdAt: new Date(Date.now() - 60 * 60000),
          timeLabel: 'Hace 1 hora'
        }
      );
    } else if (role === UserRoles.Admin) {
      list.push(
        {
          id: 'seed-admin-msg',
          title: 'Nuevo mensaje',
          message: 'Tenés un nuevo mensaje de Pedro Martínez (Profesional).',
          actionUrl: 'messages',
          type: 'message',
          isRead: false,
          createdAt: new Date(Date.now() - 5 * 60000),
          timeLabel: 'Hace 5 min'
        },
        {
          id: 'seed-msg',
          title: 'Nuevo profesional',
          message: 'La profesional Laura González se ha registrado y está pendiente de aprobación.',
          actionUrl: 'professionals',
          type: 'system',
          isRead: false,
          createdAt: new Date(Date.now() - 10 * 60000),
          timeLabel: 'Hace 10 min'
        },
        {
          id: 'seed-act',
          title: 'Reporte enviado',
          message: 'Se ha presentado un reporte semanal para evaluación.',
          actionUrl: 'reports',
          type: 'activity',
          isRead: true,
          createdAt: new Date(Date.now() - 30 * 60000),
          timeLabel: 'Hace 30 min'
        },
        {
          id: 'seed-sys',
          title: 'Mantenimiento del servidor',
          message: 'Recordatorio: Mantenimiento programado de la base de datos a las 23:00.',
          actionUrl: 'dashboard',
          type: 'system',
          isRead: true,
          createdAt: new Date(Date.now() - 120 * 60000),
          timeLabel: 'Hace 2 horas'
        }
      );
    }

    this.notifications.set(list);
    this.saveToStorage();
    this.updateUnreadCount();
  }

  private fetchCount(): void {
    this.messagesService.getUnreadCount().subscribe({
      next: (n) => {
        if (n > 0) {
          const list = this.notifications();
          const hasMsgNotif = list.some(notif => notif.id === 'unread-messages-summary' && !notif.isRead);
          if (!hasMsgNotif) {
            const role = this.authService.getUserRole();
            const actionUrl = role === UserRoles.Professional ? 'messages' : role === UserRoles.FamilyRepresentative ? 'messages' : 'messages';
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
          let changed = false;
          const cutoff = new Date();
          cutoff.setDate(cutoff.getDate() - 7);

          for (const p of persons) {
            const assignedDate = new Date(p.assignedAt);
            if (assignedDate >= cutoff) {
              const notifId = `assign-${p.personId}`;
              const exists = current.some(n => n.id === notifId || (n.actionUrl && n.actionUrl.includes(p.personId)));
              if (!exists) {
                const newNotif: AppNotification = {
                  id: notifId,
                  title: '🎓 Nuevo alumno asignado',
                  message: `Tienes un nuevo alumno, ${p.personFullName}. Llená el perfil funcional.`,
                  actionUrl: `/#/pro/persons/${p.personId}`,
                  type: 'activity',
                  isRead: false,
                  createdAt: assignedDate,
                  timeLabel: 'Reciente'
                };
                current.unshift(newNotif);
                changed = true;
              }
            }
          }
          if (changed) {
            this.notifications.set([...current]);
            this.saveToStorage();
            this.updateUnreadCount();
          }
        },
        error: () => { /* non-critical, skip */ }
      });
    }
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
