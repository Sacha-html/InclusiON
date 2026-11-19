import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '@env';
import { ApiResponse } from '@models';
import { unwrapResponse } from '@shared/utils';

export type NotificationType =
  | 'calendar'
  | 'activity'
  | 'report'
  | 'message'
  | 'assignment'
  | 'alert'
  | 'system';

export interface NotificationDto {
  id: string;
  type: NotificationType | string;
  eventCode: string;
  title: string;
  message: string;
  actionUrl?: string | null;
  isRead: boolean;
  readAt?: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return `${environment.apiUrl}/notifications`;
  }

  getNotifications(page = 1, pageSize = 30, onlyUnread = false): Observable<NotificationDto[]> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('onlyUnread', onlyUnread);

    return this.http
      .get<ApiResponse<NotificationDto[]>>(this.baseUrl, { params })
      .pipe(unwrapResponse());
  }

  getUnreadCount(): Observable<number> {
    return this.http
      .get<ApiResponse<{ count: number }>>(`${this.baseUrl}/unread-count`)
      .pipe(
        unwrapResponse(),
        map((res) => res?.count ?? 0)
      );
  }

  markAsRead(id: string): Observable<unknown> {
    return this.http
      .patch<ApiResponse<unknown>>(`${this.baseUrl}/${encodeURIComponent(id)}/read`, {})
      .pipe(unwrapResponse());
  }

  markAllAsRead(): Observable<unknown> {
    return this.http
      .patch<ApiResponse<unknown>>(`${this.baseUrl}/read-all`, {})
      .pipe(unwrapResponse());
  }

  dismiss(id: string): Observable<unknown> {
    return this.http
      .delete<ApiResponse<unknown>>(`${this.baseUrl}/${encodeURIComponent(id)}`)
      .pipe(unwrapResponse());
  }

  dismissAll(): Observable<unknown> {
    return this.http
      .delete<ApiResponse<unknown>>(this.baseUrl)
      .pipe(unwrapResponse());
  }

  /**
   * Elimina claves legacy de notificaciones en localStorage para sanear navegadores
   * afectados por la migración cruzada entre usuarios previa.
   */
  purgeLegacyStorage(): void {
    try {
      const keysToRemove: string[] = [];
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (!key) continue;
        if (
          key === 'app_notifications' ||
          key.startsWith('app_notifications_') ||
          key.startsWith('app_last_assignment_sync_') ||
          key === 'inclusion_calendar_events'
        ) {
          keysToRemove.push(key);
        }
      }
      keysToRemove.forEach((k) => localStorage.removeItem(k));
    } catch {
      // ignore storage errors
    }
  }
}
