import { inject, Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';
import { environment } from '@env';

export interface SignalRNotification {
  title: string;
  message: string;
  actionUrl?: string;
}

@Injectable({ providedIn: 'root' })
export class SignalrService implements OnDestroy {
  readonly #auth = inject(AuthService);
  readonly #toast = inject(ToastService);

  #connection: HubConnection | null = null;

  /** Emits each time a 'Notification' push is received from the hub. */
  readonly notification$ = new Subject<SignalRNotification>();

  start(): void {
    if (this.#connection) return;

    const token = this.#auth.getToken();
    if (!token) return;

    const hubUrl = environment.apiUrl.replace('/api', '') + '/hubs/notifications';

    this.#connection = new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.#connection.on('Notification', (data: SignalRNotification) => {
      this.#toast.info(data.message, data.title);
      this.notification$.next(data);
    });

    this.#connection.start().catch(() => this.#connection = null);
  }

  stop(): void {
    this.#connection?.stop();
    this.#connection = null;
  }

  ngOnDestroy(): void {
    this.stop();
  }
}
