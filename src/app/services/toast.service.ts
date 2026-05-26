import { Injectable } from '@angular/core';
import { Toast } from '@models';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  toasts$ = new Subject<Toast>();
  private idCounter = 0;

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

  warning(message: string, title: string = 'Advertencia') {
    this.show({
      title,
      message,
      color: 'warning',
      autohide: true,
      delay: 4000,
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
    this.toasts$.next({
      id: this.idCounter++,
      ...toast,
    });
  }
}
