import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { Observable } from 'rxjs';
import { ApiResponse } from '@models';
import { unwrapResponse } from '@shared/utils';

export interface CalendarEvent {
  id: string;
  title: string;
  type: 'Consulta' | 'Tutoría' | 'Clase' | 'Tarea';
  date: string; // YYYY-MM-DD
  time: string; // HH:MM
  description?: string;
  studentName?: string;
  createdBy?: string;
  targetScope?: 'all' | 'single';
  studentId?: string;
}

@Injectable({
  providedIn: 'root',
})
export class CalendarService {
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return environment.apiUrl;
  }

  getEvents(): Observable<CalendarEvent[]> {
    return this.http
      .get<ApiResponse<CalendarEvent[]>>(`${this.baseUrl}/calendar`)
      .pipe(unwrapResponse());
  }

  saveEvent(event: any): Observable<CalendarEvent> {
    return this.http
      .post<ApiResponse<CalendarEvent>>(`${this.baseUrl}/calendar`, event)
      .pipe(unwrapResponse());
  }

  deleteEvent(id: string): Observable<any> {
    return this.http
      .delete<ApiResponse<any>>(`${this.baseUrl}/calendar/${id}`)
      .pipe(unwrapResponse());
  }
}
