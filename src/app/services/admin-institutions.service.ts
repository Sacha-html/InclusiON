import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { AdminInstitutionResponse, ApiResponse } from '../models';
import { catchError, map, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AdminInstitutionsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/institutions-assignments`;
  }

  getMyInstitutions(): Observable<AdminInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<AdminInstitutionResponse[]>>(`${this.apiUrl}/me`)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  getByAdmin(adminUserId: string): Observable<AdminInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<AdminInstitutionResponse[]>>(`${this.apiUrl}/${adminUserId}`)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  assign(adminUserId: string, institutionId: number): Observable<AdminInstitutionResponse> {
    return this.http
      .post<ApiResponse<AdminInstitutionResponse>>(`${this.apiUrl}/${adminUserId}`, { institutionId })
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  remove(adminUserId: string, institutionId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${adminUserId}/${institutionId}`)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
