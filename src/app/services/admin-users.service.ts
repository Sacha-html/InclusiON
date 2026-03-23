import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { AdminUserResponse, ApiResponse, CreateAdminUserResponse } from '../models';
import { catchError, map, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AdminUsersService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/institutions-assignments`;
  }

  getAdmins(): Observable<AdminUserResponse[]> {
    return this.http
      .get<ApiResponse<AdminUserResponse[]>>(`${this.apiUrl}/admins`)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  createAdmin(request: {
    email: string;
    firstName: string;
    lastName: string;
    institutionId: number;
  }): Observable<CreateAdminUserResponse> {
    return this.http
      .post<ApiResponse<CreateAdminUserResponse>>(`${this.apiUrl}/users`, request)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
