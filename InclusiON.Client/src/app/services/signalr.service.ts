import { inject, Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';
import { environment } from '@env';
import { UserRoles } from '@shared/constants/roles';

export interface SignalRNotification {
  id?: string;
  type?: string;
  eventCode?: string;
  title: string;
  message: string;
  actionUrl?: string;
  isRead?: boolean;
  createdAt?: string;
}

@Injectable({ providedIn: 'root' })
export class SignalrService implements OnDestroy {
  readonly #auth = inject(AuthService);
  readonly #toast = inject(ToastService);

  #connection: HubConnection | null = null;
  #connectedUserId: string | null = null;

  /** Emits each time a 'Notification' push is received from the hub. */
  readonly notification$ = new Subject<SignalRNotification>();

  start(): void {
    const token = this.#auth.getToken();
    if (!token) {
      this.stop();
      return;
    }

    const userId = this.#resolveCurrentUserId();

    // Si ya hay conexión para el mismo usuario, no reconectar
    if (this.#connection && this.#connectedUserId === userId) return;

    // Si cambió el usuario autenticado, cerrar la conexión previa antes de abrir una nueva
    if (this.#connection && this.#connectedUserId !== userId) {
      this.stop();
    }

    const hubUrl = environment.apiUrl.replace('/api', '') + '/hubs/notifications';
    this.#connectedUserId = userId;

    const conn = new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => this.#auth.getToken() ?? '' })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.#connection = conn;

    conn.on('Notification', (data: SignalRNotification) => {
      // Defensa en profundidad IN-217: nunca mostrar notificaciones de calendario a Admin
      if (this.#auth.getUserRole() === UserRoles.Admin) {
        const typeLower = data.type?.toLowerCase() ?? '';
        const titleLower = data.title?.toLowerCase() ?? '';
        const urlLower = data.actionUrl?.toLowerCase() ?? '';
        if (typeLower === 'calendar' || titleLower.includes('calendario') || urlLower.includes('calendar')) {
          return;
        }
      }

      this.#toast.info(data.message, data.title);
      this.notification$.next(data);
    });

    conn.start().catch(() => {
      if (this.#connection === conn) {
        this.#connection = null;
        this.#connectedUserId = null;
      }
    });
  }

  stop(): void {
    const conn = this.#connection;
    this.#connection = null;
    this.#connectedUserId = null;
    conn?.stop().catch(() => { /* ignore */ });
  }

  ngOnDestroy(): void {
    this.stop();
  }

  #resolveCurrentUserId(): string | null {
    return this.#auth.getCurrentUser()?.id ?? this.#auth.getUserFromToken()?.id ?? null;
  }
}
