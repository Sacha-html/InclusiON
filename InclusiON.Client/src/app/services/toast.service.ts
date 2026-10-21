import { Injectable } from '@angular/core';
import { Toast } from '@models';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  toasts$ = new Subject<Toast>();
  private idCounter = 0;
  private lastToast: { message: string; color?: string; timestamp: number } | null = null;

  success(message: string, title: string = '¡Éxito!') {
    this.show({
      title,
      message,
      color: 'success',
      autohide: true,
      delay: 3000,
    });
  }

  error(message: string, title: string = 'Error') {
    this.show({
      title,
      message,
      color: 'danger',
      autohide: true,
      delay: 5000,
    });
  }

  warning(message: string, title: string = 'Advertencia', delay: number = 7000) {
    this.show({
      title,
      message,
      color: 'warning',
      autohide: true,
      delay,
    });
  }

  info(message: string, title: string = 'Información') {
    this.show({
      title,
      message,
      color: 'info',
      autohide: true,
      delay: 3000,
    });
  }

  show(toast: Toast) {
    const now = Date.now();
    if (
      this.lastToast &&
      this.lastToast.message === toast.message &&
      this.lastToast.color === toast.color &&
      now - this.lastToast.timestamp < 1500
    ) {
      return;
    }

    this.lastToast = { message: toast.message, color: toast.color, timestamp: now };
    this.toasts$.next({
      id: this.idCounter++,
      ...toast,
    });
  }
}
